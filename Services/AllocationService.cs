using Microsoft.EntityFrameworkCore;
using Workwise.Mvc.Data;
using Workwise.Mvc.Models;

namespace Workwise.Mvc.Services;

public record AllocationCandidate(Guid EmployeeId, string Name, string Department,
    int SkillMatch, double CurrentPercent, double ProjectedPercent, bool Eligible, string Reason);

public class AllocationService(WorkwiseDb db)
{
    public async Task<IReadOnlyList<AllocationCandidate>> RecommendAsync(WorkItem task)
    {
        var people = await db.Employees.AsNoTracking().OrderBy(x => x.FullName).ToListAsync();
        var existing = await db.WorkItems.AsNoTracking()
            .Where(x => x.Status != TaskStates.Completed && x.AssignedEmployeeId != null && x.Id != task.Id)
            .ToListAsync();
        var required = SplitSkills(task.RequiredSkills);
        return people.Select(person =>
        {
            var known = SplitSkills(person.Skills);
            var matches = required.Count == 0 ? 100 :
                (int)Math.Round(100.0 * required.Count(x => known.Contains(x)) / required.Count);
            var committed = existing.Where(x => x.AssignedEmployeeId == person.Id).Sum(x => x.RemainingHours);
            var current = Math.Round(100 * committed / Math.Max(1, person.WeeklyHours), 1);
            var projected = Math.Round(100 * (committed + task.RemainingHours) / Math.Max(1, person.WeeklyHours), 1);
            var eligible = person.IsAvailable && matches == 100;
            var reason = !person.IsAvailable ? "Unavailable" :
                matches < 100 ? "Missing required skills" :
                projected > 100 ? "Skills matched, but workload exceeds weekly capacity" :
                "Skills matched and capacity available";
            return new AllocationCandidate(person.Id, person.FullName, person.Department, matches,
                current, projected, eligible, reason);
        }).OrderByDescending(x => x.Eligible)
          .ThenBy(x => x.ProjectedPercent > 100)
          .ThenByDescending(x => x.SkillMatch)
          .ThenBy(x => x.ProjectedPercent)
          .ToList();
    }
    private static HashSet<string> SplitSkills(string? text) =>
        (text ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
}

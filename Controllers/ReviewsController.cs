using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workwise.Mvc.Data;
using Workwise.Mvc.Models;

namespace Workwise.Mvc.Controllers;

[Authorize(Roles = Roles.Management)]
public class ReviewsController(WorkwiseDb db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.WorkItems.AsNoTracking()
        .Include(x => x.AssignedEmployee).Include(x => x.Engagement)
        .Where(x => x.Status == TaskStates.AwaitingReview)
        .OrderBy(x => x.DueDate).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Decide(Guid id, bool approve, string? feedback)
    {
        var task = await db.WorkItems.FindAsync(id);
        if (task is null) return NotFound();
        if (task.Status != TaskStates.AwaitingReview) return BadRequest("Task is not awaiting review.");
        if (feedback?.Length > 2000) return BadRequest("Feedback exceeds the allowed length.");
        if (!approve && string.IsNullOrWhiteSpace(feedback))
        {
            TempData["Error"] = "Provide a reason when returning work.";
            return RedirectToAction("Details", "Tasks", new { id });
        }
        task.Status = approve ? TaskStates.Completed : TaskStates.InProgress;
        task.Progress = approve ? 100 : Math.Min(90, task.Progress);
        task.UpdatedAt = DateTime.UtcNow;
        db.TaskUpdates.Add(new TaskUpdate { WorkItemId = task.Id,
            Author = User.Identity?.Name ?? "",
            Message = (approve ? "Approved and completed" : "Returned for changes") +
                (string.IsNullOrWhiteSpace(feedback) ? "." : ": " + feedback.Trim()) });
        db.Audit.Add(new AuditEntry { Actor = User.Identity?.Name ?? "",
            Action = approve ? "approve-task" : "return-task", Detail = task.Title });
        await db.SaveChangesAsync();
        TempData["Message"] = approve ? "Work approved and completed." : "Work returned to the employee.";
        return RedirectToAction(nameof(Index));
    }
}

using Microsoft.EntityFrameworkCore;
using Workwise.Web.Data;
namespace Workwise.Web.Services;
public record AllocationSuggestion(long EmployeeUserId,decimal Score,int CurrentAssignedMinutes);
public class TaskAllocationService(ApplicationDbContext db) {
 public async Task<List<AllocationSuggestion>> SuggestAsync(long taskId,CancellationToken ct=default) {
  var task=await db.Tasks.AsNoTracking().SingleAsync(x=>x.Id==taskId,ct);
  var requirements=await db.TaskSkillRequirements.Where(x=>x.TaskId==taskId).ToListAsync(ct);
  var employees=await db.EmployeeProfiles.AsNoTracking().ToListAsync(ct);
  var skills=await db.EmployeeSkills.AsNoTracking().ToListAsync(ct);
  var active=await (from a in db.TaskAssignments join t in db.Tasks on a.TaskId equals t.Id where a.UnassignedAt==null && t.Status!="Completed" select new {a.EmployeeUserId,t.EstimatedMinutes}).ToListAsync(ct);
  return employees.Select(e=>{var used=active.Where(a=>a.EmployeeUserId==e.UserId).Sum(a=>a.EstimatedMinutes); var eligible=requirements.All(r=>skills.Any(s=>s.EmployeeUserId==e.UserId&&s.SkillId==r.SkillId&&s.ProficiencyLevel>=r.MinimumLevel)); var skillScore=requirements.Count==0?1m:requirements.Sum(r=>skills.Where(s=>s.EmployeeUserId==e.UserId&&s.SkillId==r.SkillId).Select(s=>(decimal)s.ProficiencyLevel/5*r.ImportanceWeight).DefaultIfEmpty(0).Max())/Math.Max(1,requirements.Sum(r=>r.ImportanceWeight)); var capacity=Math.Max(1,e.WeeklyCapacityMinutes); return new {e.UserId,used,eligible,score=0.65m*skillScore+0.35m*Math.Max(0,1m-(decimal)used/capacity)};}).Where(x=>x.eligible).OrderByDescending(x=>x.score).Select(x=>new AllocationSuggestion(x.UserId,Math.Round(x.score*100,2),x.used)).ToList();
 }
}
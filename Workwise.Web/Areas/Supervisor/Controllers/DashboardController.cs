using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workwise.Web.Data;
using Workwise.Web.Models;
namespace Workwise.Web.Areas.Supervisor.Controllers;
[Area("Supervisor")][Authorize(Roles="Supervisor")]
public class DashboardController(ApplicationDbContext db,UserManager<ApplicationUser> users):Controller {
 public async Task<IActionResult> Index(CancellationToken ct){
  var id=long.Parse(users.GetUserId(User)!);
  var departments=db.Teams.Where(t=>t.SupervisorUserId==id).Select(t=>t.DepartmentId);
  var projects=db.Projects.Where(p=>departments.Contains(p.DepartmentId)).Select(p=>p.Id);
  var tasks=db.Tasks.Where(t=>projects.Contains(t.ProjectId)).Select(t=>t.Id);
  return View(new SupervisorDashboardViewModel(
   await db.EmployeeProfiles.CountAsync(e=>departments.Contains(e.DepartmentId),ct),
   await projects.CountAsync(ct),
   await db.Tasks.CountAsync(t=>tasks.Contains(t.Id)&&t.Status!="Completed",ct),
   await db.TaskAssignments.CountAsync(a=>tasks.Contains(a.TaskId)&&a.UnassignedAt==null,ct),
   await db.TaskSubmissions.CountAsync(s=>tasks.Contains(s.TaskId)&&!db.TaskReviews.Any(r=>r.SubmissionId==s.Id),ct)));
 }
}
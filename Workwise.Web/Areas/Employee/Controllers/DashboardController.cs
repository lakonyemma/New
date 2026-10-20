using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workwise.Web.Data;
using Workwise.Web.Models;
namespace Workwise.Web.Areas.Employee.Controllers;
[Area("Employee")][Authorize(Roles="Employee")]
public class DashboardController(ApplicationDbContext db,UserManager<ApplicationUser> users):Controller {
 public async Task<IActionResult> Index(CancellationToken ct){
  var id=long.Parse(users.GetUserId(User)!);
  return View(new EmployeeDashboardViewModel(
   await db.TaskAssignments.CountAsync(a=>a.EmployeeUserId==id&&a.UnassignedAt==null,ct),
   await db.TaskAssignments.CountAsync(a=>a.EmployeeUserId==id&&a.UnassignedAt==null&&db.Tasks.Any(t=>t.Id==a.TaskId&&t.Status=="Completed"),ct),
   await db.TaskSubmissions.CountAsync(s=>s.EmployeeUserId==id&&!db.TaskReviews.Any(r=>r.SubmissionId==s.Id),ct),
   await db.Notifications.CountAsync(n=>n.RecipientUserId==id&&n.ReadAt==null,ct)));
 }
}
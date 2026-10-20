using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workwise.Web.Data;
using Workwise.Web.Models;
namespace Workwise.Web.Areas.Employee.Controllers;
[Area("Employee")][Authorize(Roles="Employee")]
public class MyTasksController(ApplicationDbContext db,UserManager<ApplicationUser> users):Controller {
 public async Task<IActionResult> Index(CancellationToken ct){
  var userId=long.Parse(users.GetUserId(User)!);
  var tasks=await (from a in db.TaskAssignments join t in db.Tasks on a.TaskId equals t.Id where a.EmployeeUserId==userId&&a.UnassignedAt==null orderby t.DueAt select t).ToListAsync(ct);
  return View(tasks);
 }
 [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Submit(long taskId,string? note,CancellationToken ct){
  var userId=long.Parse(users.GetUserId(User)!);
  if(!await db.TaskAssignments.AnyAsync(a=>a.TaskId==taskId&&a.EmployeeUserId==userId&&a.UnassignedAt==null,ct))return Forbid();
  if(await db.TaskSubmissions.AnyAsync(s=>s.TaskId==taskId&&s.EmployeeUserId==userId,ct))return Conflict();
  db.TaskSubmissions.Add(new TaskSubmission{TaskId=taskId,EmployeeUserId=userId,SubmissionNote=(note??"").Trim()});
  await db.SaveChangesAsync(ct);return RedirectToAction(nameof(Index));
 }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workwise.Web.Data;
using Workwise.Web.Models;
using Workwise.Web.Services;
namespace Workwise.Web.Areas.Supervisor.Controllers;
[Area("Supervisor")][Authorize(Roles="Supervisor")]
public class TasksController(ApplicationDbContext db,UserManager<ApplicationUser> users,TaskAllocationService allocation) : Controller {
 public async Task<IActionResult> Index(CancellationToken ct) {
  var id=long.Parse(users.GetUserId(User)!);
  var teamIds=await db.Teams.Where(t=>t.SupervisorUserId==id).Select(t=>t.DepartmentId).ToListAsync(ct);
  var tasks=await (from t in db.Tasks join p in db.Projects on t.ProjectId equals p.Id where teamIds.Contains(p.DepartmentId) orderby t.Id descending select t).Take(100).ToListAsync(ct);
  return View(tasks);
 }
 [HttpGet] public async Task<IActionResult> Create(CancellationToken ct) {
  var id=long.Parse(users.GetUserId(User)!);
  ViewBag.Projects=await (from p in db.Projects join team in db.Teams on p.DepartmentId equals team.DepartmentId where team.SupervisorUserId==id select p).Distinct().ToListAsync(ct);
  return View(new TaskInput());
 }
 [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Create(TaskInput input,CancellationToken ct) {
  var id=long.Parse(users.GetUserId(User)!);
  var allowed=await (from p in db.Projects join team in db.Teams on p.DepartmentId equals team.DepartmentId where p.Id==input.ProjectId && team.SupervisorUserId==id select p.Id).AnyAsync(ct);
  if(!allowed) return Forbid();
  if(!ModelState.IsValid){ViewBag.Projects=await db.Projects.Where(p=>db.Teams.Any(t=>t.DepartmentId==p.DepartmentId&&t.SupervisorUserId==id)).ToListAsync(ct);return View(input);}
  var task=new TaskItem {ProjectId=input.ProjectId,CreatedBy=id,Title=input.Title.Trim(),Description=input.Description??"",EstimatedMinutes=input.EstimatedMinutes,Priority=input.Priority,DueAt=input.DueAt};
  db.Tasks.Add(task);await db.SaveChangesAsync(ct);
  return RedirectToAction(nameof(Index));
 }
 public async Task<IActionResult> Suggestions(long id,CancellationToken ct){
  var userId=long.Parse(users.GetUserId(User)!);
  var task=await db.Tasks.SingleOrDefaultAsync(t=>t.Id==id && db.Projects.Any(p=>p.Id==t.ProjectId && db.Teams.Any(team=>team.DepartmentId==p.DepartmentId&&team.SupervisorUserId==userId)),ct);
  if(task==null)return NotFound();
  ViewBag.Task=task;
  return View(await allocation.SuggestAsync(id,ct));
 }
 [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Assign(long taskId,long employeeUserId,CancellationToken ct) {
  var supervisorId=long.Parse(users.GetUserId(User)!);
  var task=await db.Tasks.SingleOrDefaultAsync(t=>t.Id==taskId && db.Projects.Any(p=>p.Id==t.ProjectId && db.Teams.Any(team=>team.DepartmentId==p.DepartmentId && team.SupervisorUserId==supervisorId)),ct);
  if(task==null)return NotFound();
  var allowed=await db.EmployeeProfiles.AnyAsync(e=>e.UserId==employeeUserId && db.Teams.Any(team=>team.DepartmentId==e.DepartmentId && team.SupervisorUserId==supervisorId),ct);
  if(!allowed)return BadRequest("Employee is not in your supervised department.");
  var suggestions=await allocation.SuggestAsync(taskId,ct);
  if(!suggestions.Any(s=>s.EmployeeUserId==employeeUserId))return BadRequest("Employee does not meet required skills.");
  if(await db.TaskAssignments.AnyAsync(a=>a.TaskId==taskId&&a.UnassignedAt==null,ct))return Conflict();
  db.TaskAssignments.Add(new TaskAssignment{TaskId=taskId,EmployeeUserId=employeeUserId,AssignedBy=supervisorId});
  db.Notifications.Add(new Notification{RecipientUserId=employeeUserId,Title="New task assigned",Message=task.Title});
  await db.SaveChangesAsync(ct);return RedirectToAction(nameof(Index));
 }
}
public class TaskInput {
 [System.ComponentModel.DataAnnotations.Range(1,long.MaxValue)] public long ProjectId {get;set;}
 [System.ComponentModel.DataAnnotations.Required][System.ComponentModel.DataAnnotations.StringLength(180)] public string Title {get;set;}="";
 public string? Description {get;set;}
 [System.ComponentModel.DataAnnotations.Range(1,100000)] public int EstimatedMinutes {get;set;}=60;
 [System.ComponentModel.DataAnnotations.Range(1,5)] public int Priority {get;set;}=2;
 public DateTime? DueAt {get;set;}
}
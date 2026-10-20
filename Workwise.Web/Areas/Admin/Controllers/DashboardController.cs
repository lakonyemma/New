using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workwise.Web.Data;
using Workwise.Web.Models;
namespace Workwise.Web.Areas.Admin.Controllers;
[Area("Admin")][Authorize(Roles="Admin")]
public class DashboardController(ApplicationDbContext db):Controller {
 public async Task<IActionResult> Index(CancellationToken ct)=>View(new AdminDashboardViewModel(
  await db.Users.CountAsync(ct),await db.Departments.CountAsync(ct),await db.Projects.CountAsync(ct),
  await db.Tasks.CountAsync(ct),await db.TaskAssignments.CountAsync(a=>a.UnassignedAt==null,ct)));
}
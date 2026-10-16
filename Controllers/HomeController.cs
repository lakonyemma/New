using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workwise.Mvc.Data;
using Workwise.Mvc.Models;
using Workwise.Mvc.ViewModels;

namespace Workwise.Mvc.Controllers;

[Authorize]
public class HomeController(WorkwiseDb db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var q = db.WorkItems.Include(x => x.AssignedEmployee).Include(x => x.Engagement).AsNoTracking().AsQueryable();
        if (User.IsInRole(Roles.Employee))
        {
            var employeeId = User.FindFirstValue("EmployeeId");
            if (!Guid.TryParse(employeeId, out var id)) return Forbid();
            q = q.Where(t => t.AssignedEmployeeId == id);
        }
        var tasks = await q.OrderBy(x => x.DueDate).ToListAsync();
        var vm = new DashboardVm
        {
            Total = tasks.Count,
            InProgress = tasks.Count(x => x.Status == TaskStates.InProgress),
            AwaitingReview = tasks.Count(x => x.Status == TaskStates.AwaitingReview),
            Overdue = tasks.Count(x => x.Status != TaskStates.Completed && x.DueDate.Date < DateTime.UtcNow.Date),
            TeamSize = User.IsInRole(Roles.Employee) ? 0 : await db.Employees.CountAsync(),
            RecentTasks = tasks.Take(6).ToList()
        };
        return View(vm);
    }
    [AllowAnonymous]
    public IActionResult Error() => View();
}

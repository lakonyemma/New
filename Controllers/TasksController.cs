using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workwise.Mvc.Data;
using Workwise.Mvc.Models;
using Workwise.Mvc.Services;
using Workwise.Mvc.ViewModels;

namespace Workwise.Mvc.Controllers;

[Authorize]
public class TasksController(WorkwiseDb db, AllocationService allocation) : Controller
{
    private Guid? CurrentEmployee() =>
        Guid.TryParse(User.FindFirstValue("EmployeeId"), out var id) ? id : null;

    public async Task<IActionResult> Index()
    {
        var q = db.WorkItems.Include(x => x.AssignedEmployee).Include(x => x.Engagement).AsNoTracking().AsQueryable();
        if (User.IsInRole(Roles.Employee))
        {
            var employeeId = CurrentEmployee();
            if (employeeId is null) return Forbid();
            q = q.Where(x => x.AssignedEmployeeId == employeeId);
        }
        return View(await q.OrderBy(x => x.DueDate).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var task = await db.WorkItems.Include(x => x.AssignedEmployee).Include(x => x.Engagement)
            .AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (task is null) return NotFound();
        if (User.IsInRole(Roles.Employee) && task.AssignedEmployeeId != CurrentEmployee()) return Forbid();
        return View(new TaskDetailsVm { Task = task,
            Recommendations = User.IsInRole(Roles.Employee) || task.Status == TaskStates.Completed
                ? [] : await allocation.RecommendAsync(task),
            History = await db.TaskUpdates.AsNoTracking().Where(x => x.WorkItemId == id)
                .OrderByDescending(x => x.At).ToListAsync() });
    }

    private async Task EngagementOptions(Guid? selected = null)
    {
        ViewBag.Engagements = new SelectList(
            await db.Engagements.AsNoTracking().OrderBy(x => x.ClientName).ToListAsync(),
            "Id", "Name", selected);
    }

    [Authorize(Roles = Roles.Management), HttpGet]
    public async Task<IActionResult> Create()
    {
        await EngagementOptions();
        return View("Edit", new TaskForm());
    }

    [Authorize(Roles = Roles.Management), HttpPost]
    public async Task<IActionResult> Create(TaskForm form)
    {
        await ValidateForm(form);
        if (!ModelState.IsValid) { await EngagementOptions(form.EngagementId); return View("Edit", form); }
        var task = new WorkItem { Title = form.Title.Trim(), Description = form.Description,
            Department = form.Department, RequiredSkills = form.RequiredSkills, Priority = form.Priority,
            EstimatedHours = form.EstimatedHours, DueDate = form.DueDate.Date,
            EngagementId = form.EngagementId, CreatedByUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) };
        db.WorkItems.Add(task);
        db.Audit.Add(new AuditEntry { Actor = User.Identity?.Name ?? "", Action = "create-task", Detail = task.Title });
        await db.SaveChangesAsync();
        TempData["Message"] = "Task created. Choose an employee from the recommendations.";
        return RedirectToAction(nameof(Details), new { id = task.Id });
    }

    [Authorize(Roles = Roles.Management), HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var t = await db.WorkItems.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (t is null) return NotFound();
        if (t.Status == TaskStates.Completed) return BadRequest("Completed tasks cannot be edited.");
        await EngagementOptions(t.EngagementId);
        return View(new TaskForm { Id = t.Id, Title = t.Title, Description = t.Description,
            Department = t.Department, RequiredSkills = t.RequiredSkills, Priority = t.Priority,
            EstimatedHours = t.EstimatedHours, DueDate = t.DueDate, EngagementId = t.EngagementId });
    }

    [Authorize(Roles = Roles.Management), HttpPost]
    public async Task<IActionResult> Edit(Guid id, TaskForm form)
    {
        var t = await db.WorkItems.FindAsync(id);
        if (t is null) return NotFound();
        if (t.Status == TaskStates.Completed) return BadRequest("Completed tasks cannot be edited.");
        await ValidateForm(form);
        if (!ModelState.IsValid) { await EngagementOptions(form.EngagementId); return View(form); }
        t.Title = form.Title.Trim();
        t.Description = form.Description;
        t.Department = form.Department;
        t.RequiredSkills = form.RequiredSkills;
        t.Priority = form.Priority;
        t.EstimatedHours = form.EstimatedHours;
        t.DueDate = form.DueDate.Date;
        t.EngagementId = form.EngagementId;
        t.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Message"] = "Task updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task ValidateForm(TaskForm f)
    {
        if (f.DueDate.Date < DateTime.UtcNow.Date) ModelState.AddModelError(nameof(f.DueDate), "Choose today or a future date.");
        if (f.Priority is not ("Low" or "Medium" or "High")) ModelState.AddModelError(nameof(f.Priority), "Invalid priority.");
        if (f.EngagementId.HasValue && !await db.Engagements.AnyAsync(x => x.Id == f.EngagementId.Value))
            ModelState.AddModelError(nameof(f.EngagementId), "Select an existing engagement.");
    }

    [Authorize(Roles = Roles.Management), HttpPost]
    public async Task<IActionResult> Assign(Guid id, Guid employeeId, bool confirmOverload = false, string? overrideReason = null)
    {
        var task = await db.WorkItems.FindAsync(id);
        if (task is null) return NotFound();
        if (task.Status is TaskStates.Completed or TaskStates.AwaitingReview) return BadRequest("This task cannot be reassigned now.");
        var candidate = (await allocation.RecommendAsync(task)).FirstOrDefault(x => x.EmployeeId == employeeId);
        if (candidate is null || !candidate.Eligible) return BadRequest("Selected employee is unavailable or missing required skills.");
        if (candidate.ProjectedPercent > 100 &&
            (!confirmOverload || string.IsNullOrWhiteSpace(overrideReason) || overrideReason.Trim().Length < 10))
        {
            TempData["Error"] = "Projected workload exceeds capacity. Confirm the override and give a reason (10+ characters).";
            return RedirectToAction(nameof(Details), new { id });
        }
        task.AssignedEmployeeId = employeeId;
        task.Status = TaskStates.InProgress;
        task.UpdatedAt = DateTime.UtcNow;
        db.TaskUpdates.Add(new TaskUpdate { WorkItemId = task.Id, Author = User.Identity?.Name ?? "",
            Message = "Assigned to " + candidate.Name +
                (candidate.ProjectedPercent > 100 ? "; overload override: " + overrideReason!.Trim() : "") });
        db.Audit.Add(new AuditEntry { Actor = User.Identity?.Name ?? "", Action = "assign-task", Detail = task.Title });
        await db.SaveChangesAsync();
        TempData["Message"] = "Task assigned to " + candidate.Name + ".";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.Employee), HttpPost]
    public async Task<IActionResult> Progress(Guid id, int progress, string? note)
    {
        var task = await db.WorkItems.FindAsync(id);
        if (task is null) return NotFound();
        if (task.AssignedEmployeeId != CurrentEmployee()) return Forbid();
        if (task.Status is TaskStates.Completed or TaskStates.AwaitingReview) return BadRequest("Task is not open for edits.");
        if (progress is < 0 or > 100) return BadRequest("Progress must be between 0 and 100.");
        if (note?.Length > 2000) return BadRequest("Note is too long.");
        task.Progress = progress;
        task.Status = TaskStates.InProgress;
        task.UpdatedAt = DateTime.UtcNow;
        db.TaskUpdates.Add(new TaskUpdate { WorkItemId = id, Author = User.Identity?.Name ?? "",
            Message = "Updated progress to " + progress + "%" + (string.IsNullOrWhiteSpace(note) ? "" : ": " + note.Trim()) });
        await db.SaveChangesAsync();
        TempData["Message"] = "Progress saved.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.Employee), HttpPost]
    public async Task<IActionResult> Submit(Guid id)
    {
        var task = await db.WorkItems.FindAsync(id);
        if (task is null) return NotFound();
        if (task.AssignedEmployeeId != CurrentEmployee()) return Forbid();
        if (task.Status != TaskStates.InProgress || task.Progress != 100)
            return BadRequest("Finish progress to 100% before submitting for review.");
        task.Status = TaskStates.AwaitingReview;
        task.UpdatedAt = DateTime.UtcNow;
        db.TaskUpdates.Add(new TaskUpdate { WorkItemId = id, Author = User.Identity?.Name ?? "",
            Message = "Submitted work for supervisor review." });
        await db.SaveChangesAsync();
        TempData["Message"] = "Submitted for review.";
        return RedirectToAction(nameof(Details), new { id });
    }
}

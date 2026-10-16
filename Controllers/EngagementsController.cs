using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workwise.Mvc.Data;
using Workwise.Mvc.Models;
using Workwise.Mvc.ViewModels;

namespace Workwise.Mvc.Controllers;

[Authorize]
public class EngagementsController(WorkwiseDb db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var query = db.Engagements.AsNoTracking().AsQueryable();
        if (User.IsInRole(Roles.Employee))
        {
            if (!Guid.TryParse(User.FindFirstValue("EmployeeId"), out var id)) return Forbid();
            query = query.Where(e => db.WorkItems.Any(t => t.EngagementId == e.Id && t.AssignedEmployeeId == id));
        }
        return View(await query.OrderBy(x => x.DueDate).ToListAsync());
    }

    [Authorize(Roles = Roles.Management), HttpGet]
    public IActionResult Create() => View("Edit", new EngagementForm());

    [Authorize(Roles = Roles.Management), HttpPost]
    public async Task<IActionResult> Create(EngagementForm form)
    {
        if (!ModelState.IsValid) return View("Edit", form);
        var engagement = new ClientEngagement { ClientName = form.ClientName.Trim(), Name = form.Name.Trim(),
            ServiceLine = form.ServiceLine.Trim(), Stage = form.Stage, DueDate = form.DueDate.Date, Notes = form.Notes };
        db.Engagements.Add(engagement);
        db.Audit.Add(new AuditEntry { Actor = User.Identity?.Name ?? "", Action = "create-engagement", Detail = engagement.Name });
        await db.SaveChangesAsync();
        TempData["Message"] = "Engagement created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Management), HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var e = await db.Engagements.FindAsync(id);
        return e is null ? NotFound() : View(new EngagementForm
        {
            Id = e.Id, Name = e.Name, ClientName = e.ClientName,
            ServiceLine = e.ServiceLine, Stage = e.Stage, DueDate = e.DueDate, Notes = e.Notes
        });
    }

    [Authorize(Roles = Roles.Management), HttpPost]
    public async Task<IActionResult> Edit(Guid id, EngagementForm form)
    {
        var e = await db.Engagements.FindAsync(id);
        if (e is null) return NotFound();
        if (!ModelState.IsValid) return View(form);
        e.ClientName = form.ClientName.Trim();
        e.Name = form.Name.Trim();
        e.ServiceLine = form.ServiceLine.Trim();
        e.Stage = form.Stage;
        e.DueDate = form.DueDate.Date;
        e.Notes = form.Notes;
        await db.SaveChangesAsync();
        TempData["Message"] = "Engagement updated.";
        return RedirectToAction(nameof(Index));
    }
}

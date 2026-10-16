using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workwise.Mvc.Data;
using Workwise.Mvc.Models;
using Workwise.Mvc.ViewModels;

namespace Workwise.Mvc.Controllers;

[Authorize(Roles = Roles.Management)]
public class EmployeesController(WorkwiseDb db, IPasswordHasher<UserAccount> hasher) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await db.Employees.AsNoTracking().OrderBy(x => x.FullName).ToListAsync());

    [Authorize(Roles = Roles.Administrator), HttpGet]
    public IActionResult Create() => View("Edit", new EmployeeForm());

    [Authorize(Roles = Roles.Administrator), HttpPost]
    public async Task<IActionResult> Create(EmployeeForm form)
    {
        if (string.IsNullOrWhiteSpace(form.UserName) || string.IsNullOrWhiteSpace(form.InitialPassword))
            ModelState.AddModelError("", "Username and temporary password are required.");
        if (!string.IsNullOrEmpty(form.InitialPassword) && form.InitialPassword.Length < 12)
            ModelState.AddModelError(nameof(form.InitialPassword), "Use a temporary password of at least 12 characters.");
        if (form.Role is not (Roles.Employee or Roles.Supervisor))
            ModelState.AddModelError(nameof(form.Role), "Select Employee or Supervisor.");
        if (await db.Employees.AnyAsync(x => x.Email == form.Email))
            ModelState.AddModelError(nameof(form.Email), "An employee with this email already exists.");
        if (await db.Users.AnyAsync(x => x.UserName == form.UserName))
            ModelState.AddModelError(nameof(form.UserName), "This username is taken.");
        if (!ModelState.IsValid) return View("Edit", form);
        var employee = new Employee { FullName = form.FullName.Trim(), Email = form.Email.Trim(),
            Department = form.Department.Trim(), Skills = form.Skills.Trim(), WeeklyHours = form.WeeklyHours,
            IsAvailable = form.IsAvailable };
        var account = new UserAccount { UserName = form.UserName!.Trim(), Role = form.Role, EmployeeId = employee.Id };
        account.PasswordHash = hasher.HashPassword(account, form.InitialPassword!);
        db.Employees.Add(employee);
        db.Users.Add(account);
        db.Audit.Add(new AuditEntry { Actor = User.Identity?.Name ?? "", Action = "create-employee", Detail = employee.FullName });
        await db.SaveChangesAsync();
        TempData["Message"] = "Employee and login account created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Administrator), HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var e = await db.Employees.FindAsync(id);
        return e is null ? NotFound() : View(new EmployeeForm { Id = e.Id, FullName = e.FullName,
            Email = e.Email, Department = e.Department, Skills = e.Skills,
            WeeklyHours = e.WeeklyHours, IsAvailable = e.IsAvailable });
    }

    [Authorize(Roles = Roles.Administrator), HttpPost]
    public async Task<IActionResult> Edit(Guid id, EmployeeForm form)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee is null) return NotFound();
        if (await db.Employees.AnyAsync(x => x.Id != id && x.Email == form.Email))
            ModelState.AddModelError(nameof(form.Email), "An employee with this email already exists.");
        if (!ModelState.IsValid) return View(form);
        employee.FullName = form.FullName.Trim();
        employee.Email = form.Email.Trim();
        employee.Department = form.Department.Trim();
        employee.Skills = form.Skills.Trim();
        employee.WeeklyHours = form.WeeklyHours;
        employee.IsAvailable = form.IsAvailable;
        db.Audit.Add(new AuditEntry { Actor = User.Identity?.Name ?? "", Action = "edit-employee", Detail = employee.FullName });
        await db.SaveChangesAsync();
        TempData["Message"] = "Employee updated.";
        return RedirectToAction(nameof(Index));
    }
}

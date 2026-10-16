using System.ComponentModel.DataAnnotations;

namespace Workwise.Mvc.Models;

public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Supervisor = "Supervisor";
    public const string Employee = "Employee";
    public const string Management = Administrator + "," + Supervisor;
}

public static class TaskStates
{
    public const string Open = "Open";
    public const string InProgress = "In Progress";
    public const string AwaitingReview = "Awaiting Review";
    public const string Completed = "Completed";
}

public class UserAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(191)] public string UserName { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    [Required, MaxLength(25)] public string Role { get; set; } = Roles.Employee;
    public bool Active { get; set; } = true;
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}
public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(120)] public string FullName { get; set; } = "";
    [Required, EmailAddress, MaxLength(254)] public string Email { get; set; } = "";
    [Required, MaxLength(80)] public string Department { get; set; } = "Audit";
    [MaxLength(800)] public string Skills { get; set; } = "";
    [Range(1, 80)] public double WeeklyHours { get; set; } = 40;
    public bool IsAvailable { get; set; } = true;
}
public class ClientEngagement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(160)] public string ClientName { get; set; } = "";
    [Required, MaxLength(160)] public string Name { get; set; } = "";
    [Required, MaxLength(80)] public string ServiceLine { get; set; } = "Audit";
    [MaxLength(25)] public string Stage { get; set; } = "Planning";
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);
    [MaxLength(2000)] public string Notes { get; set; } = "";
}
public class WorkItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(180)] public string Title { get; set; } = "";
    [MaxLength(4000)] public string Description { get; set; } = "";
    [MaxLength(80)] public string Department { get; set; } = "Audit";
    [MaxLength(800)] public string RequiredSkills { get; set; } = "";
    [MaxLength(20)] public string Priority { get; set; } = "Medium";
    [MaxLength(30)] public string Status { get; set; } = TaskStates.Open;
    public double EstimatedHours { get; set; } = 4;
    public int Progress { get; set; }
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
    public Guid? AssignedEmployeeId { get; set; }
    public Employee? AssignedEmployee { get; set; }
    public Guid? EngagementId { get; set; }
    public ClientEngagement? Engagement { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public double RemainingHours => Status == TaskStates.Completed ? 0 : EstimatedHours * (100 - Progress) / 100d;
}
public class TaskUpdate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkItemId { get; set; }
    public WorkItem? WorkItem { get; set; }
    [MaxLength(2000)] public string Message { get; set; } = "";
    [MaxLength(191)] public string Author { get; set; } = "";
    public DateTime At { get; set; } = DateTime.UtcNow;
}
public class AuditEntry
{
    public long Id { get; set; }
    public DateTime At { get; set; } = DateTime.UtcNow;
    [MaxLength(191)] public string Actor { get; set; } = "";
    [MaxLength(100)] public string Action { get; set; } = "";
    [MaxLength(500)] public string Detail { get; set; } = "";
}

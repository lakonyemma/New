using System.ComponentModel.DataAnnotations;
using Workwise.Mvc.Models;
using Workwise.Mvc.Services;

namespace Workwise.Mvc.ViewModels;

public class LoginForm
{
    [Required] public string UserName { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public class EmployeeForm
{
    public Guid? Id { get; set; }
    [Required, MaxLength(120)] public string FullName { get; set; } = "";
    [Required, EmailAddress, MaxLength(254)] public string Email { get; set; } = "";
    [Required, MaxLength(80)] public string Department { get; set; } = "Audit";
    [MaxLength(800)] public string Skills { get; set; } = "";
    [Range(1, 80)] public double WeeklyHours { get; set; } = 40;
    public bool IsAvailable { get; set; } = true;
    [MaxLength(191)] public string? UserName { get; set; }
    [DataType(DataType.Password)] public string? InitialPassword { get; set; }
    public string Role { get; set; } = Roles.Employee;
}
public class EngagementForm
{
    public Guid? Id { get; set; }
    [Required, MaxLength(160)] public string ClientName { get; set; } = "";
    [Required, MaxLength(160)] public string Name { get; set; } = "";
    [Required, MaxLength(80)] public string ServiceLine { get; set; } = "Audit";
    [Required, MaxLength(25)] public string Stage { get; set; } = "Planning";
    [Required, DataType(DataType.Date)] public DateTime DueDate { get; set; } = DateTime.UtcNow.Date.AddDays(30);
    [MaxLength(2000)] public string Notes { get; set; } = "";
}
public class TaskForm
{
    public Guid? Id { get; set; }
    [Required, MaxLength(180)] public string Title { get; set; } = "";
    [MaxLength(4000)] public string Description { get; set; } = "";
    [Required, MaxLength(80)] public string Department { get; set; } = "Audit";
    [MaxLength(800)] public string RequiredSkills { get; set; } = "";
    [Required] public string Priority { get; set; } = "Medium";
    [Range(0.5, 1000)] public double EstimatedHours { get; set; } = 4;
    [Required, DataType(DataType.Date)] public DateTime DueDate { get; set; } = DateTime.UtcNow.Date.AddDays(7);
    public Guid? EngagementId { get; set; }
}
public class DashboardVm
{
    public int Total { get; set; }
    public int InProgress { get; set; }
    public int AwaitingReview { get; set; }
    public int Overdue { get; set; }
    public int TeamSize { get; set; }
    public List<WorkItem> RecentTasks { get; set; } = [];
}
public class TaskDetailsVm
{
    public WorkItem Task { get; set; } = new();
    public IReadOnlyList<AllocationCandidate> Recommendations { get; set; } = [];
    public List<TaskUpdate> History { get; set; } = [];
}

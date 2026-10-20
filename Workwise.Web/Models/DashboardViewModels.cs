namespace Workwise.Web.Models;
public record AdminDashboardViewModel(int Users,int Departments,int Projects,int Tasks,int ActiveAssignments);
public record SupervisorDashboardViewModel(int TeamMembers,int Projects,int OpenTasks,int ActiveAssignments,int PendingSubmissions);
public record EmployeeDashboardViewModel(int AssignedTasks,int CompletedTasks,int PendingSubmissions,int UnreadNotifications);

using Microsoft.EntityFrameworkCore;
using Workwise.Mvc.Models;

namespace Workwise.Mvc.Data;

public class WorkwiseDb(DbContextOptions<WorkwiseDb> options) : DbContext(options)
{
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<ClientEngagement> Engagements => Set<ClientEngagement>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<TaskUpdate> TaskUpdates => Set<TaskUpdate>();
    public DbSet<AuditEntry> Audit => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<UserAccount>().HasIndex(x => x.UserName).IsUnique();
        model.Entity<UserAccount>().HasIndex(x => x.EmployeeId).IsUnique();
        model.Entity<UserAccount>().HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        model.Entity<Employee>().HasIndex(x => x.Email).IsUnique();
        model.Entity<WorkItem>().HasIndex(x => new { x.AssignedEmployeeId, x.DueDate });
        model.Entity<WorkItem>().HasOne(x => x.AssignedEmployee).WithMany().HasForeignKey(x => x.AssignedEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        model.Entity<WorkItem>().HasOne(x => x.Engagement).WithMany().HasForeignKey(x => x.EngagementId)
            .OnDelete(DeleteBehavior.Restrict);
        model.Entity<TaskUpdate>().HasOne(x => x.WorkItem).WithMany().HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);
        model.Entity<TaskUpdate>().HasIndex(x => x.WorkItemId);
        model.Entity<AuditEntry>().Property(x => x.Id).ValueGeneratedOnAdd();
    }
}

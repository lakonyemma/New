namespace Workwise.Web.Models;
// Supporting entities for the Workwise domain. Authentication/session storage uses ASP.NET Core Identity.
public class SystemSetting { public long Id {get;set;} string SettingKey {get;set;} = ""; string SettingValue {get;set;} = ""; long? UpdatedBy {get;set;} }
public class AuditLog { public long Id {get;set;} long? ActorUserId {get;set;} string Action {get;set;} = ""; string EntityType {get;set;} = ""; long? EntityId {get;set;} string? OldValues {get;set;} string? NewValues {get;set;} DateTime CreatedAt {get;set;} = DateTime.UtcNow; }
public class TeamMember {  long TeamId {get;set;} long EmployeeUserId {get;set;} DateTime JoinedAt {get;set;} = DateTime.UtcNow; DateTime? LeftAt {get;set;} }
public class ProjectMember {  long ProjectId {get;set;} long UserId {get;set;} string ProjectRole {get;set;} = "Member"; }
public class EmployeeAvailability { public long Id {get;set;} long EmployeeUserId {get;set;} DateTime StartsAt {get;set;} DateTime EndsAt {get;set;} string AvailabilityType {get;set;} = "Unavailable"; string? Notes {get;set;} }
public class TaskDependency {  long TaskId {get;set;} long DependsOnTaskId {get;set;} string DependencyType {get;set;} = "FinishToStart"; }
public class TaskComment { public long Id {get;set;} long TaskId {get;set;} long AuthorUserId {get;set;} string Body {get;set;} = ""; DateTime CreatedAt {get;set;} = DateTime.UtcNow; }
public class TaskAttachment { public long Id {get;set;} long TaskId {get;set;} long UploadedBy {get;set;} string OriginalName {get;set;} = ""; string StorageKey {get;set;} = ""; string MimeType {get;set;} long SizeBytes {get;set;} }
public class AllocationScore { public long Id {get;set;} long TaskId {get;set;} long EmployeeUserId {get;set;} decimal Score {get;set;} string ScoreBreakdown {get;set;} = "{}"; DateTime CalculatedAt {get;set;} = DateTime.UtcNow; }
public class AllocationHistory { public long Id {get;set;} long TaskId {get;set;} long SuggestedEmployeeId {get;set;} long? SelectedEmployeeId {get;set;} long? DecisionBy {get;set;} string Decision {get;set;} = "Suggested"; DateTime CreatedAt {get;set;} = DateTime.UtcNow; }
public class AllocationRule { public long Id {get;set;} string Name {get;set;} = ""; string RuleConfig {get;set;} = "{}"; bool IsActive {get;set;} = true; long? UpdatedBy {get;set;} }
public class WorkloadRecord { public long Id {get;set;} long EmployeeUserId {get;set;} DateTime PeriodStart {get;set;} DateTime PeriodEnd {get;set;} int CapacityMinutes {get;set;} int AssignedMinutes {get;set;} }
public class TimeEntry { public long Id {get;set;} long TaskId {get;set;} long EmployeeUserId {get;set;} DateTime StartedAt {get;set;} DateTime? EndedAt {get;set;} int DurationMinutes {get;set;} string? Notes {get;set;} }
public class WorkSchedule { public long Id {get;set;} long EmployeeUserId {get;set;} int DayOfWeek {get;set;} TimeSpan StartTime {get;set;} TimeSpan EndTime {get;set;} DateTime EffectiveFrom {get;set;} DateTime? EffectiveTo {get;set;} }
public class NotificationPreference {  long UserId {get;set;} bool InAppEnabled {get;set;} = true; bool EmailEnabled {get;set;} = true; bool TaskUpdatesEnabled {get;set;} = true; bool RemindersEnabled {get;set;} = true; }

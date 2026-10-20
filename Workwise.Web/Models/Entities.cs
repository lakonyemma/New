namespace Workwise.Web.Models;
public class Department { public long Id {get;set;} public string Name {get;set;} = ""; }
public class Team { public long Id {get;set;} public long DepartmentId {get;set;} public long SupervisorUserId {get;set;} public string Name {get;set;} = ""; }
public class EmployeeProfile { public long UserId {get;set;} public string EmployeeNumber {get;set;} = ""; public long DepartmentId {get;set;} public int WeeklyCapacityMinutes {get;set;} = 2400; }
public class Skill { public long Id {get;set;} public string Name {get;set;} = ""; }
public class EmployeeSkill { public long EmployeeUserId {get;set;} public long SkillId {get;set;} public int ProficiencyLevel {get;set;} }
public class Project { public long Id {get;set;} public long DepartmentId {get;set;} public string Name {get;set;} = ""; public string Status {get;set;} = "Active"; }
public class TaskItem { public long Id {get;set;} public long ProjectId {get;set;} public long CreatedBy {get;set;} public string Title {get;set;} = ""; public string Description {get;set;} = ""; public int Priority {get;set;} = 2; public string Status {get;set;} = "Pending"; public int EstimatedMinutes {get;set;} public DateTime? DueAt {get;set;} }
public class TaskAssignment { public long Id {get;set;} public long TaskId {get;set;} public long EmployeeUserId {get;set;} public long AssignedBy {get;set;} public DateTime AssignedAt {get;set;} = DateTime.UtcNow; public DateTime? UnassignedAt {get;set;} }
public class TaskSkillRequirement { public long TaskId {get;set;} public long SkillId {get;set;} public int MinimumLevel {get;set;} = 1; public decimal ImportanceWeight {get;set;} = 1; }
public class Notification { public long Id {get;set;} public long RecipientUserId {get;set;} public string Title {get;set;} = ""; public string Message {get;set;} = ""; public DateTime? ReadAt {get;set;} public DateTime CreatedAt {get;set;} = DateTime.UtcNow; }
public class TaskSubmission { public long Id {get;set;} public long TaskId {get;set;} public long EmployeeUserId {get;set;} public string SubmissionNote {get;set;} = ""; public DateTime SubmittedAt {get;set;} = DateTime.UtcNow; }
public class TaskReview { public long Id {get;set;} public long SubmissionId {get;set;} public long ReviewerUserId {get;set;} public string Decision {get;set;} = ""; public string Feedback {get;set;} = ""; }
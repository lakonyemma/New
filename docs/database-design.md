# Workwise database schema status

The domain currently includes 27 EF Core business entity classes, with ASP.NET Core Identity providing users, roles, role membership, user claims, logins, tokens and session authentication. The original 32-table conceptual proposal is not a literal one-to-one match with Identity's physical schema.

## Core entities
- Department
- Team
- EmployeeProfile
- Skill
- EmployeeSkill
- Project
- TaskItem
- TaskAssignment
- TaskSkillRequirement
- Notification
- TaskSubmission
- TaskReview
- SystemSetting
- AuditLog
- TeamMember
- ProjectMember
- EmployeeAvailability
- TaskDependency
- TaskComment
- TaskAttachment
- AllocationScore
- AllocationHistory
- AllocationRule
- WorkloadRecord
- TimeEntry
- WorkSchedule
- NotificationPreference

## Current limitations
- No committed EF migrations yet; use dotnet-ef after configuring MySQL.
- Foreign key constraints, cascade rules and normalized relationships need an explicit review before production migration.
- Domain constraints such as assignment uniqueness and task dependency cycles need application/database enforcement.
- Authentication bootstrap and full workflow testing remain outstanding.

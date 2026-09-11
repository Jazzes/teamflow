import type {
  projects,
  sprints,
  sprintStatusEnum,
  taskPriorityEnum,
  tasks,
  taskStatusChanges,
  taskStatusEnum,
  teamRoleEnum,
  timeEntries,
} from "./schema.js";

export type TaskStatus = (typeof taskStatusEnum.enumValues)[number];
export type TaskPriority = (typeof taskPriorityEnum.enumValues)[number];
export type SprintStatus = (typeof sprintStatusEnum.enumValues)[number];
export type TeamRole = (typeof teamRoleEnum.enumValues)[number];

export type Project = typeof projects.$inferSelect;
export type NewProject = typeof projects.$inferInsert;
export type Sprint = typeof sprints.$inferSelect;
export type Task = typeof tasks.$inferSelect;
export type NewTask = typeof tasks.$inferInsert;
export type NewTaskStatusChange = typeof taskStatusChanges.$inferInsert;
export type TimeEntry = typeof timeEntries.$inferSelect;
export type NewTimeEntry = typeof timeEntries.$inferInsert;

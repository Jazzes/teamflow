import type {
  NewProject,
  NewTask,
  NewTaskStatusChange,
  NewTimeEntry,
  Project,
  Sprint,
  SprintStatus,
  Task,
  TaskStatus,
  TimeEntry,
} from "./types.js";

/** Контракты репозиториев. BLL зависит только от них, а не от конкретной ORM. */
export interface TeamRepository {
  isMember(teamId: string, userId: string): Promise<boolean>;
}

export interface ProjectRepository {
  findById(id: string): Promise<Project | null>;
  findByKey(key: string): Promise<Project | null>;
  create(data: NewProject): Promise<Project>;
}

export interface SprintRepository {
  findById(id: string): Promise<Sprint | null>;
  findActiveByProject(projectId: string): Promise<Sprint | null>;
  updateStatus(id: string, status: SprintStatus): Promise<Sprint>;
}

export interface TaskRepository {
  findById(id: string): Promise<Task | null>;
  listByProject(projectId: string): Promise<Task[]>;
  /** Следующий порядковый номер задачи внутри проекта (TF-1, TF-2, ...). */
  nextNumber(projectId: string): Promise<number>;
  create(data: NewTask): Promise<Task>;
  updateStatus(id: string, status: TaskStatus): Promise<Task>;
  /** Возвращает незавершённые задачи спринта в бэклог, результат: число перенесённых задач. */
  moveUnfinishedToBacklog(sprintId: string): Promise<number>;
  addStatusChange(data: NewTaskStatusChange): Promise<void>;
}

export interface TimeEntryRepository {
  create(data: NewTimeEntry): Promise<TimeEntry>;
}

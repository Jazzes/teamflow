import type {
  Project,
  ProjectRepository,
  SprintRepository,
  Task,
  TaskRepository,
  TeamRepository,
  TimeEntry,
  TimeEntryRepository,
} from "@teamflow/dal";
import { DomainError } from "../errors.js";
import { allowedTransitions, canTransition } from "../taskWorkflow.js";
import {
  changeStatusSchema,
  createTaskSchema,
  logTimeSchema,
  parseInput,
  type ChangeStatusInput,
  type CreateTaskInput,
  type LogTimeInput,
} from "../validation.js";

export interface TaskServiceDeps {
  projects: ProjectRepository;
  teams: TeamRepository;
  sprints: SprintRepository;
  tasks: TaskRepository;
  timeEntries: TimeEntryRepository;
}

export class TaskService {
  private readonly deps: TaskServiceDeps;

  constructor(deps: TaskServiceDeps) {
    this.deps = deps;
  }

  /** Создаёт задачу в проекте: проверяет доступ, исполнителя и спринт, присваивает номер. */
  async createTask(projectId: string, reporterId: string, input: CreateTaskInput): Promise<Task> {
    const data = parseInput(createTaskSchema, input);
    const project = await this.requireProjectAccess(projectId, reporterId);

    if (data.assigneeId && !(await this.deps.teams.isMember(project.teamId, data.assigneeId))) {
      throw new DomainError("VALIDATION", "Исполнитель не состоит в команде проекта");
    }

    if (data.sprintId) {
      const sprint = await this.deps.sprints.findById(data.sprintId);
      if (!sprint || sprint.projectId !== projectId) {
        throw new DomainError("VALIDATION", "Спринт не относится к этому проекту");
      }
      if (sprint.status === "closed") {
        throw new DomainError("CONFLICT", "Нельзя добавить задачу в закрытый спринт");
      }
    }

    const number = await this.deps.tasks.nextNumber(projectId);
    return this.deps.tasks.create({
      projectId,
      number,
      title: data.title,
      description: data.description ?? null,
      priority: data.priority,
      storyPoints: data.storyPoints ?? null,
      assigneeId: data.assigneeId ?? null,
      reporterId,
      sprintId: data.sprintId ?? null,
      dueDate: data.dueDate ?? null,
      status: data.sprintId ? "todo" : "backlog",
    });
  }

  /** Переводит задачу в новый статус по правилам доски и пишет историю изменения. */
  async changeStatus(taskId: string, userId: string, input: ChangeStatusInput): Promise<Task> {
    const { status } = parseInput(changeStatusSchema, input);
    const task = await this.requireTask(taskId);
    await this.requireProjectAccess(task.projectId, userId);

    if (!canTransition(task.status, status)) {
      throw new DomainError("CONFLICT", `Переход ${task.status} → ${status} запрещён`, {
        allowed: allowedTransitions(task.status),
      });
    }
    if (status === "done" && !task.assigneeId) {
      throw new DomainError("CONFLICT", "Нельзя закрыть задачу без исполнителя");
    }

    const fromStatus = task.status;
    const updated = await this.deps.tasks.updateStatus(task.id, status);
    await this.deps.tasks.addStatusChange({
      taskId: task.id,
      changedBy: userId,
      fromStatus,
      toStatus: status,
    });
    return updated;
  }

  /** Фиксирует затраченное на задачу время. */
  async logTime(taskId: string, userId: string, input: LogTimeInput): Promise<TimeEntry> {
    const data = parseInput(logTimeSchema, input);
    const task = await this.requireTask(taskId);
    await this.requireProjectAccess(task.projectId, userId);
    return this.deps.timeEntries.create({
      taskId: task.id,
      userId,
      minutes: data.minutes,
      spentOn: data.spentOn,
      note: data.note ?? null,
    });
  }

  private async requireTask(taskId: string): Promise<Task> {
    const task = await this.deps.tasks.findById(taskId);
    if (!task) {
      throw new DomainError("NOT_FOUND", "Задача не найдена");
    }
    return task;
  }

  private async requireProjectAccess(projectId: string, userId: string): Promise<Project> {
    const project = await this.deps.projects.findById(projectId);
    if (!project) {
      throw new DomainError("NOT_FOUND", "Проект не найден");
    }
    if (!(await this.deps.teams.isMember(project.teamId, userId))) {
      throw new DomainError("FORBIDDEN", "Нет доступа к проекту");
    }
    return project;
  }
}

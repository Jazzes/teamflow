import { randomUUID } from "node:crypto";
import type {
  NewProject,
  NewTask,
  NewTaskStatusChange,
  NewTimeEntry,
  Project,
  ProjectRepository,
  Sprint,
  SprintRepository,
  SprintStatus,
  Task,
  TaskRepository,
  TaskStatus,
  TeamRepository,
  TimeEntry,
  TimeEntryRepository,
} from "@teamflow/dal";

/** In-memory реализации репозиториев: бизнес-логика тестируется без базы данных. */
export class InMemoryTeams implements TeamRepository {
  readonly members = new Set<string>();

  add(teamId: string, userId: string): void {
    this.members.add(`${teamId}:${userId}`);
  }

  async isMember(teamId: string, userId: string): Promise<boolean> {
    return this.members.has(`${teamId}:${userId}`);
  }
}

export class InMemoryProjects implements ProjectRepository {
  readonly rows: Project[] = [];

  async findById(id: string): Promise<Project | null> {
    return this.rows.find((p) => p.id === id) ?? null;
  }

  async findByKey(key: string): Promise<Project | null> {
    return this.rows.find((p) => p.key === key) ?? null;
  }

  async create(data: NewProject): Promise<Project> {
    const row: Project = {
      id: data.id ?? randomUUID(),
      teamId: data.teamId,
      key: data.key,
      name: data.name,
      description: data.description ?? null,
      createdAt: new Date(),
    };
    this.rows.push(row);
    return row;
  }
}

export class InMemorySprints implements SprintRepository {
  readonly rows: Sprint[] = [];

  async findById(id: string): Promise<Sprint | null> {
    return this.rows.find((s) => s.id === id) ?? null;
  }

  async findActiveByProject(projectId: string): Promise<Sprint | null> {
    return this.rows.find((s) => s.projectId === projectId && s.status === "active") ?? null;
  }

  async updateStatus(id: string, status: SprintStatus): Promise<Sprint> {
    const sprint = this.rows.find((s) => s.id === id);
    if (!sprint) {
      throw new Error("sprint not found");
    }
    sprint.status = status;
    return sprint;
  }
}

export class InMemoryTasks implements TaskRepository {
  readonly rows: Task[] = [];
  readonly history: NewTaskStatusChange[] = [];

  async findById(id: string): Promise<Task | null> {
    return this.rows.find((t) => t.id === id) ?? null;
  }

  async listByProject(projectId: string): Promise<Task[]> {
    return this.rows.filter((t) => t.projectId === projectId).sort((a, b) => a.number - b.number);
  }

  async nextNumber(projectId: string): Promise<number> {
    const numbers = this.rows.filter((t) => t.projectId === projectId).map((t) => t.number);
    return (numbers.length ? Math.max(...numbers) : 0) + 1;
  }

  async create(data: NewTask): Promise<Task> {
    const now = new Date();
    const row: Task = {
      id: data.id ?? randomUUID(),
      projectId: data.projectId,
      sprintId: data.sprintId ?? null,
      parentId: data.parentId ?? null,
      number: data.number,
      title: data.title,
      description: data.description ?? null,
      status: data.status ?? "backlog",
      priority: data.priority ?? "medium",
      storyPoints: data.storyPoints ?? null,
      assigneeId: data.assigneeId ?? null,
      reporterId: data.reporterId,
      dueDate: data.dueDate ?? null,
      createdAt: now,
      updatedAt: now,
    };
    this.rows.push(row);
    return row;
  }

  async updateStatus(id: string, status: TaskStatus): Promise<Task> {
    const task = this.rows.find((t) => t.id === id);
    if (!task) {
      throw new Error("task not found");
    }
    task.status = status;
    task.updatedAt = new Date();
    return task;
  }

  async moveUnfinishedToBacklog(sprintId: string): Promise<number> {
    const unfinished = this.rows.filter((t) => t.sprintId === sprintId && t.status !== "done");
    for (const task of unfinished) {
      task.sprintId = null;
      task.status = "backlog";
    }
    return unfinished.length;
  }

  async addStatusChange(data: NewTaskStatusChange): Promise<void> {
    this.history.push(data);
  }
}

export class InMemoryTimeEntries implements TimeEntryRepository {
  readonly rows: TimeEntry[] = [];

  async create(data: NewTimeEntry): Promise<TimeEntry> {
    const row: TimeEntry = {
      id: data.id ?? randomUUID(),
      taskId: data.taskId,
      userId: data.userId,
      minutes: data.minutes,
      spentOn: data.spentOn,
      note: data.note ?? null,
    };
    this.rows.push(row);
    return row;
  }
}

/** Готовое окружение: команда из двух человек, проект TF и запланированный спринт. */
export function createWorld() {
  const teams = new InMemoryTeams();
  const projects = new InMemoryProjects();
  const sprints = new InMemorySprints();
  const tasks = new InMemoryTasks();
  const timeEntries = new InMemoryTimeEntries();

  const teamId = randomUUID();
  const leadId = randomUUID();
  const developerId = randomUUID();
  const outsiderId = randomUUID();
  teams.add(teamId, leadId);
  teams.add(teamId, developerId);

  const project: Project = {
    id: randomUUID(),
    teamId,
    key: "TF",
    name: "TeamFlow",
    description: null,
    createdAt: new Date(),
  };
  projects.rows.push(project);

  const sprint: Sprint = {
    id: randomUUID(),
    projectId: project.id,
    name: "Спринт 1",
    startsOn: "2026-09-14",
    endsOn: "2026-09-27",
    status: "planned",
  };
  sprints.rows.push(sprint);

  return { teams, projects, sprints, tasks, timeEntries, teamId, leadId, developerId, outsiderId, project, sprint };
}

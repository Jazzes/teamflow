import type { Project, ProjectRepository, Task, TaskRepository, TeamRepository } from "@teamflow/dal";
import { DomainError } from "../errors.js";
import { createProjectSchema, parseInput, type CreateProjectInput } from "../validation.js";

export interface ProjectServiceDeps {
  projects: ProjectRepository;
  teams: TeamRepository;
  tasks: TaskRepository;
}

export class ProjectService {
  private readonly deps: ProjectServiceDeps;

  constructor(deps: ProjectServiceDeps) {
    this.deps = deps;
  }

  /** Создаёт проект. Создавать может только участник команды, ключ проекта уникален. */
  async createProject(userId: string, input: CreateProjectInput): Promise<Project> {
    const data = parseInput(createProjectSchema, input);
    if (!(await this.deps.teams.isMember(data.teamId, userId))) {
      throw new DomainError("FORBIDDEN", "Пользователь не состоит в команде");
    }
    if (await this.deps.projects.findByKey(data.key)) {
      throw new DomainError("CONFLICT", `Проект с ключом ${data.key} уже существует`);
    }
    return this.deps.projects.create({
      teamId: data.teamId,
      key: data.key,
      name: data.name,
      description: data.description ?? null,
    });
  }

  /** Возвращает задачи проекта, доступные только участникам его команды. */
  async listTasks(projectId: string, userId: string): Promise<Task[]> {
    const project = await this.deps.projects.findById(projectId);
    if (!project) {
      throw new DomainError("NOT_FOUND", "Проект не найден");
    }
    if (!(await this.deps.teams.isMember(project.teamId, userId))) {
      throw new DomainError("FORBIDDEN", "Нет доступа к проекту");
    }
    return this.deps.tasks.listByProject(projectId);
  }
}

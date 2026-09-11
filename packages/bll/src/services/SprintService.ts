import type { ProjectRepository, Sprint, SprintRepository, TaskRepository, TeamRepository } from "@teamflow/dal";
import { DomainError } from "../errors.js";

export interface SprintServiceDeps {
  projects: ProjectRepository;
  teams: TeamRepository;
  sprints: SprintRepository;
  tasks: TaskRepository;
}

export interface CloseSprintResult {
  sprint: Sprint;
  movedToBacklog: number;
}

export class SprintService {
  private readonly deps: SprintServiceDeps;

  constructor(deps: SprintServiceDeps) {
    this.deps = deps;
  }

  /** Запускает спринт. В проекте одновременно может быть только один активный спринт. */
  async startSprint(sprintId: string, userId: string): Promise<Sprint> {
    const sprint = await this.requireSprintAccess(sprintId, userId);
    if (sprint.status !== "planned") {
      throw new DomainError("CONFLICT", "Запустить можно только запланированный спринт");
    }
    const active = await this.deps.sprints.findActiveByProject(sprint.projectId);
    if (active) {
      throw new DomainError("CONFLICT", `В проекте уже идёт спринт «${active.name}»`);
    }
    return this.deps.sprints.updateStatus(sprint.id, "active");
  }

  /** Закрывает спринт и возвращает незавершённые задачи в бэклог. */
  async closeSprint(sprintId: string, userId: string): Promise<CloseSprintResult> {
    const sprint = await this.requireSprintAccess(sprintId, userId);
    if (sprint.status !== "active") {
      throw new DomainError("CONFLICT", "Закрыть можно только активный спринт");
    }
    const movedToBacklog = await this.deps.tasks.moveUnfinishedToBacklog(sprint.id);
    const closed = await this.deps.sprints.updateStatus(sprint.id, "closed");
    return { sprint: closed, movedToBacklog };
  }

  private async requireSprintAccess(sprintId: string, userId: string): Promise<Sprint> {
    const sprint = await this.deps.sprints.findById(sprintId);
    if (!sprint) {
      throw new DomainError("NOT_FOUND", "Спринт не найден");
    }
    const project = await this.deps.projects.findById(sprint.projectId);
    if (!project || !(await this.deps.teams.isMember(project.teamId, userId))) {
      throw new DomainError("FORBIDDEN", "Нет доступа к спринту");
    }
    return sprint;
  }
}

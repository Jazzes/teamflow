import {
  createDatabase,
  DrizzleProjectRepository,
  DrizzleSprintRepository,
  DrizzleTaskRepository,
  DrizzleTeamRepository,
  DrizzleTimeEntryRepository,
} from "@teamflow/dal";
import { ProjectService, SprintService, TaskService } from "@teamflow/bll";

export interface Services {
  projects: ProjectService;
  tasks: TaskService;
  sprints: SprintService;
}

export interface Container {
  services: Services;
  close(): Promise<void>;
}

/** Корень композиции: единственное место, где API знает о конкретных репозиториях DAL. */
export function createContainer(databaseUrl: string): Container {
  const connection = createDatabase(databaseUrl);
  const repositories = {
    projects: new DrizzleProjectRepository(connection.db),
    teams: new DrizzleTeamRepository(connection.db),
    sprints: new DrizzleSprintRepository(connection.db),
    tasks: new DrizzleTaskRepository(connection.db),
    timeEntries: new DrizzleTimeEntryRepository(connection.db),
  };
  return {
    services: {
      projects: new ProjectService(repositories),
      tasks: new TaskService(repositories),
      sprints: new SprintService(repositories),
    },
    close: () => connection.close(),
  };
}

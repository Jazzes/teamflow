import { randomUUID } from "node:crypto";
import { beforeEach, describe, expect, it } from "vitest";
import { SprintService } from "../src/services/SprintService.js";
import { TaskService } from "../src/services/TaskService.js";
import { createWorld } from "./fakes.js";

describe("SprintService", () => {
  let world: ReturnType<typeof createWorld>;
  let sprints: SprintService;
  let tasks: TaskService;

  beforeEach(() => {
    world = createWorld();
    sprints = new SprintService(world);
    tasks = new TaskService(world);
  });

  it("запускает запланированный спринт", async () => {
    const started = await sprints.startSprint(world.sprint.id, world.leadId);
    expect(started.status).toBe("active");
  });

  it("не запускает второй активный спринт в проекте", async () => {
    await sprints.startSprint(world.sprint.id, world.leadId);
    world.sprints.rows.push({
      id: randomUUID(),
      projectId: world.project.id,
      name: "Спринт 2",
      startsOn: "2026-09-28",
      endsOn: "2026-10-11",
      status: "planned",
    });
    const second = world.sprints.rows[1];
    if (!second) {
      throw new Error("второй спринт не добавлен");
    }
    await expect(sprints.startSprint(second.id, world.leadId)).rejects.toMatchObject({ code: "CONFLICT" });
  });

  it("при закрытии возвращает незавершённые задачи в бэклог", async () => {
    await sprints.startSprint(world.sprint.id, world.leadId);
    const done = await tasks.createTask(world.project.id, world.leadId, {
      title: "Готовая задача",
      sprintId: world.sprint.id,
      assigneeId: world.developerId,
    });
    done.status = "done";
    await tasks.createTask(world.project.id, world.leadId, { title: "Недоделанная", sprintId: world.sprint.id });

    const result = await sprints.closeSprint(world.sprint.id, world.leadId);
    expect(result.sprint.status).toBe("closed");
    expect(result.movedToBacklog).toBe(1);
    expect(world.tasks.rows.find((t) => t.title === "Недоделанная")?.status).toBe("backlog");
  });

  it("не даёт управлять спринтом пользователю не из команды", async () => {
    await expect(sprints.startSprint(world.sprint.id, world.outsiderId)).rejects.toMatchObject({
      code: "FORBIDDEN",
    });
  });
});

import { randomUUID } from "node:crypto";
import { beforeEach, describe, expect, it } from "vitest";
import { DomainError } from "../src/errors.js";
import { TaskService } from "../src/services/TaskService.js";
import { createWorld } from "./fakes.js";

describe("TaskService", () => {
  let world: ReturnType<typeof createWorld>;
  let service: TaskService;

  beforeEach(() => {
    world = createWorld();
    service = new TaskService(world);
  });

  it("создаёт задачу в бэклоге с порядковым номером", async () => {
    const first = await service.createTask(world.project.id, world.leadId, { title: "Настроить CI" });
    const second = await service.createTask(world.project.id, world.leadId, { title: "Схема БД" });
    expect(first.number).toBe(1);
    expect(second.number).toBe(2);
    expect(first.status).toBe("backlog");
    expect(first.priority).toBe("medium");
  });

  it("задача, созданная сразу в спринте, получает статус todo", async () => {
    const task = await service.createTask(world.project.id, world.leadId, {
      title: "Эндпоинт задач",
      sprintId: world.sprint.id,
    });
    expect(task.status).toBe("todo");
    expect(task.sprintId).toBe(world.sprint.id);
  });

  it("не пускает в проект пользователя не из команды", async () => {
    await expect(
      service.createTask(world.project.id, world.outsiderId, { title: "Чужая задача" }),
    ).rejects.toMatchObject({ code: "FORBIDDEN" });
  });

  it("не даёт назначить исполнителем человека не из команды", async () => {
    await expect(
      service.createTask(world.project.id, world.leadId, { title: "Задача", assigneeId: world.outsiderId }),
    ).rejects.toMatchObject({ code: "VALIDATION" });
  });

  it("отклоняет слишком короткий заголовок", async () => {
    const error = await service.createTask(world.project.id, world.leadId, { title: "ab" }).catch((e: unknown) => e);
    expect(error).toBeInstanceOf(DomainError);
    expect((error as DomainError).code).toBe("VALIDATION");
  });

  it("не добавляет задачу в закрытый спринт", async () => {
    world.sprint.status = "closed";
    await expect(
      service.createTask(world.project.id, world.leadId, { title: "Поздняя задача", sprintId: world.sprint.id }),
    ).rejects.toMatchObject({ code: "CONFLICT" });
  });

  it("меняет статус по правилам доски и пишет историю", async () => {
    const task = await service.createTask(world.project.id, world.leadId, {
      title: "Авторизация",
      assigneeId: world.developerId,
    });
    await service.changeStatus(task.id, world.developerId, { status: "todo" });
    const moved = await service.changeStatus(task.id, world.developerId, { status: "in_progress" });
    expect(moved.status).toBe("in_progress");
    expect(world.tasks.history).toHaveLength(2);
    expect(world.tasks.history[1]).toMatchObject({ fromStatus: "todo", toStatus: "in_progress" });
  });

  it("запрещает недопустимый переход статуса", async () => {
    const task = await service.createTask(world.project.id, world.leadId, { title: "Рефакторинг" });
    await expect(service.changeStatus(task.id, world.leadId, { status: "done" })).rejects.toMatchObject({
      code: "CONFLICT",
    });
  });

  it("не закрывает задачу без исполнителя", async () => {
    const task = await service.createTask(world.project.id, world.leadId, { title: "Без исполнителя" });
    task.status = "review";
    await expect(service.changeStatus(task.id, world.leadId, { status: "done" })).rejects.toMatchObject({
      code: "CONFLICT",
    });
  });

  it("учитывает время и проверяет диапазон минут", async () => {
    const task = await service.createTask(world.project.id, world.leadId, { title: "Код-ревью" });
    const entry = await service.logTime(task.id, world.developerId, { minutes: 90, spentOn: "2026-09-15" });
    expect(entry.minutes).toBe(90);
    await expect(
      service.logTime(task.id, world.developerId, { minutes: 2000, spentOn: "2026-09-15" }),
    ).rejects.toMatchObject({ code: "VALIDATION" });
  });

  it("возвращает NOT_FOUND для несуществующей задачи", async () => {
    await expect(service.changeStatus(randomUUID(), world.leadId, { status: "todo" })).rejects.toMatchObject({
      code: "NOT_FOUND",
    });
  });
});

import { beforeEach, describe, expect, it } from "vitest";
import { ProjectService } from "../src/services/ProjectService.js";
import { createWorld } from "./fakes.js";

describe("ProjectService", () => {
  let world: ReturnType<typeof createWorld>;
  let service: ProjectService;

  beforeEach(() => {
    world = createWorld();
    service = new ProjectService(world);
  });

  it("создаёт проект и приводит ключ к верхнему регистру", async () => {
    const project = await service.createProject(world.leadId, {
      teamId: world.teamId,
      key: "crm",
      name: "CRM для отдела продаж",
    });
    expect(project.key).toBe("CRM");
  });

  it("не создаёт проект с занятым ключом", async () => {
    await expect(
      service.createProject(world.leadId, { teamId: world.teamId, key: "TF", name: "Дубликат" }),
    ).rejects.toMatchObject({ code: "CONFLICT" });
  });

  it("не показывает задачи проекта постороннему пользователю", async () => {
    await expect(service.listTasks(world.project.id, world.outsiderId)).rejects.toMatchObject({
      code: "FORBIDDEN",
    });
  });
});

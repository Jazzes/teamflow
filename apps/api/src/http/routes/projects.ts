import { Router } from "express";
import type { Services } from "../../container.js";
import { currentUserId } from "../auth.js";
import { uuidParam } from "../params.js";

export function projectsRouter(services: Services): Router {
  const router = Router();
  router.param("projectId", uuidParam);

  router.post("/", async (req, res) => {
    const project = await services.projects.createProject(currentUserId(res), req.body);
    res.status(201).json(project);
  });

  router.get("/:projectId/tasks", async (req, res) => {
    const tasks = await services.projects.listTasks(req.params.projectId, currentUserId(res));
    res.json(tasks);
  });

  router.post("/:projectId/tasks", async (req, res) => {
    const task = await services.tasks.createTask(req.params.projectId, currentUserId(res), req.body);
    res.status(201).json(task);
  });

  return router;
}

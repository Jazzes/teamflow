import { Router } from "express";
import type { Services } from "../../container.js";
import { currentUserId } from "../auth.js";
import { uuidParam } from "../params.js";

export function tasksRouter(services: Services): Router {
  const router = Router();
  router.param("taskId", uuidParam);

  router.patch("/:taskId/status", async (req, res) => {
    const task = await services.tasks.changeStatus(req.params.taskId, currentUserId(res), req.body);
    res.json(task);
  });

  router.post("/:taskId/time-entries", async (req, res) => {
    const entry = await services.tasks.logTime(req.params.taskId, currentUserId(res), req.body);
    res.status(201).json(entry);
  });

  return router;
}

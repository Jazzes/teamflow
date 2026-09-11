import { Router } from "express";
import type { Services } from "../../container.js";
import { currentUserId } from "../auth.js";
import { uuidParam } from "../params.js";

export function sprintsRouter(services: Services): Router {
  const router = Router();
  router.param("sprintId", uuidParam);

  router.post("/:sprintId/start", async (req, res) => {
    const sprint = await services.sprints.startSprint(req.params.sprintId, currentUserId(res));
    res.json(sprint);
  });

  router.post("/:sprintId/close", async (req, res) => {
    const result = await services.sprints.closeSprint(req.params.sprintId, currentUserId(res));
    res.json(result);
  });

  return router;
}

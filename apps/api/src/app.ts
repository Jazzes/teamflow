import express, { type Express } from "express";
import type { Services } from "./container.js";
import { requireUser } from "./http/auth.js";
import { errorHandler } from "./http/errors.js";
import { projectsRouter } from "./http/routes/projects.js";
import { sprintsRouter } from "./http/routes/sprints.js";
import { tasksRouter } from "./http/routes/tasks.js";

/** Собирает Express-приложение. Сервисы передаются снаружи, поэтому приложение легко тестировать. */
export function createApp(services: Services): Express {
  const app = express();
  app.disable("x-powered-by");
  app.use(express.json({ limit: "100kb" }));

  app.get("/api/health", (_req, res) => {
    res.json({ status: "ok" });
  });

  app.use("/api", requireUser);
  app.use("/api/projects", projectsRouter(services));
  app.use("/api/tasks", tasksRouter(services));
  app.use("/api/sprints", sprintsRouter(services));

  app.use((_req, res) => {
    res.status(404).json({ error: "NOT_FOUND", message: "Маршрут не найден" });
  });
  app.use(errorHandler);
  return app;
}

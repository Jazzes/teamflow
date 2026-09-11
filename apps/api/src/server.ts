import { createApp } from "./app.js";
import { loadConfig } from "./config.js";
import { createContainer } from "./container.js";

const config = loadConfig();
const container = createContainer(config.databaseUrl);
const app = createApp(container.services);

const server = app.listen(config.port, () => {
  console.log(`TeamFlow API слушает http://localhost:${config.port}`);
});

async function shutdown(signal: string): Promise<void> {
  console.log(`Получен ${signal}, останавливаю сервер`);
  server.close();
  await container.close();
  process.exit(0);
}

process.on("SIGINT", () => void shutdown("SIGINT"));
process.on("SIGTERM", () => void shutdown("SIGTERM"));

export interface AppConfig {
  port: number;
  databaseUrl: string;
}

/** Читает настройки из переменных окружения. Пароли и строки подключения в коде не хранятся. */
export function loadConfig(env: NodeJS.ProcessEnv = process.env): AppConfig {
  const databaseUrl = env.DATABASE_URL;
  if (!databaseUrl) {
    throw new Error("Не задана переменная окружения DATABASE_URL");
  }
  const port = Number(env.PORT ?? 3000);
  if (!Number.isInteger(port) || port <= 0) {
    throw new Error(`Некорректный PORT: ${env.PORT}`);
  }
  return { port, databaseUrl };
}

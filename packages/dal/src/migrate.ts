import { fileURLToPath } from "node:url";
import { migrate } from "drizzle-orm/node-postgres/migrator";
import { createDatabase } from "./client.js";

const connectionString = process.env.DATABASE_URL;
if (!connectionString) {
  throw new Error("Не задана переменная окружения DATABASE_URL");
}

const migrationsFolder = fileURLToPath(new URL("../migrations", import.meta.url));
const connection = createDatabase(connectionString);

try {
  await migrate(connection.db, { migrationsFolder });
  console.log("Миграции применены");
} finally {
  await connection.close();
}

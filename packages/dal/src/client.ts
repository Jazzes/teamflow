import { drizzle, type NodePgDatabase } from "drizzle-orm/node-postgres";
import pg from "pg";
import * as schema from "./schema.js";

export type Database = NodePgDatabase<typeof schema>;

export interface DatabaseConnection {
  db: Database;
  close(): Promise<void>;
}

/** Создаёт пул соединений PostgreSQL. Строка подключения приходит из окружения, а не из кода. */
export function createDatabase(connectionString: string): DatabaseConnection {
  const pool = new pg.Pool({ connectionString, max: 10 });
  const db = drizzle(pool, { schema });
  return {
    db,
    close: () => pool.end(),
  };
}

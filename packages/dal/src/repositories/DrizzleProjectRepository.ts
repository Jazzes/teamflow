import { eq } from "drizzle-orm";
import type { Database } from "../client.js";
import type { ProjectRepository } from "../contracts.js";
import { projects } from "../schema.js";
import type { NewProject, Project } from "../types.js";

export class DrizzleProjectRepository implements ProjectRepository {
  private readonly db: Database;

  constructor(db: Database) {
    this.db = db;
  }

  async findById(id: string): Promise<Project | null> {
    const [row] = await this.db.select().from(projects).where(eq(projects.id, id)).limit(1);
    return row ?? null;
  }

  async findByKey(key: string): Promise<Project | null> {
    const [row] = await this.db.select().from(projects).where(eq(projects.key, key)).limit(1);
    return row ?? null;
  }

  async create(data: NewProject): Promise<Project> {
    const [row] = await this.db.insert(projects).values(data).returning();
    if (!row) {
      throw new Error("INSERT в projects не вернул строку");
    }
    return row;
  }
}

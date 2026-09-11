import { and, eq } from "drizzle-orm";
import type { Database } from "../client.js";
import type { SprintRepository } from "../contracts.js";
import { sprints } from "../schema.js";
import type { Sprint, SprintStatus } from "../types.js";

export class DrizzleSprintRepository implements SprintRepository {
  private readonly db: Database;

  constructor(db: Database) {
    this.db = db;
  }

  async findById(id: string): Promise<Sprint | null> {
    const [row] = await this.db.select().from(sprints).where(eq(sprints.id, id)).limit(1);
    return row ?? null;
  }

  async findActiveByProject(projectId: string): Promise<Sprint | null> {
    const [row] = await this.db
      .select()
      .from(sprints)
      .where(and(eq(sprints.projectId, projectId), eq(sprints.status, "active")))
      .limit(1);
    return row ?? null;
  }

  async updateStatus(id: string, status: SprintStatus): Promise<Sprint> {
    const [row] = await this.db.update(sprints).set({ status }).where(eq(sprints.id, id)).returning();
    if (!row) {
      throw new Error(`Спринт ${id} не найден при обновлении статуса`);
    }
    return row;
  }
}

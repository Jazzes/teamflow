import { and, eq } from "drizzle-orm";
import type { Database } from "../client.js";
import type { TeamRepository } from "../contracts.js";
import { teamMembers } from "../schema.js";

export class DrizzleTeamRepository implements TeamRepository {
  private readonly db: Database;

  constructor(db: Database) {
    this.db = db;
  }

  async isMember(teamId: string, userId: string): Promise<boolean> {
    const rows = await this.db
      .select({ userId: teamMembers.userId })
      .from(teamMembers)
      .where(and(eq(teamMembers.teamId, teamId), eq(teamMembers.userId, userId)))
      .limit(1);
    return rows.length > 0;
  }
}

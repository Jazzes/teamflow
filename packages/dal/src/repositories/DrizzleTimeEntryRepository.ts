import type { Database } from "../client.js";
import type { TimeEntryRepository } from "../contracts.js";
import { timeEntries } from "../schema.js";
import type { NewTimeEntry, TimeEntry } from "../types.js";

export class DrizzleTimeEntryRepository implements TimeEntryRepository {
  private readonly db: Database;

  constructor(db: Database) {
    this.db = db;
  }

  async create(data: NewTimeEntry): Promise<TimeEntry> {
    const [row] = await this.db.insert(timeEntries).values(data).returning();
    if (!row) {
      throw new Error("INSERT в time_entries не вернул строку");
    }
    return row;
  }
}

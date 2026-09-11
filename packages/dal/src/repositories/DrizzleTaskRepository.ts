import { and, asc, eq, ne, sql } from "drizzle-orm";
import type { Database } from "../client.js";
import type { TaskRepository } from "../contracts.js";
import { tasks, taskStatusChanges } from "../schema.js";
import type { NewTask, NewTaskStatusChange, Task, TaskStatus } from "../types.js";

export class DrizzleTaskRepository implements TaskRepository {
  private readonly db: Database;

  constructor(db: Database) {
    this.db = db;
  }

  async findById(id: string): Promise<Task | null> {
    const [row] = await this.db.select().from(tasks).where(eq(tasks.id, id)).limit(1);
    return row ?? null;
  }

  async listByProject(projectId: string): Promise<Task[]> {
    return this.db.select().from(tasks).where(eq(tasks.projectId, projectId)).orderBy(asc(tasks.number));
  }

  async nextNumber(projectId: string): Promise<number> {
    const [row] = await this.db
      .select({ max: sql<number>`coalesce(max(${tasks.number}), 0)::int` })
      .from(tasks)
      .where(eq(tasks.projectId, projectId));
    return (row?.max ?? 0) + 1;
  }

  async create(data: NewTask): Promise<Task> {
    const [row] = await this.db.insert(tasks).values(data).returning();
    if (!row) {
      throw new Error("INSERT в tasks не вернул строку");
    }
    return row;
  }

  async updateStatus(id: string, status: TaskStatus): Promise<Task> {
    const [row] = await this.db
      .update(tasks)
      .set({ status, updatedAt: new Date() })
      .where(eq(tasks.id, id))
      .returning();
    if (!row) {
      throw new Error(`Задача ${id} не найдена при обновлении статуса`);
    }
    return row;
  }

  async moveUnfinishedToBacklog(sprintId: string): Promise<number> {
    const moved = await this.db
      .update(tasks)
      .set({ sprintId: null, status: "backlog", updatedAt: new Date() })
      .where(and(eq(tasks.sprintId, sprintId), ne(tasks.status, "done")))
      .returning({ id: tasks.id });
    return moved.length;
  }

  async addStatusChange(data: NewTaskStatusChange): Promise<void> {
    await this.db.insert(taskStatusChanges).values(data);
  }
}

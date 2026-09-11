import { z } from "zod";
import { DomainError } from "./errors.js";

const isoDate = z.string().regex(/^\d{4}-\d{2}-\d{2}$/, "Дата в формате ГГГГ-ММ-ДД");

export const createProjectSchema = z.object({
  teamId: z.uuid(),
  key: z
    .string()
    .trim()
    .toUpperCase()
    .regex(/^[A-Z]{2,10}$/, "Ключ проекта: от 2 до 10 латинских букв"),
  name: z.string().trim().min(3).max(150),
  description: z.string().trim().max(5000).optional(),
});

export const createTaskSchema = z.object({
  title: z.string().trim().min(3).max(200),
  description: z.string().trim().max(10000).optional(),
  priority: z.enum(["low", "medium", "high", "critical"]).default("medium"),
  storyPoints: z.number().int().min(0).max(100).optional(),
  assigneeId: z.uuid().optional(),
  sprintId: z.uuid().optional(),
  dueDate: isoDate.optional(),
});

export const changeStatusSchema = z.object({
  status: z.enum(["backlog", "todo", "in_progress", "review", "done"]),
});

export const logTimeSchema = z.object({
  minutes: z.number().int().min(1).max(1440),
  spentOn: isoDate,
  note: z.string().trim().max(500).optional(),
});

export type CreateProjectInput = z.input<typeof createProjectSchema>;
export type CreateTaskInput = z.input<typeof createTaskSchema>;
export type ChangeStatusInput = z.input<typeof changeStatusSchema>;
export type LogTimeInput = z.input<typeof logTimeSchema>;

/** Проверяет входные данные схемой zod и превращает ошибки в DomainError с кодом VALIDATION. */
export function parseInput<T extends z.ZodType>(schema: T, input: unknown): z.output<T> {
  const result = schema.safeParse(input);
  if (!result.success) {
    throw new DomainError("VALIDATION", "Некорректные входные данные", z.flattenError(result.error));
  }
  return result.data;
}

import type { TaskStatus } from "@teamflow/dal";

/** Разрешённые переходы между статусами задачи на доске. */
const transitions: Record<TaskStatus, readonly TaskStatus[]> = {
  backlog: ["todo"],
  todo: ["in_progress", "backlog"],
  in_progress: ["review", "todo"],
  review: ["done", "in_progress"],
  done: ["in_progress"],
};

export function canTransition(from: TaskStatus, to: TaskStatus): boolean {
  return transitions[from].includes(to);
}

export function allowedTransitions(from: TaskStatus): readonly TaskStatus[] {
  return transitions[from];
}

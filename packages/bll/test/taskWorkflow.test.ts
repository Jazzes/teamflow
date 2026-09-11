import { describe, expect, it } from "vitest";
import { allowedTransitions, canTransition } from "../src/taskWorkflow.js";

describe("taskWorkflow", () => {
  it("разрешает движение задачи по доске вперёд", () => {
    expect(canTransition("backlog", "todo")).toBe(true);
    expect(canTransition("todo", "in_progress")).toBe(true);
    expect(canTransition("in_progress", "review")).toBe(true);
    expect(canTransition("review", "done")).toBe(true);
  });

  it("запрещает перепрыгивать через этапы", () => {
    expect(canTransition("backlog", "done")).toBe(false);
    expect(canTransition("todo", "review")).toBe(false);
  });

  it("позволяет переоткрыть закрытую задачу только в работу", () => {
    expect(allowedTransitions("done")).toEqual(["in_progress"]);
  });
});

export type DomainErrorCode = "NOT_FOUND" | "VALIDATION" | "FORBIDDEN" | "CONFLICT";

/** Ошибка бизнес-правила. Слой представления сам решает, в какой HTTP-статус её превратить. */
export class DomainError extends Error {
  readonly code: DomainErrorCode;
  readonly details: unknown;

  constructor(code: DomainErrorCode, message: string, details?: unknown) {
    super(message);
    this.name = "DomainError";
    this.code = code;
    this.details = details;
  }
}

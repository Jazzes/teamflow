import type { NextFunction, Request, Response } from "express";
import { DomainError, type DomainErrorCode } from "@teamflow/bll";

const statusByCode: Record<DomainErrorCode, number> = {
  VALIDATION: 400,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  CONFLICT: 409,
};

/** Переводит ошибки бизнес-логики в HTTP-ответы, остальные ошибки логирует и скрывает детали. */
export function errorHandler(err: unknown, _req: Request, res: Response, _next: NextFunction): void {
  if (err instanceof DomainError) {
    res.status(statusByCode[err.code]).json({ error: err.code, message: err.message, details: err.details });
    return;
  }
  if (err instanceof SyntaxError && "body" in err) {
    res.status(400).json({ error: "VALIDATION", message: "Тело запроса не является корректным JSON" });
    return;
  }
  console.error(err);
  res.status(500).json({ error: "INTERNAL", message: "Внутренняя ошибка сервера" });
}

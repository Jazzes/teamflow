import type { NextFunction, Request, Response } from "express";

const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/** Проверяет, что идентификатор в пути является UUID, до обращения к бизнес-логике и БД. */
export function uuidParam(_req: Request, res: Response, next: NextFunction, value: string, name: string): void {
  if (!uuidPattern.test(value)) {
    res.status(400).json({ error: "VALIDATION", message: `Параметр ${name} должен быть UUID` });
    return;
  }
  next();
}

export { uuidPattern };

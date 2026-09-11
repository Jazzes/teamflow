import type { NextFunction, Request, Response } from "express";
import { uuidPattern } from "./params.js";

/**
 * Определяет текущего пользователя по заголовку X-User-Id.
 * На этапе структуры проекта это временная схема; в следующих работах её заменит JWT.
 */
export function requireUser(req: Request, res: Response, next: NextFunction): void {
  const userId = req.header("x-user-id");
  if (!userId || !uuidPattern.test(userId)) {
    res.status(401).json({ error: "UNAUTHORIZED", message: "Нужен заголовок X-User-Id с UUID пользователя" });
    return;
  }
  res.locals.userId = userId;
  next();
}

export function currentUserId(res: Response): string {
  return res.locals.userId as string;
}

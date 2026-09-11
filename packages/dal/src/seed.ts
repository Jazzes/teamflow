import { createDatabase } from "./client.js";
import { labels, projects, sprints, teamMembers, teams, users } from "./schema.js";

const connectionString = process.env.DATABASE_URL;
if (!connectionString) {
  throw new Error("Не задана переменная окружения DATABASE_URL");
}

const connection = createDatabase(connectionString);

try {
  const result = await connection.db.transaction(async (tx) => {
    const [lead, developer] = await tx
      .insert(users)
      .values([
        { email: "lead@teamflow.local", fullName: "Иванов Илья", passwordHash: "seed-no-login" },
        { email: "dev@teamflow.local", fullName: "Петрова Анна", passwordHash: "seed-no-login" },
      ])
      .returning();
    if (!lead || !developer) {
      throw new Error("Не удалось создать пользователей");
    }
    const [team] = await tx.insert(teams).values({ name: "Команда TeamFlow" }).returning();
    if (!team) {
      throw new Error("Не удалось создать команду");
    }
    await tx.insert(teamMembers).values([
      { teamId: team.id, userId: lead.id, role: "lead" },
      { teamId: team.id, userId: developer.id, role: "developer" },
    ]);
    const [project] = await tx
      .insert(projects)
      .values({ teamId: team.id, key: "TF", name: "TeamFlow", description: "Учебный проект" })
      .returning();
    if (!project) {
      throw new Error("Не удалось создать проект");
    }
    const [sprint] = await tx
      .insert(sprints)
      .values({ projectId: project.id, name: "Спринт 1", startsOn: "2026-09-14", endsOn: "2026-09-27" })
      .returning();
    await tx.insert(labels).values([
      { projectId: project.id, name: "bug", color: "#DC2626" },
      { projectId: project.id, name: "feature", color: "#2563EB" },
    ]);
    return { leadId: lead.id, developerId: developer.id, projectId: project.id, sprintId: sprint?.id };
  });
  console.log(JSON.stringify(result, null, 2));
} finally {
  await connection.close();
}

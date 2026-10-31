# My Research Desk

A personal knowledge-management app for capturing the takeaways from articles, announcements and research.
ASP.NET Core 9 Razor Pages, EF Core, PostgreSQL, Docker Compose.

## Run

```powershell
docker compose up -d --build
```

- App: http://localhost:8080
- Swagger / API: http://localhost:8080/swagger (`/api/articles`, full CRUD)
- PostgreSQL: `localhost:5432` (db/user/password: `researchdesk`), persisted in the `pgdata` volume

EF Core migrations are applied automatically at startup. Stop with `docker compose down`
(add `-v` to also delete the data).

## Develop

Requires the .NET 9 SDK and a PostgreSQL instance (the compose `db` service works: `docker compose up -d db`).

```powershell
dotnet run --project src/MyResearchDesk.Web
dotnet tool restore
dotnet ef migrations add <Name> --project src/MyResearchDesk.Web
```

The connection string is `ConnectionStrings:Default` (override with the `ConnectionStrings__Default` env var).

## Test

```powershell
dotnet test
```

Integration tests start a throwaway PostgreSQL container (Docker must be running). They cover CRUD,
validation, search, filters, dashboard counts, and search speed over 1,000 articles.

## Features

- Create, view, edit and delete articles (title, URL, topic, source, status, takeaway)
- Search across title, topic, source and takeaway (case-insensitive, partial match)
- Filter by status (To Read, Read, Worth Acting On) and topic
- Dashboard with counts and recent articles

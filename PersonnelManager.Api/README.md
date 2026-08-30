# PersonnelManager.Api

A REST / OpenAPI backend for the Personnel domain. It is a thin HTTP host over the existing
`PersonnelManager` core library (domain, application service, FluentValidation, EF Core / PostgreSQL
store) — the API adds routing, JWT authentication, request validation, and OpenAPI documentation.

## Features

- **REST + OpenAPI** — attribute-routed controllers; interactive Swagger UI at `/swagger`.
- **Auth** — JWT bearer with **role-based** authorization (`Admin`, `User`). `POST /api/auth/login`
  issues a signed token; all other endpoints require it. Destructive operations require `Admin`.
- **Validation** — FluentValidation at the HTTP boundary (a global filter returning RFC 7807
  `ValidationProblemDetails`), plus the domain's own rules inside the application service.
- **CRUD + operations** — create, read, update, delete, plus status change, search/filter,
  JSON backup and restore.
- **Routing** — `/api/auth`, `/api/personnel`, and `/health`.
- **Store** — PostgreSQL via EF Core by default (connection string `ConnectionStrings:Personnel`);
  clear the connection string to fall back to an in-memory store for zero-setup runs.

## Endpoints

| Method | Route                          | Role         | Purpose                       |
|--------|--------------------------------|--------------|-------------------------------|
| POST   | `/api/auth/login`              | anonymous    | Get a JWT bearer token        |
| GET    | `/api/personnel`               | User/Admin   | List (`?status=&name=` filter)|
| GET    | `/api/personnel/{id}`          | User/Admin   | Get one                       |
| POST   | `/api/personnel`               | User/Admin   | Create                        |
| PUT    | `/api/personnel/{id}`          | User/Admin   | Update                        |
| PATCH  | `/api/personnel/{id}/status`   | User/Admin   | Change employment status      |
| DELETE | `/api/personnel/{id}`          | **Admin**    | Delete                        |
| POST   | `/api/personnel/backup`        | **Admin**    | Save all personnel to JSON    |
| POST   | `/api/personnel/restore`       | **Admin**    | Restore personnel from JSON   |
| GET    | `/health`                      | anonymous    | Liveness + data-store check   |

## Run

```bash
# PostgreSQL (default): set the connection string in appsettings.json, then:
dotnet run --project PersonnelManager.Api

# Or run against the in-memory store (no database needed):
ConnectionStrings__Personnel="" dotnet run --project PersonnelManager.Api
```

Open <http://localhost:5080/swagger>, call **`POST /api/auth/login`** with the demo credentials,
click **Authorize**, paste the token, and try the personnel endpoints.

## Demo credentials (from `appsettings.json`)

| Username | Password   | Roles         |
|----------|------------|---------------|
| `admin`  | `admin123` | Admin, User   |
| `user`   | `user123`  | User          |

> Change `Jwt:SigningKey` and the demo users before any real deployment (use user-secrets / env vars).

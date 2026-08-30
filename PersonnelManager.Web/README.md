# PersonnelManager.Web

A standalone ASP.NET Core **MVC** front-end that consumes **PersonnelManager.Api** over HTTP.
It is a true API client: it does **not** reference the API or core projects — it talks to the REST
endpoints through a typed `HttpClient`, with its own small view models.

## How it works

- **Typed client** — `ApiClient/PersonnelApiClient.cs` wraps the REST calls (login, list, get, create,
  update, delete, backup) and turns non-success responses into `ApiException` (with per-field errors).
- **Auth** — `AccountController.Login` posts credentials to `POST /api/auth/login`, then stores the
  returned JWT (plus the role claims decoded from it) in a **cookie**. `ApiClient/BearerTokenHandler.cs`
  reads that token from the signed-in user and attaches it as `Authorization: Bearer …` on every API call.
- **Authorization** — `PersonnelController` requires a signed-in user (`[Authorize]`); its `Delete`
  actions and the "Back up" action are `Admin`-only (and the API enforces the same regardless of the UI).
- **Validation** — client-side data annotations plus the API's own validation errors, surfaced in the
  view's validation summary.

## Routes

| Route                       | Purpose                                        |
|-----------------------------|------------------------------------------------|
| `/Account/Login`            | Log in (obtains + stores the JWT)              |
| `/Personnel/Index`          | List with search (`name`) + status filter      |
| `/Personnel/Create`         | Add a person                                   |
| `/Personnel/Edit/{id}`      | Edit fields + change status                    |
| `/Personnel/Delete/{id}`    | Delete (Admin only)                            |

## Run (both apps)

The web app calls the API at `Api:BaseUrl` (default `http://localhost:5080`).

```bash
# Terminal 1 — the API (in-memory store, no database needed):
ConnectionStrings__Personnel="" dotnet run --project PersonnelManager.Api   # http://localhost:5080

# Terminal 2 — the web front-end:
dotnet run --project PersonnelManager.Web                                    # http://localhost:5090
```

Open <http://localhost:5090>, log in as `admin` / `admin123` (Admin) or `user` / `user123` (User).

> Point `Api:BaseUrl` in `appsettings.json` at wherever the API is hosted to use a different backend.

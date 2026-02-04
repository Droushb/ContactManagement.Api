# ContactManagement.Api

ASP.NET Core 8 Web API for managing contacts with optional custom fields. Uses Entity Framework Core and SQL Server.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server

## Run the API

```bash
cd ContactManagement
dotnet run
```

Swagger UI: **https://localhost:&lt;port&gt;/swagger** (port from `launchSettings.json` or console).

Database is created/updated automatically on startup via EF Core migrations. Set the connection string in `appsettings.json` or `appsettings.Development.json` under `ConnectionStrings:DefaultConnection`.

## API Overview

| Resource | Endpoints |
|----------|-----------|
| **Contacts** | `GET /api/contacts` (paged, sort, filter), `GET /api/contacts/{id}`, `POST`, `PUT`, `DELETE`, `POST /api/contacts/merge` |
| **Custom fields** | `GET /api/customfields`, `GET /api/customfields/{id}`, `POST`, `PUT`, `DELETE` |

- **Contacts**: FirstName, LastName, Email (unique), Phone; optional custom field values per contact.
- **Custom fields**: Name and type (String, Int, Bool). Define once, then assign values on contacts.
- **Bulk merge**: `POST /api/contacts/merge` with `{ "contactIds": ["...", "..."] }` merges contacts that share the same email (master = oldest by CreatedAt).

## Tests

```bash
dotnet test
```

Integration tests use `ContactManagement.Api.Tests` and an in-memory database (no SQL Server required).

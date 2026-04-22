# SteamHeart

A database and analytics platform for Steam game reviews. Track review sentiment, detect review bombs, and analyze player feedback over time.

---

## Tech Stack

- **Backend:** ASP.NET Core Web API (.NET 10)
- **Database:** PostgreSQL 16
- **Frontend:** Next.js 15 + Tailwind CSS
- **ORM:** Entity Framework Core + Npgsql
- **Deployed on:** Render

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 16](https://www.postgresql.org/download/)
- [Node.js 18+](https://nodejs.org/)
- [dotnet-ef CLI tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

Install the EF CLI tool if you don't have it:

```bash
dotnet tool install --global dotnet-ef
```

---

## 1. Clone the repository

```bash
git clone <repo-url>
cd steam_alerts
```

---

## 2. Set up PostgreSQL

If PostgreSQL is not running, start it:

```bash
# macOS (Homebrew)
brew services start postgresql@16

# Ubuntu/Debian
sudo service postgresql start
```

Create a database and user:

```sql
psql -U postgres

CREATE DATABASE steamheart;
CREATE USER steamheart_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE steamheart TO steamheart_user;
\q
```

---

## 3. Configure the API

Edit `SteamHeartAPI/appsettings.json` and fill in your values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=steamheart;Username=steamheart_user;Password=your_password"
  },
  "AllowedOrigins": "http://localhost:3000",
  "X_API_KEY": "choose-a-secret-key"
}
```

> `X_API_KEY` is the key you will send with every API request. Choose any strong random string.

---

## 4. Run database migrations

This creates all the tables and indexes in your PostgreSQL database:

```bash
cd SteamHeartAPI
dotnet ef database update
```

You should see EF Core apply each migration in order, ending with:

```
Done.
```

---

## 5. Run the API

```bash
cd SteamHeartAPI
dotnet run
```

The API will be available at `http://localhost:5116`. You can browse the interactive API docs at:

```
http://localhost:5116/scalar/v1?X-Api-Key=choose-a-secret-key
```

---

## 6. Seed the game catalog (optional)

The worker downloads ~100,000 games from Steam and inserts them into your database. This is a one-time bootstrap step and takes several minutes.

Set your API key as an environment variable, then run the worker:

```bash
# macOS/Linux
export X_Api_Key=choose-a-secret-key

# Windows
set X_Api_Key=choose-a-secret-key
```

```bash
cd SteamHeart.Worker
dotnet run
```

The worker will print progress as it uploads batches of 500 games. Once finished you can close it.

---

## 7. Run the frontend

```bash
cd web
npm install
npm run dev
```

The frontend will be available at `http://localhost:3000`.

---

## Environment variables reference

| Variable | Where | Description |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | API | PostgreSQL connection string |
| `X_API_KEY` | API | Secret key required for all API requests |
| `AllowedOrigins` | API | Comma-separated list of allowed CORS origins |
| `X_Api_Key` | Worker | Must match the API's `X_API_KEY` to seed the catalog |

In production these are set as environment variables on your host (e.g. Render environment variables), not in `appsettings.json`.

---

## Project structure

```
steam_alerts/
├── SteamHeartAPI/           # ASP.NET Core Web API
│   ├── controllers/         # HTTP endpoints
│   ├── services/            # Steam API integration
│   ├── models/              # Database entities and API response models
│   ├── data/                # EF Core DbContext
│   ├── Migrations/          # Database migration history
│   └── Dockerfile           # Container build
├── SteamHeart.Worker/       # One-shot catalog seeding tool
└── web/                     # Next.js frontend
```

---

## API overview

All requests require either an `X-Api-Key` header or `?X-Api-Key=` query parameter.

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/games` | List all tracked games (paginated) |
| POST | `/api/games` | Import a single game by Steam AppId |
| POST | `/api/games/batch` | Bulk import games |
| GET | `/api/games/{id}` | Get a single game by internal ID |
| GET | `/api/games/{id}/metrics` | Get historical metrics for a game |
| POST | `/api/metrics/import` | Fetch and store current metrics for a game |
| GET | `/api/games/{id}/reviews` | Get stored reviews for a game (paginated) |
| POST | `/api/games/{id}/reviews/sync` | Sync reviews from Steam for a game |
| GET | `/api/games/{id}/reviews/summary` | Get daily positive/negative review counts |
| GET | `/api/reviews` | Get all reviews across all games (paginated) |

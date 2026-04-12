# BioWira Backend API

A gamified wildlife education backend API built with **ASP.NET Core 9.0 Minimal APIs**. BioWira encourages children to learn about wildlife conservation through interactive weather-adaptive missions, quizzes, progression systems, and achievement badges.

## Tech Stack

| Technology | Purpose |
|---|---|
| ASP.NET Core 9.0 | Minimal API framework |
| PostgreSQL | Database (AWS RDS) |
| Dapper | Lightweight micro-ORM |
| Npgsql | PostgreSQL .NET driver |
| Swagger / OpenAPI | API documentation |
| Docker | Containerised deployment |

## Project Architecture

```
Endpoints → Services → Repositories → PostgreSQL
                ↕
              DTOs / Models / Mappers
```

**Layered architecture** with clean separation of concerns:

- **Endpoints** — Minimal API route handlers with OpenAPI docs
- **Services** — Business logic (mission assignment, score calculation, badge awarding, level-ups)
- **Repositories** — Data access via Dapper + raw SQL
- **Interfaces** — Dependency injection contracts
- **Models** — Entity classes mapped to database tables
- **DTOs** — Request/response objects decoupled from domain models
- **Enums** — `BadgeType`, `BadgeSource`, `DifficultyLevel`, `MissionStatus`
- **Constants** — Animal category definitions
- **Mappers** — Entity-to-DTO mapping logic

## API Endpoints

### Animals — `/api/animals`

| Method | Route | Description |
|---|---|---|
| GET | `/` | Paginated animal list, filterable by category |
| GET | `/{animalId}` | Detailed animal info (habitat, diet, lifespan, conservation status) |
| GET | `/{animalId}/occurrence` | Sighting/occurrence records for an animal |

### Profiles — `/api/profiles`

| Method | Route | Description |
|---|---|---|
| POST | `/` | Create a new child profile (returns profile code + session token) |
| GET | `/` | Auto-login via `X-Session-Token` header |
| POST | `/restore` | Restore profile with profile code + PIN (new device login) |
| DELETE | `/session` | Logout (invalidate session token) |

### Missions — `/api/missions`

| Method | Route | Description |
|---|---|---|
| GET | `/weather-adaptive?weatherCode={code}&isDay={bool}` | Get a random mission adapted to current weather and time of day |
| POST | `/assign` | Assign a mission to a profile |
| POST | `/start` | Mark assigned mission as in-progress |
| POST | `/complete` | Complete mission — awards points, checks level-up, unlocks facts, awards badges |
| GET | `/history` | Last 3 completed missions for the profile |

### Quizzes — `/api/quizzes`

| Method | Route | Description |
|---|---|---|
| GET | `/` | List all available quizzes |
| GET | `/questions/random?count={n}` | Random non-repeated questions across all quizzes |
| GET | `/animals/{animalId}/questions?count={n}` | Random questions for a specific animal |
| GET | `/{quizId}/questions` | All questions with choices and hints for a quiz |
| POST | `/save-progress` | Save quiz results — computes score, updates level, unlocks facts, awards badges |

### Fun Facts — `/api/fun-facts`

| Method | Route | Description |
|---|---|---|
| GET | `/` | All fun facts with personalised lock/unlock status |
| GET | `/{animalId}` | Fun facts for a specific animal |

### Badges — `/api/badges`

| Method | Route | Description |
|---|---|---|
| GET | `/` | Full badge collection with earned status, progress %, and criteria |

## Authentication

Session token-based authentication via the `X-Session-Token` HTTP header.

- Tokens are issued on profile creation or profile restore
- Validated against the `ProfileSession` table on each request
- Supports multi-device sessions via `DeviceInfo`
- Logout invalidates the active session

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL database (or access to the configured AWS RDS instance)

### Run Locally

```bash
# Restore dependencies
dotnet restore

# Run (HTTP — port 5215)
dotnet run --project Deploy --launch-profile http

# Run (HTTPS — port 7251)
dotnet run --project Deploy --launch-profile https
```

Swagger UI is available at `http://localhost:5215/swagger` in development mode.

### Run with Docker

```bash
# Build
docker build -t biowira-api .

# Run (exposes ports 8080 and 8081)
docker run -p 8080:8080 -p 8081:8081 biowira-api
```

### Configuration

Database connection and logging are configured in `Deploy/appsettings.json`. Environment-specific overrides go in `Deploy/appsettings.Development.json`.

## Key Features

- **Weather-adaptive missions** — Outdoor/indoor missions selected based on WMO weather codes and time of day
- **Progression system** — Points, levels, and milestones tracked per profile
- **Badge achievements** — Level-based, mission-based, and special badges
- **Fun fact unlocking** — Educational content unlocked as players progress
- **Quiz engine** — Randomised questions with difficulty levels, hints, and server-side scoring

## Animal Categories

Mammals · Birds · Reptiles · Amphibians · Marine & Freshwater · Insects

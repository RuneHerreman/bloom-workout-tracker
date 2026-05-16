# Bloom

> Self-hosted workout tracker. Plan, log, and analyse your training — strength and cardio.

![Dashboard overview](https://placehold.co/1200x620/3E544B/e8ede9?text=Dashboard)

---

## What it does

**Templates** — Build reusable routines. Set target reps, weight, and RIR for strength sets, or duration and distance for cardio. Templates load directly into a session so you are not starting from scratch every time.

**Logging** — Record actual reps and weight per set as you go. Every session is timestamped and stored in your own database.

**Analytics** — Volume trends over time, personal records by exercise, muscle group breakdown, and a 1-rep-max calculator. All derived from your own logs.

**Tools** — Macro calculator and 1RM estimator as standalone pages.

---

## Pages

| | |
|---|---|
| ![Logbook](https://placehold.co/580x360/3E544B/e8ede9?text=Logbook) | ![Log+detail](https://placehold.co/580x360/3E544B/e8ede9?text=Log+Detail) |
| Logbook — session history | Log detail — sets and notes |
| ![Templates](https://placehold.co/580x360/3E544B/e8ede9?text=Templates) | ![GPX+map](https://placehold.co/580x360/3E544B/e8ede9?text=GPX+Map+Overlay) |
| Template builder | GPX route overlay for cardio |

---

## Stack

| Layer | Technology |
|---|---|
| Frontend | React 19 · TypeScript · Vite |
| Backend | ASP.NET Core 10 (minimal APIs) |
| Database | PostgreSQL 16 |
| Auth | JWT · bcrypt |
| Deployment | Docker Compose |

---

## Self-hosting

### Prerequisites

- [Docker](https://www.docker.com/) with Compose

### Run

```bash
git clone https://github.com/RuneHerreman/bloom-workout-tracker.git
cd bloom-workout-tracker
cp .env.example .env          # fill in JWT__Key and any other values
docker compose up --build -d
```

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API | http://localhost:8080 |
| API docs (Scalar) | http://localhost:8080/scalar |

### Stop / update

```bash
docker compose down
git pull
docker compose up --build -d
```

---

## Local development

**Backend**

```bash
# Start only the database
docker compose up bloom-db -d

# Run the API
cd src/Bloom.Main
dotnet run
```

**Frontend**

```bash
cd frontend/bloom
npm install
npm run dev
```

**Tests**

```bash
dotnet test
```

---

## Migrations

If you need to recreate the migrations from scratch, delete the existing migration files and run:

```bash
dotnet ef migrations add InitialCreate \
  -p src/Bloom.Infrastructure \
  -s src/Bloom.Main \
  --context PostgresDomainDbContext \
  --output-dir Persistence/EntityFramework/Migrations/PostgreSQL
```

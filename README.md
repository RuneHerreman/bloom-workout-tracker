# Bloom

> Self-hosted workout tracker. Plan, log, and analyse your training — strength and cardio.

[![App](https://img.shields.io/badge/App-localhost%3A3000-3E544B?style=flat-square)](http://localhost:3000)
[![API docs](https://img.shields.io/badge/API%20docs-Scalar-3E544B?style=flat-square)](http://localhost:8080/scalar)
[![React](https://img.shields.io/badge/React-19-61DAFB?style=flat-square&logo=react&logoColor=white)](https://react.dev)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square&logo=docker&logoColor=white)](https://www.docker.com)

![Dashboard overview](docs/dashboard.png)

---

## Overview

Bloom is a self-hosted workout tracking application designed for anyone serious about their training. Whether you're focused on strength training or cardio, Bloom helps you plan your workouts, log your performance, and analyze your progress over time. Since it's self-hosted, your data stays on your own infrastructure.

## Features

**Templates** — Build reusable workout routines. Set target reps, weight, and RIR for strength sets, or duration and distance for cardio. Templates load directly into a session so you're not starting from scratch each time.

**Logging** — Record actual reps and weight per set as you go. Every session is timestamped and stored in your own database, giving you a complete history of your training.

**Analytics** — Track volume trends over time, view personal records by exercise, see muscle group breakdowns, and use the built-in 1-rep-max calculator. All derived from your logged data.

**Tools** — Standalone pages for macro calculation and 1RM estimation, useful for planning and reference.

---

## Table of Contents

- [Screenshots](#screenshots)
- [Stack](#stack)
- [Getting Started](#getting-started)
- [Self-hosting](#self-hosting)
- [Local Development](#local-development)
- [Contributing](#contributing)

---

## Screenshots

| | |
|---|---|
| ![Logbook](docs/log-book.png) | ![Log detail](docs/add-log.png) |
| Logbook — session history | Log detail — sets and notes |
| ![Templates](docs/templates.png) | ![Add exercises](docs/add-exercises.png) |
| Template builder | Exercise library |
| ![GPX map](docs/gpx-overlay.png) | |
| GPX route overlay for cardio | |

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

## Getting Started

The fastest way to get Bloom running is with Docker Compose. You'll have the application up and running in just a few minutes.

### Prerequisites

- [Docker](https://www.docker.com/) with Compose

### Quick Start

```bash
git clone https://github.com/RuneHerreman/bloom-workout-tracker.git
cd bloom-workout-tracker
cp .env.example .env
```

Edit `.env` and set a strong random value for `Jwt__Key` (you can generate one with `openssl rand -hex 32`). Then:

```bash
docker compose up --build -d
```

Once running, access:

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API | http://localhost:8080 |
| API docs (Scalar) | http://localhost:8080/scalar |

### Stopping and Updating

```bash
docker compose down
git pull
docker compose up --build -d
```

---

## Self-hosting

### Environment Configuration

Before running Bloom, copy `.env.example` to `.env` and configure these values:

**Database**
- `POSTGRES_DB` — Database name (default: bloom)
- `POSTGRES_USER` — Database user (default: bloom_user)
- `POSTGRES_PASSWORD` — Strong password for the database user

**API**
- `ASPNETCORE_ENVIRONMENT` — Set to `Production` for deployment or `Development` for local testing with API docs
- `ASPNETCORE_URLS` — API binding address (default: http://+:5000)
- `Jwt__Key` — Random 64-character secret for JWT signing. Generate with: `openssl rand -hex 32`
- `Jwt__Issuer` — JWT issuer name (default: bloom.workout)
- `Jwt__Audience` — JWT audience name (default: users)

For a production deployment, ensure `Jwt__Key` is a strong random value and never commit your `.env` file.

---

## Local Development

### Prerequisites

- [Docker](https://www.docker.com/) with Compose
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS)

### Backend Setup

Start the database container:

```bash
docker compose up bloom-db -d
```

Run the API from the `src/Bloom.Main` directory:

```bash
cd src/Bloom.Main
dotnet run --Database:ConnectionString="Host=localhost;Port=5432;Database=bloom;Username=bloom_user;Password=change_me_strong_password"
```

Use the username and password from your `.env` file. This override is needed because the development configuration expects the Docker-internal hostname.

### Frontend Setup

```bash
cd frontend/bloom
cp .env.example .env
npm install
npm run dev
```

The frontend will be available at http://localhost:5173 and communicate with the API at http://localhost:8080/api/.

### Running Tests

```bash
dotnet test
```

---

## Database Migrations

If you need to recreate migrations from scratch, delete existing migration files and run:

```bash
dotnet ef migrations add InitialCreate \
  -p src/Bloom.Infrastructure \
  -s src/Bloom.Main \
  --context PostgresDomainDbContext \
  --output-dir Persistence/EntityFramework/Migrations/PostgreSQL
```

---

## Contributing

Contributions are welcome. Please feel free to open issues or submit pull requests.

To contribute:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -am 'Add feature'`)
4. Push to the branch (`git push origin feature/your-feature`)
5. Open a pull request

---

## License

This project is provided as-is. Check the repository for license details.

# Bloom Workout Tracker
**Simple, private workout logging. Track strength, cardio, volume. Self-hosted**

## Features
- **Workout Templates**: Create reusable routines with target sets/reps/RIR for strength. Duration / distance for cardio
- **Granular Logging**: Record actual reps, weight per set
- **Progress Analytics**: Volume trends, 1RM estimates, PR trends, muscle group insights
- **Privacy Focused**: Your data stays yours. Self hostable.

## Tech Stack
- **Frontend**: React + Vite + TypeScript
- **Backend**: ASP.NET Core Web API
- **Database**: PostgreSQL
- **Auth**: JWT + BCrypt
- **Deployment**: Docker Compose

## Quick Start
### Prerequisites
- [Docker](https://www.docker.com/)

1. Clone repo
2. Copy `.env.example` → `.env`
3. ```bash
    docker compose up --build
    ```
### URLs
- API Docs (Scalar): http://localhost:8080/scalar
- Frontend: http://localhost:3000/
- Database: http://localhost:5432/

### Extra
If you need to reapply the migrations use delete the old psql directory in infrasturucture and run the following command:
```bash
dotnet ef migrations add InitialCreate -p src/Bloom.Infrastructure -s src/Bloom.Main --context PostgresDomainDbContext --output-dir Persistence/EntityFramework/Migrations/PostgreSQL
```
# Steam Alerts 🎮 📈

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat&logo=postgresql)
![React](https://img.shields.io/badge/React-18-61DAFB?style=flat&logo=react)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=flat&logo=docker)
![License](https://img.shields.io/badge/License-MIT-green)

**Steam Alerts** is a smart dashboard for Game Developers. It enables studios to track performance metrics (player counts, pricing, review sentiment) of competitor games on Steam in real-time.

By mimicking Agile software development, this project demonstrates a modern **Microservice-ready Architecture** using .NET 10, Entity Framework Core, and a React frontend.

---

## 🚀 Features

- **Real-Time Tracking:** Ingests live data from the Steam Storefront API.
- **Historical Analytics:** Tracks player counts and price changes over time (Time-Series).
- **Competitor Watchlist:** Allows users to build custom dashboards of games to monitor.
- **Background Automation:** Uses **Hangfire** to schedule automated data scraping jobs.
- **Secure Auth:** JWT-based authentication for user management.

---

## 🛠 Tech Stack

- **Backend:** ASP.NET Core Web API (.NET 10)
- **Database:** PostgreSQL (with Entity Framework Core)
- **Frontend:** React + Vite + Tailwind CSS (Planned)
- **Background Jobs:** Hangfire
- **DevOps:** Docker, GitHub Actions
- **Testing:** xUnit, Moq

---

## 🏗 Architecture

The project follows **Clean Architecture** principles to ensure scalability and testability:

```text
src/
├── SteamAlertsAPI/          # Main Web API Project
│   ├── Controllers/         # API Endpoints (Entry Point)
│   ├── Services/            # Business Logic & External API Calls
│   ├── Data/                # EF Core Context & Migrations
│   ├── Models/              # Database Entities
│   └── DTOs/                # Data Transfer Objects (Contracts)
├── SteamAlerts.Tests/       # Unit & Integration Tests
└── docker-compose.yml       # Container Orchestration
```

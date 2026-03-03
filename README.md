# Drive2Own 🗺️

A GPS tracking application that lets you record and visualise the roads and paths you travel. Built with **C# (ASP.NET Core)** on the backend, **React** for the web, and **.NET MAUI** for iOS & Android.

---

## Features

- **User Registration & Profiles** – Sign up, log in, update your display name and avatar
- **GPS Route Tracking** – Real-time tracking via the browser Geolocation API (web) or device GPS (mobile)
- **Travel Modes** – Car 🚗, Walk 🚶, Cycle 🚴, Run 🏃
- **Location Pins** – Drop custom pins anywhere on the map with titles and colours
- **Dashboard** – View total distance (km/miles), duration, route count, and distance broken down by travel mode
- **Route Sharing** – Generate a shareable link for any route that anyone can view without an account

---

## Architecture

```
drive2own/
├── src/
│   ├── Drive2Own.Api/        # ASP.NET Core 10 REST API (C#)
│   ├── drive2own-web/        # React 18 + TypeScript web app
│   └── Drive2Own.Mobile/     # .NET MAUI app (iOS + Android)
└── Drive2Own.sln
```

---

## Backend – Drive2Own.Api

**Stack:** ASP.NET Core 10 · Entity Framework Core · SQLite · JWT Authentication

### Setup & Run

```bash
cd src/Drive2Own.Api
dotnet run
```

The API starts on `http://localhost:5000`.  
Swagger UI is available at `http://localhost:5000/swagger`.

### Configuration

`appsettings.json` contains the JWT secret and database path. **Change `Jwt:Key` in production.**

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login, returns JWT |
| GET | `/api/users/me` | Get current user profile |
| PUT | `/api/users/me` | Update profile |
| GET | `/api/locationpins` | List user's pins |
| POST | `/api/locationpins` | Create a pin |
| PUT | `/api/locationpins/{id}` | Update a pin |
| DELETE | `/api/locationpins/{id}` | Delete a pin |
| GET | `/api/routes` | List user's routes |
| POST | `/api/routes` | Start a new route |
| GET | `/api/routes/{id}` | Get route details |
| POST | `/api/routes/{id}/complete` | Mark route complete |
| POST | `/api/routes/{id}/points/batch` | Upload GPS points |
| DELETE | `/api/routes/{id}` | Delete route |
| GET | `/api/dashboard` | Dashboard metrics |
| POST | `/api/shares/routes/{routeId}` | Create share link |
| GET | `/api/shares/{token}` | View shared route |
| GET | `/api/shares/my-shares` | List your shares |

---

## Web App – drive2own-web

**Stack:** React 18 · TypeScript · Leaflet.js · Axios · React Router

### Setup & Run

```bash
cd src/drive2own-web
npm install
REACT_APP_API_URL=http://localhost:5000 npm start
```

Opens at `http://localhost:3000`.

### Pages

- `/login` – Sign in
- `/register` – Create account  
- `/dashboard` – Stats overview
- `/map` – Interactive map (add pins, record routes)
- `/routes` – Browse, share, and delete routes
- `/profile` – Edit your profile
- `/shared/:token` – Public route viewer (no login needed)

---

## Mobile App – Drive2Own.Mobile

**Stack:** .NET MAUI 10 · CommunityToolkit.Mvvm · Microsoft.Maui.Maps

> **Note:** MAUI requires **Windows** (for Android builds) or **macOS** (for iOS/Android builds). The project cannot be built on Linux.

### Setup

1. Install Visual Studio 2022 / VS for Mac with the **.NET MAUI** workload
2. Open `Drive2Own.sln`
3. Set `Drive2Own.Mobile` as the startup project
4. Update the API base URL in `Services/ApiService.cs`
5. Run on Android emulator or iOS Simulator

### Platforms

- **Android** – minSdkVersion 24 (Android 7.0+)
- **iOS** – iOS 15.0+

### Permissions

- **Location (Foreground)** – Required for viewing current position
- **Location (Background/Always)** – Required for continuous route recording

---

## Development Setup

### Prerequisites

- .NET 10 SDK
- Node.js 18+
- Visual Studio 2022 / VS Code with C# Dev Kit

### Run everything locally

```bash
# Terminal 1 – API
cd src/Drive2Own.Api && dotnet run

# Terminal 2 – Web
cd src/drive2own-web && REACT_APP_API_URL=http://localhost:5000 npm start
```

---

## Security Notes

- JWT tokens expire after 7 days
- Passwords are hashed with BCrypt
- All data endpoints require authentication except public share links
- **For production:** change `Jwt:Key` in `appsettings.json` to a strong random secret and store it in an environment variable or secrets manager

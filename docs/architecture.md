# System Architecture

This document outlines the architecture of the Massage Therapy Booking System, focusing on its current local development setup.

## System Overview

The Massage Therapy Booking System is a web application designed to enable massage therapists to manage appointments, client information, and their practice. The application consists of an ASP.NET Core backend API and a React frontend client.

## Architecture Principles

- **Separation of Concerns**: Each component has a single responsibility (API vs Frontend, Services vs Repositories).
- **Loose Coupling**: Minimize dependencies between components (e.g., using Dependency Injection in ASP.NET Core).
- **Layered Architecture**: Backend follows distinct layers for presentation (Controllers), business logic (Services), and data access (Repositories).

## System Components (Local Development)

### High-Level Architecture

```
┌───────────────────┐      ┌────────────────────┐      ┌───────────────────┐
│                   │      │                    │      │                   │
│  Frontend Client  │◄────►│  Backend API       │◄────►│  Database         │
│  (React SPA)      │ HTTP │  (ASP.NET Core API)│ EFCore│  (SQL Server      │
│  (localhost:3000) │      │  (localhost:5001)  │      │   LocalDB)        │
│                   │      │                    │      │                   │
└───────────────────┘      └────────────────────┘      └───────────────────┘
```

### Frontend Architecture (`src/massage-booking-client`)

The frontend is a Single Page Application (SPA) built with React.js and follows a standard component-based architecture.

```
┌─────────────────────────────────────────────────────────────┐
│ React Application (localhost:3000)                          │
│                                                             │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐        │
│  │             │   │             │   │             │        │
│  │  Pages      │   │ Components  │   │  Context    │        │
│  │ (Routes)    │   │ (UI Blocks) │   │ (Auth state)│        │
│  └─────────────┘   └─────────────┘   └─────────────┘        │
│         │                 ▲                  ▲              │
│         ▼                 │                  │              │
│  ┌─────────────┐          │                  │              │
│  │             │          │                  │              │
│  │ App.js      │          │                  │              │
│  │ (Routing)   │─────────►│                  │              │
│  └─────────────┘          │                  │              │
│                           │                  │              │
│  ┌─────────────┐          │                  │              │
│  │             │          │                  │              │
│  │ apiClient.js│◄─────────┴──────────────────┘              │
│  │ (Axios)     │                                            │
│  └─────────────┘            (Proxy to API @ localhost:5001) │
└─────────────────────────────────────────────────────────────┘
```

**Key Components:**

- **App.js**: Defines application routes using `react-router-dom`.
- **Pages/Components**: Functional components representing UI elements and views.
- **Context Providers**: `AuthContext.js` manages global authentication state using React Context API.
- **API Client (`apiClient.js`)**: Uses Axios to handle HTTP communication with the backend API. Configured with `withCredentials: true` for cookie handling.
- **Proxy**: The `package.json` proxy setting forwards API requests from `localhost:3000` to `localhost:5001` during development.

### Backend Architecture (`src/MassageBooking.API`)

The backend follows a standard ASP.NET Core layered architecture pattern:

```
┌─────────────────────────────────────────────────────────────┐
│ ASP.NET Core Web API (localhost:5001)                       │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐   ┌──────────────┐     │
│  │              │  │              │   │              │     │
│  │ Controllers  │──► Services     │───► Repositories │     │
│  │ (API Endpoints)│  │ (Business Logic)│   │ (Data Access)│     │
│  └──────────────┘  └──────────────┘   └──────────────┘     │
│         ▲      │          ▲       │          ▲           │     │
│         │      │          │       │          │           │     │
│  ┌──────┴──────┐  ┌───────┴───────┐  ┌───────┴───────┐     │
│  │             │  │               │  │               │     │
│  │ DTOs        │  │ Domain Models │  │ EF Core DbContext│     │
│  │ (API Data)  │  │ (Entities)    │  │ (& Migrations)│     │
│  └─────────────┘  └───────────────┘  └───────────────┘     │
│        │                                    │              │
│        ▼                                    ▼              │
│  ┌───────────────────────────────────────────────────┐     │
│  │ Middleware (Logging, AuthN, AuthZ, CORS, etc.)    │     │
│  └───────────────────────────────────────────────────┘     │
│                                                          │
└──────────────────────────────────────────────────────────┘
                  │
                  │ Uses EF Core Provider
                  ▼
         ┌─────────────────┐
         │                 │
         │  SQL Server DB  │
         │  (LocalDB)      │
         │                 │
         └─────────────────┘
```

**Key Layers & Components:**

- **Controllers**: Handle HTTP requests, validate input (DTOs), and orchestrate responses. Use `[Authorize]` attributes for access control.
- **Services**: Contain core business logic, coordinating repositories and enforcing rules.
- **Repositories**: Abstract data access operations using Entity Framework Core.
- **DTOs (Data Transfer Objects)**: Define the shape of data exchanged via the API.
- **Domain Models**: Represent core business entities (e.g., `Appointment`, `Client`, `Therapist`).
- **ApplicationDbContext**: EF Core DbContext managing database interactions and migrations.
- **ASP.NET Core Identity**: Provides authentication (cookie-based) and role management services.
- **Middleware**: Handles cross-cutting concerns like request logging, authentication, authorization, and CORS.

## Data Management

- **Database**: SQL Server LocalDB is used for local development.
- **Schema Management**: Entity Framework Core Migrations are used to manage database schema changes. Run `dotnet ef database update` to apply migrations.
- **Seeding**: Basic data (roles, default admin user) is seeded on application startup in the Development environment via `Program.cs`.

## Authentication & Authorization

- **Mechanism**: ASP.NET Core Identity using cookie-based authentication.
- **User Management**: `UserManager<ApplicationUser>` handles user creation, password hashing, etc.
- **Sign-In**: `SignInManager<ApplicationUser>` handles the sign-in process and cookie creation.
- **Roles**: `RoleManager<IdentityRole<Guid>>` manages user roles (Admin, Therapist, Client).
- **Authorization**: API endpoints are secured using `[Authorize]` attributes, often with specific roles (`[Authorize(Roles = \"Admin\")]`).
- **Frontend**: The React client uses `AuthContext` to manage the authenticated state based on the session cookie, facilitated by `withCredentials: true` in Axios requests.

## Removed Components (Previously Considered)

- **Cloud Infrastructure**: Azure services (App Service, Static Web Apps, Azure SQL, Key Vault, AAD B2C, etc.) are not currently part of the local development focus.
- **Infrastructure as Code**: Terraform configuration for deploying Azure resources has been removed.
- **CI/CD**: Azure DevOps pipelines are not currently configured.
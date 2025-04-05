# Massage Therapy Booking System

This is a comprehensive booking system for massage therapy services that supports client, therapist, and admin user workflows. It includes a backend API built with ASP.NET Core and a frontend client built with React.

## Project Overview

This system allows massage therapists to manage their practice efficiently with features including:

- Online appointment booking and management
- Therapist availability management
- Client profiles and history tracking
- SOAP notes for treatment documentation
- Service management
- Email notifications (via a configured service)
- Admin dashboard with reporting capabilities
- User management system with role-based access control

## Technology Stack

### Backend API (`src/MassageBooking.API`)
- **Framework**: ASP.NET Core 9 Web API
- **Database**: SQL Server (using LocalDB for development)
- **ORM**: Entity Framework Core 9
- **Database Management**: EF Core Migrations
- **Authentication/Authorization**: ASP.NET Core Identity (Cookie-based)
- **API Documentation**: Swagger/OpenAPI

### Frontend Client (`src/massage-booking-client`)
- **Framework**: React.js (likely Create React App based)
- **State Management**: React Context API (`AuthContext.js`)
- **Routing**: React Router
- **API Client**: Axios
- **UI**: Basic structure, likely needs styling improvements

### Development Environment
- **IDE**: Visual Studio / VS Code
- **SDK**: .NET 9 SDK
- **Database Server**: SQL Server LocalDB (installed with Visual Studio or separately)
- **Package Managers**: NuGet (Backend), npm or yarn (Frontend)

## Getting Started (Local Development)

### Prerequisites
- .NET 9 SDK
- Node.js and npm/yarn
- SQL Server (LocalDB is sufficient for default setup)
- A code editor like Visual Studio 2022+ or VS Code

### Backend Setup (`src/MassageBooking.API`)
1.  **Navigate to API Directory:** `cd src/MassageBooking.API`
2.  **Restore Dependencies:** `dotnet restore`
3.  **Configure Database:** The default connection string in `appsettings.Development.json` points to `(localdb)\mssqllocaldb`. Ensure this server is running. You can manage LocalDB instances via Visual Studio's SQL Server Object Explorer or using `SqlLocalDB.exe` command line tool.
4.  **Apply Migrations:** `dotnet ef database update` (This creates the database `MassageBookingDb` if it doesn't exist and applies all migrations).
5.  **Configure Admin User (First Run):** Edit `appsettings.Development.json` and provide an email and strong password under the `DefaultAdminUser` section. This user will be created and assigned the 'Admin' role on the first run in Development mode.
    ```json
    "DefaultAdminUser": {
      "Email": "admin@example.com",
      "Password": "YourSecurePassword123!"
    }
    ```
6.  **Run the API:** `dotnet run`. The API should now be accessible at `https://localhost:5001` and `http://localhost:5000`.
7.  **API Documentation:** Access the Swagger UI at `https://localhost:5001/swagger`.

### Frontend Setup (`src/massage-booking-client`)
1.  **Navigate to Client Directory:** `cd src/massage-booking-client`
2.  **Install Dependencies:** `npm install` (or `yarn install`)
3.  **Run the Client:** `npm start` (or `yarn start`). This should open the application in your browser, typically at `http://localhost:3000`.
4.  **Proxy:** The `package.json` likely contains a `proxy` setting pointing to the backend API (`https://localhost:5001`). This allows the frontend development server to forward API requests to the backend, avoiding CORS issues during development.

### Login
- Once both backend and frontend are running, navigate to the frontend URL (`http://localhost:3000`).
- Use the login link and the admin credentials you configured in the backend's `appsettings.Development.json`.

## Key Features Implemented (Relevant to Current State)

- **Core Models:** Client, Therapist, Service, Appointment, Availability, SoapNote.
- **Repositories:** EF Core implementations for data access.
- **Services:** Business logic layer for core entities.
- **API Controllers:** Endpoints for managing core entities.
- **Authentication:** ASP.NET Core Identity setup with cookie-based authentication.
- **Authorization:** Role-based access control (`[Authorize(Roles = "...")]`) implemented on controllers/actions (e.g., Admin dashboard, Client/Therapist management).
- **Database Seeding:** Basic seeding for roles and a default admin user (Development only).
- **Frontend Basics:** React client with routing, components for Login, Admin Dashboard, Client List, Therapist List, Appointments.
- **Frontend Auth Context:** Manages authentication state using cookies.

## Development Focus

- Current efforts are focused on **local development and deployment**.
- Cloud deployment (Azure) and Infrastructure as Code (Terraform) have been **removed** for now.

## User Roles

- **Admin:** Full access, manages users, services, views reports.
- **Therapist:** Manages own schedule, appointments, and SOAP notes.
- **Client:** Books and manages own appointments.
(Note: Detailed permissions are enforced via `[Authorize]` attributes in the API).
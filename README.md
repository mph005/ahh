# Massage Therapy Booking System

This is a comprehensive booking system for massage therapy services that supports client, therapist, and admin user workflows.

## Appointment Booking Implementation

The appointment booking functionality has been implemented using ASP.NET Core and Entity Framework Core, following the repository pattern and service layer architecture.

### Key Components

#### Models & DTOs
- `Appointment`: Core entity for appointment data
- `AvailableSlot`: Model for representing available time slots
- Various DTOs to support API operations:
  - `AppointmentDetailsDTO`: Complete appointment details
  - `AppointmentListItemDTO`: Simplified appointment information for lists
  - `AvailableSlotDTO`: Available time slot information
  - `AppointmentBookingDTO`: Request model for booking
  - `AppointmentRescheduleDTO`: Request model for rescheduling
  - `RebookRequestDTO`: Request model for rebooking
  - `BookingResultDTO`: Response model for booking operations

#### Data Access Layer
- `IAppointmentRepository`: Interface defining data access operations
- `AppointmentRepository`: Implementation using Entity Framework Core
- Key methods:
  - Finding available time slots based on therapist availability
  - Checking for scheduling conflicts
  - Getting a therapist's schedule for a specific date range
  - CRUD operations for appointment management

#### Business Logic Layer
- `IAppointmentService`: Interface defining business operations
- `AppointmentService`: Implementation of appointment-related business logic
- Responsibilities:
  - Validation of booking/rescheduling requests
  - Coordination between repositories
  - Transformation between entities and DTOs
  - Error handling and logging

#### API Layer
- `AppointmentsController`: REST API for appointment operations
- Endpoints:
  - GET /api/appointments/{id}: Get appointment details
  - GET /api/appointments/client/{clientId}: Get client appointments
  - GET /api/appointments/therapist/{therapistId}: Get therapist schedule
  - GET /api/appointments/available-slots: Find available slots
  - POST /api/appointments: Book a new appointment
  - PUT /api/appointments/reschedule: Reschedule an appointment
  - PUT /api/appointments/{id}/cancel: Cancel an appointment
  - POST /api/appointments/rebook: Rebook a previous appointment

### Key Features

1. **Available Slot Finding**
   - Considers therapist availability schedules
   - Accounts for service duration
   - Filters out conflicting appointments
   - Supports filtering by specific therapist

2. **Conflict Detection**
   - Prevents double-booking
   - Handles appointment overlap scenarios
   - Supports exclusion of current appointment for reschedule operations

3. **Therapist Schedule Management**
   - Retrieves appointments for a specific date range
   - Includes all relevant appointment details

4. **Client Booking Experience**
   - Select available slots based on service and preferred therapist
   - Book, reschedule, and cancel appointments
   - View appointment history
   - Rebook previous services

## Technology Stack

- ASP.NET Core 6.0
- Entity Framework Core 6.0
- SQL Server
- Swagger for API documentation

## Getting Started

1. Clone the repository
2. Ensure SQL Server is installed
3. Update the connection string in `appsettings.json` if needed
4. Run `dotnet ef database update` to create the database
5. Run `dotnet run` to start the application
6. Access the Swagger UI at `https://localhost:5001/swagger`

## User Stories Implemented

- **As a client, I want to see available appointment times so that I can book a massage**
- **As a client, I want to select my preferred massage therapist so that I can continue with someone I like**
- **As a client, I want to choose from different massage services so that I get the right treatment**
- **As a client, I want to reschedule or cancel my appointment if my plans change**
- **As a client, I want to view my past appointments so I can track my treatment history**
- **As a client, I want to rebook previous services easily so I don't have to enter all the details again**
- **As a therapist, I want to see my upcoming appointments for the day/week**
- **As a therapist, I want to view client information before appointments so I can prepare**

## Project Overview

This system allows massage therapists to manage their practice efficiently with features including:

- Online appointment booking and management
- Weekly appointment calendar view with availability visualization
- Client profiles and history tracking
- SOAP notes and intake forms
- Therapist availability management
- Service management
- Email/SMS notifications

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 7 Web API
- **Database**: Azure SQL Database
- **ORM**: Entity Framework Core
- **Authentication**: Azure Active Directory B2C
- **Cloud Infrastructure**: Microsoft Azure PaaS services

### Frontend
- **Framework**: React.js
- **UI Library**: Material UI
- **State Management**: React Context API
- **Routing**: React Router
- **API Client**: Axios

### DevOps
- **Source Control**: Git
- **CI/CD**: Azure DevOps
- **Hosting**: Azure App Service & Azure Static Web Apps
- **Monitoring**: Azure Application Insights

## Architecture

The application follows a layered architecture pattern:

1. **Presentation Layer**: React SPA
2. **API Layer**: ASP.NET Core Web API
3. **Service Layer**: Business logic and validation
4. **Data Access Layer**: Entity Framework Core repositories
5. **Database Layer**: Azure SQL Database

See [Architecture Document](./docs/architecture.md) for detailed architecture information.

## Getting Started

### Prerequisites
- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Node.js](https://nodejs.org/) (v16+)
- [npm](https://www.npmjs.com/) (v8+)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)

### Local Development Setup

1. **Clone the repository**
   ```
   git clone https://github.com/yourusername/massage-booking-system.git
   cd massage-booking-system
   ```

2. **Set up backend**
   ```
   cd src/MassageBooking.API
   dotnet restore
   ```

3. **Set up frontend**
   ```
   cd src/massage-booking-client
   npm install
   ```

4. **Configure local settings**
   - Create `appsettings.Development.json` in the API project
   - Update connection strings and configuration values

5. **Run database migrations**
   ```
   cd src/MassageBooking.API
   dotnet ef database update
   ```

6. **Start the application**
   - Run the API: `dotnet run`
   - In a separate terminal, run the frontend: `npm start`

### Azure Deployment

See the [Deployment Guide](./docs/deployment.md) for detailed instructions on deploying to Azure.

## Code Organization

The solution follows these design patterns and principles:

- **Clean Architecture**: Separation of concerns with distinct layers
- **Repository Pattern**: Data access abstraction
- **CQRS (Light)**: Separation of read and write operations
- **Dependency Injection**: For loose coupling of components
- **RESTful API Design**: Standard HTTP methods and status codes

Folder structure:
```
/src
  /MassageBooking.API           # Web API project
    /Controllers                # API endpoints
    /Models                     # DTOs and view models
    /Services                   # Business logic
    /Data                       # EF context and migrations
    /Config                     # Configuration classes
    /Extensions                 # Service extension methods
  
  /massage-booking-client       # React frontend
    /src
      /components               # Reusable UI components
      /pages                    # Page components
      /contexts                 # React context providers
      /api                      # API client services
      /hooks                    # Custom React hooks
      /utils                    # Utility functions
```

## Development Guidelines

- Follow the [Technical Specifications](./docs/technical.md) for coding standards
- Check [Current Tasks](./tasks/tasks.md) for development priorities
- Add unit tests for all business logic
- Document public APIs and components

## License

This project is proprietary software. All rights reserved.
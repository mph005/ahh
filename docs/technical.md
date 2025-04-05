# Technical Specifications

This document outlines the technical specifications, patterns, and conventions currently used in the Massage Therapy Booking System, focusing on the local development setup.

## Development Standards

### General Coding Standards

- Use consistent code formatting and naming conventions (see below).
- Follow SOLID principles where applicable.
- Write self-documenting code with meaningful names.
- Add XML documentation comments for public API endpoints and complex service methods in the backend.
- Add JSDoc comments for complex React components or utility functions.
- Keep methods/functions small and focused on a single responsibility.
- Use async/await for I/O-bound operations in the backend and for API calls in the frontend.
- Implement proper error handling and logging (basic logging is set up).
- Write unit tests for critical business logic (test projects might need setup/update).

### Backend Development (.NET 9)

#### Naming Conventions

- **Namespaces**: `MassageBooking.API.[Module].[Submodule]` (e.g., `MassageBooking.API.Services`)
- **Classes**: PascalCase, noun or noun phrase (e.g., `AppointmentService`, `Client`)
- **Interfaces**: PascalCase with 'I' prefix (e.g., `IAppointmentRepository`)
- **Methods**: PascalCase, verb or verb phrase (e.g., `GetAppointmentsInRangeAsync`)
- **Properties**: PascalCase (e.g., `StartTime`, `ClientId`)
- **Local Variables**: camelCase (e.g., `availableSlots`)
- **Constants**: PascalCase (e.g., `DefaultPageSize`)
- **Private Fields**: camelCase with underscore prefix (e.g., `_logger`)

#### Class Structure (Example Service)
```csharp
using System;
using System.Threading.Tasks;
// System imports first
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // If DbContext is directly used (less common in services)
// Microsoft imports second
using MassageBooking.API.Data.Repositories;
using MassageBooking.API.Models;
using MassageBooking.API.DTOs; // For DTO mapping
using AutoMapper; // If using AutoMapper
// Project imports last, ordered by namespace

namespace MassageBooking.API.Services
{
    /// <summary>
    /// Provides functionality for managing appointments.
    /// </summary>
    public class AppointmentService : IAppointmentService
    {
        // Private fields for dependencies
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAvailabilityRepository _availabilityRepository; // Example dependency
        private readonly ILogger<AppointmentService> _logger;
        private readonly IMapper _mapper; // Example if using AutoMapper

        // Constructor injection for dependencies
        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IAvailabilityRepository availabilityRepository,
            ILogger<AppointmentService> logger,
            IMapper mapper)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _availabilityRepository = availabilityRepository ?? throw new ArgumentNullException(nameof(availabilityRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Public interface methods (implementing IAppointmentService)
        public async Task<AppointmentDetailsDTO> GetAppointmentByIdAsync(Guid appointmentId)
        {
            // Implementation using repositories and mapping
            _logger.LogInformation("Fetching appointment with ID: {AppointmentId}", appointmentId);
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                _logger.LogWarning("Appointment not found: {AppointmentId}", appointmentId);
                return null; // Or throw specific exception
            }
            return _mapper.Map<AppointmentDetailsDTO>(appointment);
        }

        // Other public methods...

        // Private helper methods (if needed)
    }
}
```

#### File Organization

- **One primary class per file** (DTOs, simple related classes can be exceptions).
- File name should match the primary class name (e.g., `AppointmentService.cs`).
- Group related files in appropriate folders (`Controllers`, `Services`, `Repositories`, `Models`, `DTOs`, `Data/Migrations`).

#### Error Handling

- Use `try-catch` blocks in service methods for specific, handleable exceptions.
- Rely on global exception handling middleware (if configured) for unhandled exceptions.
- Log errors with sufficient context using `ILogger`.
- Return appropriate HTTP status codes from controllers (e.g., `NotFound()`, `BadRequest()`, `Unauthorized()`, `Forbid()`).

#### Database

- Use **Entity Framework Core 9** with SQL Server.
- Define entity configurations using Data Annotations (`[Key]`, `[Required]`, `[MaxLength]`, `[ForeignKey]`) primarily. Use Fluent API in `ApplicationDbContext.OnModelCreating` for more complex configurations (e.g., composite keys, cascade delete behavior, indexing).
- Use **EF Core Migrations** for schema management. Generate migrations using `dotnet ef migrations add MigrationName` and apply them with `dotnet ef database update`.

#### Authentication & Authorization

- Use **ASP.NET Core Identity** for user management and authentication.
- Use Cookie-based authentication for the frontend interaction.
- Inject `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>` into controllers/services where needed.
- Secure endpoints using `[Authorize]` attributes, specifying roles (`[Authorize(Roles = "Admin")]`) where necessary.

### Frontend Development (React)

#### Naming Conventions

- **Components**: PascalCase (e.g., `AppointmentList.js`)
- **Files**: PascalCase for components, camelCase for utilities/hooks/contexts (e.g., `useAuth.js`, `apiClient.js`, `AuthContext.js`).
- **Functions/Variables**: camelCase (e.g., `fetchAppointments`, `isLoading`).
- **Constants**: UPPER_SNAKE_CASE (e.g., `API_BASE_URL`) - if applicable.
- **CSS Classes**: Use descriptive names, potentially BEM (`block__element--modifier`) or CSS Modules if introduced later.

#### Component Structure (Functional Components with Hooks)
```jsx
import React, { useState, useEffect, useContext } from 'react';
// React imports first
// import { Typography, Button, Grid } from '@mui/material'; // Example UI library imports
// Third-party imports second
import apiClient from '../api/apiClient';
import { AuthContext } from '../context/AuthContext'; // Or useAuth hook
import AppointmentCard from '../components/AppointmentCard'; // Example component import
// Project imports last, ordered by type
import './AppointmentList.css'; // Import CSS

/**
 * Displays a list of appointments.
 * (Add more details about props/functionality if complex)
 */
const AppointmentList = () => {
  // State variables
  const [appointments, setAppointments] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  // Context / Hooks
  const { user } = useContext(AuthContext); // Or const { user } = useAuth();

  // Effects for data fetching
  useEffect(() => {
    // AbortController for cleanup
    const abortController = new AbortController();

    const loadAppointments = async () => {
      // Skip fetch if user is not available (relevant for role-based fetches)
      // if (!user?.id) return;
      try {
        setIsLoading(true);
        setError(null);
        // Example fetch - adjust endpoint and params as needed
        const data = await apiClient.getAppointmentsInRange(startDate, endDate, { signal: abortController.signal });
        setAppointments(data);
      } catch (err) {
        if (err.name !== 'CanceledError') { // Ignore abort errors
          setError('Failed to load appointments.');
          console.error("Fetch appointments error:", err);
        }
      } finally {
        // Check if the component is still mounted before setting state
        if (!abortController.signal.aborted) {
            setIsLoading(false);
        }
      }
    };

    loadAppointments();

    // Cleanup function
    return () => {
      abortController.abort();
    };
  }, []); // Add dependencies if the fetch depends on changing props/state (e.g., [user.id, startDate, endDate])

  // Event handlers
  const handleSomeAction = (event) => {
    // Implementation
  };

  // Render logic
  if (isLoading) {
    return <div>Loading appointments...</div>;
  }

  if (error) {
    return <div className="error-message">Error: {error}</div>;
  }

  return (
    <div className="appointment-list-container">
      <h2>Appointments</h2>
      {appointments.length === 0 ? (
        <p>No appointments found.</p>
      ) : (
        <div className="appointments">
          {appointments.map(appointment => (
            <AppointmentCard
              key={appointment.appointmentId} // Use unique key
              appointment={appointment}
            />
          ))}
        </div>
      )}
      {/* Add buttons or other controls here */}
    </div>
  );
};

export default AppointmentList;
```

#### File Organization (`src/massage-booking-client/src`)

- **`components/`**: Reusable UI components (e.g., `AppointmentCard.js`, `TherapistList.js`).
- **`context/`**: React Context providers (e.g., `AuthContext.js`).
- **`api/`**: API client setup (`apiClient.js`).
- **`hooks/`**: Custom React hooks (if any).
- **`pages/`**: (Optional) If components become complex, separate page-level components here.
- **`App.js`**: Main application component with routing.
- **`index.js`**: Application entry point.

#### State Management

- Use **React Context API** (`AuthContext.js`) for global state like authentication status and user information.
- Use `useState` hook for local component state.
- Consider `useReducer` for more complex local state logic if needed.

#### API Interaction

- Use the configured **Axios instance** in `apiClient.js` for all backend communication.
- Ensure `withCredentials: true` is set in `apiClient.js` for cookie handling.
- Handle loading states and errors gracefully in components that fetch data.
- Use `useEffect` hook for data fetching on component mount or when dependencies change.
- Implement cleanup logic in `useEffect` (e.g., using `AbortController`) to prevent state updates on unmounted components.

## Design Patterns (Backend)

### Repository Pattern

- **Purpose**: Abstracts data access logic from the rest of the application.
- **Implementation**: Interfaces (e.g., `IAppointmentRepository`) define contracts, and concrete classes (e.g., `AppointmentRepository`) implement them using EF Core.
- **Benefits**: Decouples business logic from data access technology, improves testability (can mock repositories).

### Service Pattern

- **Purpose**: Encapsulates business logic related to a specific domain entity or feature.
- **Implementation**: Interfaces (e.g., `IAppointmentService`) define business operations, and concrete classes (e.g., `AppointmentService`) implement them, often coordinating multiple repositories.
- **Benefits**: Centralizes business logic, improves code organization and reusability.

### Dependency Injection (DI)

- **Purpose**: Provides dependencies (like repositories, services, loggers) to classes rather than having them create their own instances.
- **Implementation**: ASP.NET Core's built-in DI container is used extensively. Register services/repositories in `Program.cs` (`builder.Services.AddScoped<IAppointmentService, AppointmentService>();`) and request them via constructor injection.
- **Benefits**: Promotes loose coupling, enhances testability.

## Removed Technologies/Patterns

- **Azure Cloud Services**: Focus is on local development, so Azure-specific services (App Service, Azure SQL, AAD B2C, Key Vault) are not currently used.
- **Terraform**: Infrastructure as Code for Azure is not currently used.
- **JWT Authentication**: While the API has JWT configuration remnants, the primary authentication method for the frontend is Cookie-based Identity.
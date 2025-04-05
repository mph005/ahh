# Development Tasks

This document outlines the current development tasks and status for the Massage Therapy Booking System, focusing on the **local development environment**.

## Completed Tasks (Focus: Identity Integration & Local Setup)

These tasks were recently completed to integrate ASP.NET Core Identity and stabilize the local development workflow:

- [x] **Task ID.1**: Install Identity Packages (`Microsoft.AspNetCore.Identity.EntityFrameworkCore`, etc.).
- [x] **Task ID.2**: Define `ApplicationUser` model inheriting from `IdentityUser<Guid>`.
- [x] **Task ID.3**: Update `ApplicationDbContext` to inherit from `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`.
- [x] **Task ID.4**: Configure Identity services in `Program.cs` (`AddIdentity`, `AddEntityFrameworkStores`).
- [x] **Task ID.5**: Configure Application Cookie behavior in `Program.cs` (Return 401/403 for API requests instead of redirect).
- [x] **Task ID.6**: Resolve package version conflicts (`Microsoft.EntityFrameworkCore` v7 vs v9). Updated `.csproj` to use EF Core 9.0.3.
- [x] **Task ID.7**: Resolve build errors (Missing `AuditLog` DbSet, missing `ApplicationUser` using, ambiguous `TherapistService` reference).
- [x] **Task ID.8**: Resolve EF Core migration warnings (Shadow properties due to incorrect Fluent API configuration for `Availability`/`SoapNote` -> `Therapist` relationships in `DbContext`).
- [x] **Task ID.9**: Create initial EF Core Migration (`AddIdentityTables`) including Identity schema changes.
- [x] **Task ID.10**: Apply migration to database (`dotnet ef database update`), resolving issues with pre-existing tables by dropping and recreating the DB.
- [x] **Task ID.11**: Implement database seeding logic in `Program.cs` for roles (Admin, Therapist, Client) and a default Admin user (using credentials from `appsettings.Development.json`).
- [x] **Task ID.12**: Refactor `AuthController` to use `UserManager` and `SignInManager` for cookie-based login/logout, removing old JWT logic.
- [x] **Task ID.13**: Fix frontend CORS issues by updating the policy in `Program.cs` (`AllowCredentials`).
- [x] **Task ID.14**: Refactor frontend `apiClient.js`: Remove JWT interceptor, add `withCredentials: true`.
- [x] **Task ID.15**: Refactor frontend `AuthContext.js`: Remove JWT/localStorage logic, manage state based on cookie session (using `/api/auth/userinfo`), fix login/logout logic.
- [x] **Task ID.16**: Fix frontend routing/links (`App.js`) for Client and Therapist management pages, ensuring they are accessible to Admin users.

---

## Current Focus & Next Steps

The immediate focus is solidifying the core application functionality within the local development environment using ASP.NET Core Identity.

### High Priority

- [ ] **Task Next.1**: Implement User Registration API Endpoint
  - Add a `Register` method to `AuthController.cs`.
  - Use `UserManager.CreateAsync` to create new users (Clients initially).
  - Assign default role (e.g., "Client") upon registration.
  - Define `RegisterRequestDTO` and `RegisterResponseDTO`.
  - Add frontend registration form and connect it to the API.
  - **Goal**: Allow new clients to self-register.
- [ ] **Task Next.2**: Refine Authorization Rules
  - Review all API controllers (`AppointmentsController`, `ClientsController`, `TherapistsController`, `ServicesController`, `SoapNotesController`).
  - Apply appropriate `[Authorize]` attributes (with specific roles if needed) to actions based on intended user access (Admin, Therapist, Client, or authenticated user).
  - Example: Clients should only get their *own* appointments/profile, Therapists their *own* schedule/SOAP notes, Admins have wider access.
  - Test access control thoroughly for different roles.
  - **Goal**: Ensure API endpoints are properly secured according to user roles.
- [ ] **Task Next.3**: Implement Therapist/Client Data Seeding (Optional but Recommended)
  - Extend the `SeedDatabaseAsync` logic in `Program.cs` (or create a separate `SeedData` class) to create a few sample Therapists and Clients if none exist.
  - Potentially link sample therapists/clients to the default Admin user for testing, or create separate logins for them.
  - **Goal**: Provide sample data for easier testing of features like booking, therapist lists, etc.
- [ ] **Task Next.4**: Review and Fix Nullability Warnings
  - Address the numerous CS86xx nullability warnings reported during the `dotnet build` process.
  - Update DTOs, Models, Repository/Service method signatures, and implementation logic to correctly handle nullable reference types.
  - This improves code robustness and prevents potential runtime `NullReferenceException` errors.
  - **Goal**: Clean up build warnings and improve code quality.

### Medium Priority

- [ ] **Task Next.5**: Enhance Frontend UI/UX
  - Review existing components (`ClientList`, `TherapistList`, `AppointmentList`, forms) for usability and appearance.
  - Apply consistent styling (potentially integrate a UI library like Material UI or Bootstrap more deeply if desired).
  - Improve loading states and error message presentation.
  - **Goal**: Make the frontend more user-friendly and professional.
- [ ] **Task Next.6**: Implement Missing Core Features (if any)
  - Review original requirements/user stories against implemented features.
  - Identify and prioritize any critical missing pieces (e.g., full SOAP Note editing flow, detailed Availability management UI for therapists).
  - **Goal**: Ensure core product requirements are met.

### Low Priority (Local Focus)

- [ ] **Task Next.7**: Implement Logging/Monitoring (Local)
  - Configure Serilog or use built-in `ILogger` more extensively for structured logging to the console/debug output.
  - Add more detailed logging in key service methods and error handlers.
  - **Goal**: Improve visibility into application behavior during local development/debugging.
- [ ] **Task Next.8**: Unit/Integration Testing Setup
  - Create or update test projects (`.Tests`).
  - Set up mocking frameworks (e.g., Moq).
  - Write basic unit tests for key service logic.
  - Consider setting up an in-memory database provider for integration tests.
  - **Goal**: Establish a foundation for automated testing.

---

## Removed/Deferred Tasks (Cloud/Infrastructure Focus)

- Setting up Azure resources (SQL DB, App Service, Static Web App, AAD B2C, Key Vault, etc.).
- Implementing Infrastructure as Code using Terraform.
- Configuring CI/CD pipelines (Azure DevOps).
- Advanced cloud monitoring/alerting (Application Insights).

(These can be revisited if the project focus shifts back to cloud deployment in the future.)
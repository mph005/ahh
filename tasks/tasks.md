# Development Tasks

This document outlines the current development tasks for the Massage Therapy Booking System, organized by priority and phase.

## Phase 1: Core Functionality

The first phase focuses on establishing the essential booking and client management functionality.

### High Priority Tasks

#### Database Setup and Configuration

- [ ] **Task 1.1**: Create Azure SQL Database
  - Create resource in Azure Portal
  - Configure firewall rules
  - Set up connection string in Key Vault
  - Estimated time: 2 hours

- [ ] **Task 1.2**: Design and implement database schema
  - Create Entity Framework Core models
  - Configure relationships and constraints
  - Set up DbContext and connection
  - Create initial migration
  - Estimated time: 8 hours

- [ ] **Task 1.3**: Seed database with initial data
  - Add service catalog
  - Create admin account
  - Configure test therapist accounts
  - Estimated time: 2 hours

#### Authentication and User Management

- [ ] **Task 1.4**: Configure Azure AD B2C
  - Set up tenant
  - Configure sign-up/sign-in policies
  - Create custom user attributes
  - Design branded login pages
  - Estimated time: 8 hours

- [ ] **Task 1.5**: Implement authentication in API
  - Add authentication middleware
  - Configure JWT bearer token validation
  - Set up role-based authorization
  - Implement user registration flow
  - Estimated time: 6 hours

#### API Development

- [ ] **Task 1.6**: Create User/Client API endpoints
  - Implement user registration
  - Develop client profile management
  - Create controller with CRUD operations
  - Add validation rules
  - Estimated time: 6 hours

- [ ] **Task 1.7**: Create Service API endpoints
  - Implement service catalog endpoints
  - Add CRUD operations for services (admin only)
  - Create filtering and pagination
  - Estimated time: 4 hours

- [ ] **Task 1.8**: Create Therapist API endpoints
  - Implement therapist profile management
  - Add availability setting endpoints
  - Create CRUD operations
  - Estimated time: 6 hours

- [ ] **Task 1.9**: Create Appointment API endpoints
  - Implement appointment creation logic
  - Add validation and business rules
  - Create appointment management endpoints
  - Implement availability checking
  - Estimated time: 10 hours

- [ ] **Task 1.10**: Implement error handling middleware
  - Create consistent error response format
  - Add logging for exceptions
  - Implement appropriate HTTP status codes
  - Estimated time: 4 hours

#### Frontend Development (Core)

- [ ] **Task 1.11**: Set up React application structure
  - Initialize React project
  - Configure routing with React Router
  - Set up Material UI
  - Create layout components
  - Estimated time: 4 hours

- [ ] **Task 1.12**: Implement authentication UI
  - Create login/signup forms
  - Integrate with Azure AD B2C
  - Handle token storage and refresh
  - Add authenticated routes protection
  - Estimated time: 8 hours

- [ ] **Task 1.13**: Create Service Browsing UI
  - Implement service listing component
  - Add service details view
  - Create service selection for booking
  - Estimated time: 6 hours

- [ ] **Task 1.14**: Develop Appointment Booking Flow
  - Create multi-step booking wizard
  - Implement therapist selection
  - Add date/time selection calendar
  - Create booking confirmation
  - Estimated time: 12 hours

- [ ] **Task 1.15**: Build Client Profile Management
  - Create profile editing form
  - Implement form validation
  - Add profile view component
  - Estimated time: 4 hours

### Medium Priority Tasks

- [ ] **Task 1.16**: Implement Appointment Management UI
  - Create appointment list view
  - Add appointment details modal
  - Implement rescheduling and cancellation
  - Estimated time: 8 hours

- [x] **Task 1.17**: Add Basic Reporting
  - Create appointment calendar view ✅
  - Implement daily schedule view
  - Add basic reporting for therapists
  - Estimated time: 6 hours

- [ ] **Task 1.18**: Develop Admin Dashboard
  - Create admin dashboard layout
  - Implement service management UI
  - Add user management interface
  - Create appointment overview
  - Estimated time: 10 hours

### Low Priority Tasks

- [ ] **Task 1.19**: Add Email Notifications
  - Implement appointment confirmation emails
  - Add reminder notification logic
  - Create email templates
  - Estimated time: 6 hours

- [ ] **Task 1.20**: Implement Logging and Monitoring
  - Set up Application Insights
  - Configure structured logging
  - Add performance monitoring
  - Create dashboard in Azure portal
  - Estimated time: 4 hours

- [ ] **Task 1.21**: Add Basic SOAP Notes
  - Create SOAP note form component
  - Implement SOAP note storage
  - Add view/edit capabilities
  - Estimated time: 8 hours

## Phase 2 Preview Tasks (Future Development)

These tasks are planned for Phase 2 and are not part of the current development cycle.

- Implement intake forms for clients
- Add payment processing integration
- Create membership and package management
- Develop advanced reporting and analytics
- Build mobile applications
- Implement marketing features (email campaigns, promotions)
- Add inventory management for products

## Technical Debt Tasks

- [ ] **TD-1**: Write unit tests for core business logic
  - Create test project
  - Add test cases for appointment booking
  - Test availability calculation logic
  - Estimated time: 8 hours

- [ ] **TD-2**: Improve API documentation
  - Add XML comments to controllers
  - Configure Swagger UI
  - Create example requests/responses
  - Estimated time: 4 hours

- [ ] **TD-3**: Optimize database queries
  - Add appropriate indexes
  - Review and optimize LINQ queries
  - Implement caching for frequent queries
  - Estimated time: 6 hours

## Development Process

### Getting Started

1. Clone the repository
2. Set up development environment (see README.md)
3. Run database migrations
4. Start with high-priority tasks

### Task Workflow

1. Create a feature branch for each task
2. Implement the required functionality
3. Write tests for the implementation
4. Submit a pull request for review
5. Address feedback and merge

### Definition of Done

A task is considered complete when:

- All requirements are implemented
- Code follows the project's coding standards
- Tests are written and passing
- Documentation is updated
- Code is reviewed and approved
- Changes are merged to the main branch

## Task Dependencies

```
Task 1.1 → Task 1.2 → Task 1.3
Task 1.4 → Task 1.5 → Task 1.12
Task 1.2 → Task 1.6, 1.7, 1.8, 1.9
Task 1.11 → Task 1.12, 1.13, 1.14, 1.15
Task 1.9 → Task 1.14, 1.16
Task 1.6, 1.8 → Task 1.17
Task 1.7, 1.6, 1.8 → Task 1.18
```

## Estimated Timeline

- Database and Authentication (Tasks 1.1-1.5): Week 1-2
- Core API Development (Tasks 1.6-1.10): Week 3-4
- Frontend Foundation (Tasks 1.11-1.15): Week 5-6
- Additional Features (Tasks 1.16-1.21): Week 7-8

## Resources

- [Technical Specifications](../docs/technical.md)
- [Architecture Document](../docs/architecture.md)
- [Azure Documentation](https://docs.microsoft.com/en-us/azure/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [React Documentation](https://reactjs.org/docs/getting-started.html)
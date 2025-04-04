# System Architecture

This document outlines the architecture of the Massage Therapy Booking System.

## System Overview

The Massage Therapy Booking System is a cloud-based application that enables massage therapists to manage appointments, client information, and their practice. The application is built using modern web technologies and follows a layered architecture pattern.

## Architecture Principles

- **Separation of Concerns**: Each component has a single responsibility
- **Loose Coupling**: Minimize dependencies between components
- **High Cohesion**: Related functionality grouped together
- **Security by Design**: Security considerations built into every layer
- **Cloud-Native**: Designed to leverage Azure PaaS services

## System Components

### High-Level Architecture

```
┌───────────────────┐     ┌───────────────────┐     ┌───────────────────┐
│                   │     │                   │     │                   │
│  Client           │     │  API Layer        │     │  Data Layer       │
│  (React SPA)      │◄───►│  (ASP.NET Core)   │◄───►│  (Azure SQL)      │
│                   │     │                   │     │                   │
└───────────────────┘     └───────────────────┘     └───────────────────┘
                                   ▲
                                   │
                                   ▼
                          ┌───────────────────┐
                          │                   │
                          │  Azure Services   │
                          │                   │
                          └───────────────────┘
```

### Frontend Architecture

The frontend is a Single Page Application (SPA) built with React.js and follows a component-based architecture.

```
┌─────────────────────────────────────────────────────────────┐
│ React Application                                            │
│                                                             │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐        │
│  │             │   │             │   │             │        │
│  │  Pages      │   │ Components  │   │  Context    │        │
│  │             │   │             │   │  Providers  │        │
│  └─────────────┘   └─────────────┘   └─────────────┘        │
│         │                 ▲                  ▲              │
│         ▼                 │                  │              │
│  ┌─────────────┐          │                  │              │
│  │             │          │                  │              │
│  │  Layouts    │          │                  │              │
│  │             │─────────►│                  │              │
│  └─────────────┘          │                  │              │
│                           │                  │              │
│  ┌─────────────┐          │                  │              │
│  │             │          │                  │              │
│  │  API Client │◄─────────┴──────────────────┘              │
│  │  Services   │                                            │
│  └─────────────┘                                            │
└─────────────────────────────────────────────────────────────┘
```

**Key Components:**

- **Pages**: Container components representing entire screens (e.g., Dashboard, BookingPage)
- **Layouts**: Structural components defining the UI organization
- **Components**: Reusable UI elements (e.g., AppointmentCalendar, ClientForm)
- **Context Providers**: Global state management (e.g., AuthContext, NotificationContext)
- **API Client Services**: Handles communication with backend services

### Backend Architecture

The backend follows a layered architecture pattern:

```
┌─────────────────────────────────────────────────────────────┐
│ ASP.NET Core Web API                                         │
│                                                             │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐        │
│  │             │   │             │   │             │        │
│  │ Controllers │───► Services    │───► Repositories │        │
│  │             │   │             │   │             │        │
│  └─────────────┘   └─────────────┘   └─────────────┘        │
│         │                 │                  │              │
│         ▼                 ▼                  ▼              │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐        │
│  │             │   │             │   │             │        │
│  │ DTOs/Models │   │ Domain      │   │ Entity      │        │
│  │             │   │ Models      │   │ Framework   │        │
│  └─────────────┘   └─────────────┘   └─────────────┘        │
│                                             │               │
└─────────────────────────────────────────────┼───────────────┘
                                              ▼
                                     ┌─────────────────┐
                                     │                 │
                                     │  Azure SQL DB   │
                                     │                 │
                                     └─────────────────┘
```

**Key Layers:**

- **Controllers**: Handle HTTP requests and responses
- **Services**: Implement business logic and validation
- **Repositories**: Data access abstraction
- **DTOs/Models**: Data transfer objects for API communication
- **Domain Models**: Core business entities
- **Entity Framework**: ORM for database operations

## Azure Services Architecture

The system leverages several Azure PaaS services:

```
┌─────────────────────────────────────────────────────────────┐
│ Azure Resources                                              │
│                                                             │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐        │
│  │             │   │             │   │             │        │
│  │ App Service │   │ Static Web  │   │ SQL Database│        │
│  │             │   │ Apps        │   │             │        │
│  └─────────────┘   └─────────────┘   └─────────────┘        │
│                                                             │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐        │
│  │             │   │             │   │             │        │
│  │ Active      │   │ Key Vault   │   │ App Insights│        │
│  │ Directory B2C│   │             │   │             │        │
│  └─────────────┘   └─────────────┘   └─────────────┘        │
│                                                             │
│  ┌─────────────┐   ┌─────────────┐                          │
│  │             │   │             │                          │
│  │ Logic Apps  │   │ Notification│                          │
│  │             │   │ Hub         │                          │
│  └─────────────┘   └─────────────┘                          │
└─────────────────────────────────────────────────────────────┘
```

**Azure Services Used:**

- **Azure App Service**: Hosts the ASP.NET Core Web API
- **Azure Static Web Apps**: Hosts the React SPA
- **Azure SQL Database**: Stores application data
- **Azure Active Directory B2C**: Handles authentication and user management
- **Azure Key Vault**: Secures connection strings and secrets
- **Application Insights**: Monitors application performance
- **Logic Apps**: Handles workflow automation (e.g., email notifications)
- **Notification Hub**: Manages push notifications

## Communication Patterns

### API Communication

The frontend communicates with the backend through RESTful APIs:

- HTTP methods (GET, POST, PUT, DELETE) for CRUD operations
- JSON for data exchange
- Bearer token authentication
- API versioning for backward compatibility

### Database Access

- Entity Framework Core for database operations
- Repository pattern for data access abstraction
- Async/await pattern for non-blocking operations

## Security Architecture

Security is implemented at multiple levels:

1. **Authentication**: Azure AD B2C for identity management
2. **Authorization**: Role-based access control in the API
3. **Communication**: HTTPS/TLS for all communications
4. **Data Protection**: Encryption at rest for database
5. **Secret Management**: Azure Key Vault for secure storage of credentials

## Scalability Considerations

The architecture supports horizontal and vertical scaling:

- Stateless API design allows for multiple instances
- Connection pooling for database connections
- Azure App Service auto-scaling
- Azure SQL elastic pools (future consideration)

## Data Flow Diagrams

### Appointment Booking Flow

```
┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐
│         │     │         │     │         │     │         │
│ Client  │────►│ Service │────►│Therapist│────►│Date/Time│
│ Portal  │     │Selection│     │Selection│     │Selection│
│         │     │         │     │         │     │         │
└─────────┘     └─────────┘     └─────────┘     └─────────┘
                                                     │
                                                     ▼
┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐
│         │     │         │     │         │     │         │
│ Email   │◄────│Database │◄────│ Payment │◄────│Confirm  │
│ Notif.  │     │ Update  │     │(Future) │     │Booking  │
│         │     │         │     │         │     │         │
└─────────┘     └─────────┘     └─────────┘     └─────────┘
```

### Authentication Flow

```
┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐
│         │     │         │     │         │     │         │
│ Client  │────►│ Azure   │────►│ Token   │────►│ API     │
│ App     │     │ AD B2C  │     │ Issued  │     │ Access  │
│         │     │         │     │         │     │         │
└─────────┘     └─────────┘     └─────────┘     └─────────┘
```

## Deployment Architecture

The application uses Azure DevOps for CI/CD with separate environments:

- **Development**: For active development and testing
- **Staging**: For pre-production validation
- **Production**: For live application

Each environment has its own set of Azure resources.

## Future Architectural Considerations

- **Microservices**: Potential evolution to microservices for specific domains
- **Event-Driven Architecture**: For better decoupling of components
- **Azure Functions**: For specific background processing needs
- **Azure Cosmos DB**: For handling document-based data (e.g., SOAP notes)
- **Azure API Management**: For API governance at scale
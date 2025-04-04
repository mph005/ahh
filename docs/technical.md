# Technical Specifications

This document outlines the technical specifications, patterns, and conventions used in the Massage Therapy Booking System.

## Development Standards

### General Coding Standards

- Use consistent code formatting and naming conventions
- Follow SOLID principles
- Write self-documenting code with meaningful names
- Add XML documentation comments for public APIs
- Keep methods small and focused on a single responsibility
- Use async/await for I/O-bound operations
- Implement proper error handling and logging
- Write unit tests for business logic

### Backend Development (.NET)

#### Naming Conventions

- **Namespaces**: `MassageBooking.[Module].[Submodule]`
- **Classes**: PascalCase, noun or noun phrase
- **Interfaces**: PascalCase with 'I' prefix
- **Methods**: PascalCase, verb or verb phrase
- **Properties**: PascalCase
- **Variables**: camelCase
- **Constants**: PascalCase
- **Private Fields**: camelCase with underscore prefix

#### Class Structure
```csharp
using System;
// System imports first
using Microsoft.Extensions.Logging;
// Microsoft imports second
using MassageBooking.Core.Models;
// Project imports last, ordered by namespace

namespace MassageBooking.API.Services
{
    /// <summary>
    /// Provides functionality for managing appointments
    /// </summary>
    public class AppointmentService : IAppointmentService
    {
        // Private fields
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger<AppointmentService> _logger;
        
        // Constructor
        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            ILogger<AppointmentService> logger)
        {
            _appointmentRepository = appointmentRepository ?? 
                throw new ArgumentNullException(nameof(appointmentRepository));
            _logger = logger ?? 
                throw new ArgumentNullException(nameof(logger));
        }
        
        // Public properties
        
        // Public methods
        
        // Private helper methods
    }
}
```

#### File Organization

- One class per file (except for small related classes)
- File name should match class name
- Group related files in appropriate folders
- Use feature folders where appropriate

### Frontend Development (React)

#### Naming Conventions

- **Components**: PascalCase
- **Files**: PascalCase for components, camelCase for utilities
- **Functions**: camelCase
- **Constants**: UPPER_SNAKE_CASE
- **CSS Classes**: kebab-case

#### Component Structure
```jsx
import React, { useState, useEffect } from 'react';
// React imports first
import { Typography, Button, Grid } from '@mui/material';
// Third-party imports second
import { AppointmentCard } from '../components/AppointmentCard';
import { useAuth } from '../contexts/AuthContext';
import { fetchAppointments } from '../api/appointmentApi';
// Project imports last, ordered by type

// Constants
const ITEMS_PER_PAGE = 10;

/**
 * Displays a list of appointments for the current user
 */
const AppointmentList = () => {
  // State variables
  const [appointments, setAppointments] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Hooks
  const { user } = useAuth();
  
  // Effects
  useEffect(() => {
    const loadAppointments = async () => {
      try {
        setIsLoading(true);
        const data = await fetchAppointments(user.id);
        setAppointments(data);
        setError(null);
      } catch (err) {
        setError('Failed to load appointments');
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };
    
    loadAppointments();
  }, [user.id]);
  
  // Event handlers
  const handleRefresh = () => {
    // Implementation
  };
  
  // Helper functions
  const filterUpcomingAppointments = () => {
    // Implementation
  };
  
  // Render methods
  const renderAppointmentList = () => {
    return appointments.map(appointment => (
      <AppointmentCard 
        key={appointment.id} 
        appointment={appointment} 
      />
    ));
  };
  
  // Main render
  return (
    <div className="appointment-list">
      <Typography variant="h4">Your Appointments</Typography>
      {isLoading ? (
        <div>Loading...</div>
      ) : error ? (
        <div>{error}</div>
      ) : (
        <Grid container spacing={2}>
          {renderAppointmentList()}
        </Grid>
      )}
    </div>
  );
};

export default AppointmentList;
```

#### File Organization

- Group components by feature or type
- Keep related files close together
- Use index.js files for cleaner imports

## Design Patterns

### Repository Pattern

Used for data access abstraction:

```csharp
public interface IAppointmentRepository
{
    Task<Appointment> GetByIdAsync(Guid id);
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<IEnumerable<Appointment>> GetByClientIdAsync(Guid clientId);
    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(Guid id);
}

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;
    
    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Appointment> GetByIdAsync(Guid id)
    {
        return await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Therapist)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);
    }
    
    // Other method implementations
}
```

### Service Pattern

Used for business logic:

```csharp
public interface IAppointmentService
{
    Task<AppointmentDto> GetAppointmentByIdAsync(Guid id);
    Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync();
    Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto appointmentDto);
    Task UpdateAppointmentAsync(UpdateAppointmentDto appointmentDto);
    Task CancelAppointmentAsync(Guid id);
    Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(
        Guid serviceId, Guid? therapistId, DateTime startDate, DateTime endDate);
}

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly ITherapistRepository _therapistRepository;
    private readonly ILogger<AppointmentService> _logger;
    
    // Constructor and method implementations
}
```

### React Custom Hooks

Used for encapsulating reusable logic:

```javascript
import { useState, useEffect } from 'react';
import { fetchTherapists } from '../api/therapistApi';

export const useTherapists = (serviceId = null) => {
  const [therapists, setTherapists] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  
  useEffect(() => {
    const loadTherapists = async () => {
      try {
        setIsLoading(true);
        const data = await fetchTherapists(serviceId);
        setTherapists(data);
        setError(null);
      } catch (err) {
        setError('Failed to load therapists');
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };
    
    loadTherapists();
  }, [serviceId]);
  
  return { therapists, isLoading, error };
};
```

## Database Schema

### Entity Framework Core Conventions

- Use Fluent API for complex configurations
- Apply data annotations for simple constraints
- Configure relationships explicitly
- Use value converters for non-standard types
- Configure indexes for performance

Example configuration:

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Therapist> Therapists { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Availability> Availabilities { get; set; }
    public DbSet<IntakeForm> IntakeForms { get; set; }
    public DbSet<SOAPNote> SOAPNotes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>()
            .HasKey(u => u.UserId);
            
        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
            
        // Appointment configuration
        modelBuilder.Entity<Appointment>()
            .HasKey(a => a.AppointmentId);
            
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Client)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Therapist)
            .WithMany(t => t.Appointments)
            .HasForeignKey(a => a.TherapistId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Additional configurations
    }
}
```

## API Design

### RESTful API Conventions

- Use nouns for resource names (e.g., `/appointments`)
- Use HTTP methods appropriately:
  - GET: Retrieve resources
  - POST: Create resources
  - PUT: Update resources
  - DELETE: Remove resources
- Return appropriate HTTP status codes
- Use consistent response formats
- Implement pagination for collection endpoints
- Support filtering and sorting where appropriate

### API Response Format

```json
{
  "data": {
    "appointmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "clientName": "John Doe",
    "therapistId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "therapistName": "Jane Smith",
    "serviceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "serviceName": "Deep Tissue Massage",
    "startTime": "2023-05-01T14:00:00Z",
    "endTime": "2023-05-01T15:00:00Z",
    "status": "Scheduled",
    "notes": "Client requested focus on lower back"
  },
  "metadata": {
    "timestamp": "2023-04-28T12:34:56Z"
  }
}
```

### Error Response Format

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": [
      {
        "field": "startTime",
        "message": "Start time must be in the future"
      },
      {
        "field": "serviceId",
        "message": "Service not found"
      }
    ]
  },
  "metadata": {
    "timestamp": "2023-04-28T12:34:56Z"
  }
}
```

## Authentication & Authorization

### Azure AD B2C Integration

- Custom user flows for sign-up and sign-in
- Role-based access control
- JWT token validation
- Secure token storage in frontend

### Authorization Attributes

```csharp
[Authorize(Roles = "Admin")]
[HttpGet("reports/revenue")]
public async Task<ActionResult<RevenueReportDto>> GetRevenueReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
{
    // Implementation
}

[Authorize(Roles = "Admin,Therapist")]
[HttpGet("clients/{id}")]
public async Task<ActionResult<ClientDetailsDto>> GetClientDetails(Guid id)
{
    // Implementation with additional authorization checks
    var requestingUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Additional authorization logic
}
```

## Error Handling

### Exception Handling Strategy

- Use custom exception classes for specific error scenarios
- Handle exceptions at appropriate levels
- Log exceptions with contextual information
- Return user-friendly error messages

Example custom exception:

```csharp
public class ResourceNotFoundException : Exception
{
    public string ResourceType { get; }
    public string ResourceId { get; }
    
    public ResourceNotFoundException(string resourceType, string resourceId)
        : base($"{resourceType} with ID {resourceId} was not found")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
```

Example exception middleware:

```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    
    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");
        
        var statusCode = GetStatusCode(exception);
        var response = CreateErrorResponse(exception, statusCode);
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
    
    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ResourceNotFoundException => StatusCodes.Status404NotFound,
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
    }
    
    private static object CreateErrorResponse(Exception exception, int statusCode)
    {
        return new
        {
            error = new
            {
                code = GetErrorCode(exception),
                message = exception.Message,
                details = GetErrorDetails(exception)
            },
            metadata = new
            {
                timestamp = DateTime.UtcNow
            }
        };
    }
    
    private static string GetErrorCode(Exception exception)
    {
        return exception switch
        {
            ResourceNotFoundException => "RESOURCE_NOT_FOUND",
            ValidationException => "VALIDATION_ERROR",
            UnauthorizedAccessException => "FORBIDDEN",
            _ => "INTERNAL_SERVER_ERROR"
        };
    }
    
    private static object GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => validationEx.Errors,
            _ => null
        };
    }
}
```

## Logging Strategy

- Use structured logging with Serilog
- Log contextual information (correlation IDs, user IDs)
- Use appropriate log levels
- Configure different sinks for different environments
- Mask sensitive information

Example logging configuration:

```csharp
public static class LoggingExtensions
{
    public static IHostBuilder ConfigureLogging(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, config) =>
        {
            config
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.ApplicationInsights(
                    context.Configuration["ApplicationInsights:InstrumentationKey"],
                    TelemetryConverter.Traces);
        });
    }
}
```

## Performance Considerations

- Use asynchronous programming for I/O-bound operations
- Implement caching for frequently accessed data
- Use pagination for large result sets
- Configure database indexes for common queries
- Use compression for API responses
- Optimize frontend bundle size

Example caching implementation:

```csharp
public class CachedServiceRepository : IServiceRepository
{
    private readonly IServiceRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedServiceRepository> _logger;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);
    
    public CachedServiceRepository(
        IServiceRepository repository,
        IMemoryCache cache,
        ILogger<CachedServiceRepository> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<Service> GetByIdAsync(Guid id)
    {
        var cacheKey = $"Service_{id}";
        
        if (_cache.TryGetValue(cacheKey, out Service cachedService))
        {
            _logger.LogInformation("Cache hit for service {ServiceId}", id);
            return cachedService;
        }
        
        _logger.LogInformation("Cache miss for service {ServiceId}", id);
        var service = await _repository.GetByIdAsync(id);
        
        if (service != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_cacheDuration);
                
            _cache.Set(cacheKey, service, cacheOptions);
        }
        
        return service;
    }
    
    // Implement other repository methods with caching
}
```

## Testing Strategy

#### Unit Testing

- Use xUnit for .NET tests
- Use Jest for React tests
- Focus on testing business logic
- Use mocking for external dependencies
- Aim for high test coverage of core functionality
- Follow Arrange-Act-Assert pattern
- Name tests using [MethodUnderTest]_[Scenario]_[ExpectedResult] pattern

### Integration Testing

- Test API endpoints end-to-end
- Use in-memory database for testing
- Test authentication and authorization
- Verify API contracts

Example integration test:

```csharp
public class AppointmentControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public AppointmentControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real database with in-memory database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });
                
                // Seed test data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                
                db.Database.EnsureCreated();
                
                // Add test data
                SeedTestData(db);
            });
        });
        
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task GetAppointments_ReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        // Add authentication token
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", GenerateTestToken("Admin"));
            
        // Act
        var response = await _client.GetAsync("/api/appointments");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }
    
    [Fact]
    public async Task CreateAppointment_WithValidData_ReturnsCreatedResponse()
    {
        // Arrange
        var appointmentDto = new CreateAppointmentDto
        {
            ClientId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            TherapistId = Guid.Parse("4fa85f64-5717-4562-b3fc-2c963f66afa6"),
            ServiceId = Guid.Parse("5fa85f64-5717-4562-b3fc-2c963f66afa6"),
            StartTime = DateTime.UtcNow.AddDays(1)
        };
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", GenerateTestToken("Client"));
            
        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", appointmentDto);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<AppointmentDto>();
        Assert.NotNull(content);
        Assert.Equal(appointmentDto.ClientId, content.ClientId);
    }
    
    private void SeedTestData(ApplicationDbContext context)
    {
        // Add test data here
    }
    
    private string GenerateTestToken(string role)
    {
        // Generate test JWT token
        return "test-token";
    }
}
```

### UI Testing

- Use React Testing Library for component tests
- Test user interactions
- Verify component rendering
- Mock API calls

Example UI test:

```javascript
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import AppointmentBooking from './AppointmentBooking';
import { AppointmentContext } from '../contexts/AppointmentContext';
import { AuthContext } from '../contexts/AuthContext';
import { fetchServices, fetchTherapists } from '../api/appointmentApi';

// Mock API calls
jest.mock('../api/appointmentApi');

describe('AppointmentBooking', () => {
  const mockServices = [
    { id: '1', name: 'Deep Tissue Massage', duration: 60, price: 80 },
    { id: '2', name: 'Swedish Massage', duration: 90, price: 100 }
  ];
  
  const mockTherapists = [
    { id: '1', name: 'John Doe', specialties: 'Deep Tissue, Sports' },
    { id: '2', name: 'Jane Smith', specialties: 'Swedish, Relaxation' }
  ];
  
  const mockUser = {
    id: 'user-123',
    name: 'Test User',
    email: 'test@example.com',
    role: 'Client'
  };
  
  beforeEach(() => {
    fetchServices.mockResolvedValue(mockServices);
    fetchTherapists.mockResolvedValue(mockTherapists);
  });
  
  it('renders the booking steps correctly', async () => {
    render(
      <AuthContext.Provider value={{ user: mockUser, isAuthenticated: true }}>
        <AppointmentContext.Provider value={{ appointments: [] }}>
          <AppointmentBooking />
        </AppointmentContext.Provider>
      </AuthContext.Provider>
    );
    
    // Check if initial step is rendered
    expect(screen.getByText('Book Your Massage Appointment')).toBeInTheDocument();
    expect(screen.getByText('Select Service')).toBeInTheDocument();
    
    // Wait for services to load
    await waitFor(() => {
      expect(screen.getByText('Deep Tissue Massage')).toBeInTheDocument();
      expect(screen.getByText('Swedish Massage')).toBeInTheDocument();
    });
    
    // Select a service
    fireEvent.click(screen.getByText('Deep Tissue Massage'));
    fireEvent.click(screen.getByText('Next'));
    
    // Check if therapist selection step is rendered
    await waitFor(() => {
      expect(screen.getByText('Select Therapist')).toBeInTheDocument();
      expect(screen.getByText('John Doe')).toBeInTheDocument();
      expect(screen.getByText('Jane Smith')).toBeInTheDocument();
    });
  });
  
  it('validates form inputs before proceeding', async () => {
    render(
      <AuthContext.Provider value={{ user: mockUser, isAuthenticated: true }}>
        <AppointmentContext.Provider value={{ appointments: [] }}>
          <AppointmentBooking />
        </AppointmentContext.Provider>
      </AuthContext.Provider>
    );
    
    // Wait for services to load
    await waitFor(() => {
      expect(screen.getByText('Deep Tissue Massage')).toBeInTheDocument();
    });
    
    // Try to proceed without selecting a service
    fireEvent.click(screen.getByText('Next'));
    
    // Button should be disabled
    expect(screen.getByText('Next')).toBeDisabled();
  });
});
```

### Performance Testing

- Use JMeter or k6 for load testing
- Identify performance bottlenecks
- Establish performance baselines
- Test with realistic user loads

Example k6 load test script:

```javascript
import http from 'k6/http';
import { sleep, check } from 'k6';

export const options = {
  vus: 10,
  duration: '30s',
};

export default function () {
  const baseUrl = 'https://api.massage-booking.example.com';
  
  // Get available services
  const servicesResponse = http.get(`${baseUrl}/api/services`);
  check(servicesResponse, {
    'services status is 200': (r) => r.status === 200,
    'services response is JSON': (r) => r.headers['Content-Type'] === 'application/json',
  });
  
  // Get available therapists
  const therapistsResponse = http.get(`${baseUrl}/api/therapists`);
  check(therapistsResponse, {
    'therapists status is 200': (r) => r.status === 200,
    'therapists response is JSON': (r) => r.headers['Content-Type'] === 'application/json',
  });
  
  // Get available slots
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const tomorrowStr = tomorrow.toISOString().split('T')[0];
  
  const nextWeek = new Date();
  nextWeek.setDate(nextWeek.getDate() + 7);
  const nextWeekStr = nextWeek.toISOString().split('T')[0];
  
  const availabilityResponse = http.get(
    `${baseUrl}/api/appointments/available?startDate=${tomorrowStr}&endDate=${nextWeekStr}`
  );
  
  check(availabilityResponse, {
    'availability status is 200': (r) => r.status === 200,
    'availability response is JSON': (r) => r.headers['Content-Type'] === 'application/json',
  });
  
  sleep(1);
}
```

Example unit test:

```csharp
public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _mockRepository;
    private readonly Mock<IServiceRepository> _mockServiceRepository;
    private readonly Mock<ITherapistRepository> _mockTherapistRepository;
    private readonly Mock<ILogger<AppointmentService>> _mockLogger;
    private readonly AppointmentService _service;
    
    public AppointmentServiceTests()
    {
        _mockRepository = new Mock<IAppointmentRepository>();
        _mockServiceRepository = new Mock<IServiceRepository>();
        _mockTherapistRepository = new Mock<ITherapistRepository>();
        _mockLogger = new Mock<ILogger<AppointmentService>>();
        
        _service = new AppointmentService(
            _mockRepository.Object,
            _mockServiceRepository.Object,
            _mockTherapistRepository.Object,
            _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateAppointment_WithValidData_ReturnsCreatedAppointment()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ClientId = Guid.NewGuid(),
            TherapistId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(1)
        };
        
        var service = new Service
        {
            ServiceId = dto.ServiceId,
            Duration = 60
        };
        
        _mockServiceRepository
            .Setup(r => r.GetByIdAsync(dto.ServiceId))
            .ReturnsAsync(service);
            
        _mockTherapistRepository
            .Setup(r => r.IsAvailableAsync(dto.TherapistId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);
            
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Appointment>()))
            .Returns(Task.CompletedTask);
            
        // Act
        var result = await _service.CreateAppointmentAsync(dto);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.ClientId, result.ClientId);
        Assert.Equal(dto.TherapistId, result.TherapistId);
        Assert.Equal(dto.ServiceId, result.ServiceId);
        Assert.Equal(dto.StartTime, result.StartTime);
        Assert.Equal(dto.StartTime.AddMinutes(service.Duration), result.EndTime);
        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
    }
    
    [Fact]
    public async Task CreateAppointment_WithUnavailableTherapist_ThrowsException()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ClientId = Guid.NewGuid(),
            TherapistId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(1)
        };
        
        var service = new Service
        {
            ServiceId = dto.ServiceId,
            Duration = 60
        };
        
        _mockServiceRepository
            .Setup(r => r.GetByIdAsync(dto.ServiceId))
            .ReturnsAsync(service);
            
        _mockTherapistRepository
            .Setup(r => r.IsAvailableAsync(dto.TherapistId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
            
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAppointmentAsync(dto));
    }
}
```

## UI Components

### Appointment Calendar View

The system includes a comprehensive appointment calendar view that provides a weekly visualization of appointments and available slots.

```jsx
// AppointmentCalendarView component usage example
<AppointmentCalendarView 
  serviceId={selectedServiceId} 
  therapistId={selectedTherapistId}
  onSlotSelect={handleSlotSelect}
  initialDate={new Date()}
/>
```

#### Features

- Weekly calendar view with day columns and hour-based time slots
- Color-coded appointment status visualization (Scheduled, Completed, Cancelled, NoShow)
- Available slot highlighting and selection
- Week navigation with previous/next buttons
- Responsive design for mobile and desktop
- Built-in appointment booking dialog
- Loading and error states with retry capability
- Filtering by service and therapist

#### API Integration

The component integrates with the following API endpoints:

1. **Fetch Appointments**: 
   - `GET /api/appointments?userId={userId}&startDate={startDate}&endDate={endDate}`
   - Returns scheduled appointments for the specified user and date range

2. **Fetch Available Slots**:
   - `GET /api/appointments/available?serviceId={serviceId}&therapistId={therapistId}&startDate={startDate}&endDate={endDate}`
   - Returns available time slots based on service, therapist, and date range

3. **Create Appointment**:
   - `POST /api/appointments`
   - Creates a new appointment when a user selects an available slot

#### Data Models

```typescript
// Appointment data model
interface AppointmentDto {
  appointmentId: string;
  clientId: string;
  clientName: string;
  therapistId: string;
  therapistName: string;
  serviceId: string;
  serviceName: string;
  startTime: string; // ISO date string
  endTime: string;   // ISO date string
  status: "Scheduled" | "Completed" | "Cancelled" | "NoShow";
  notes: string;
}

// Available slot data model
interface AvailableSlotDto {
  startTime: string; // ISO date string
  endTime: string;   // ISO date string
  therapistId?: string;
  therapistName?: string;
}
```

### API Services

The system implements dedicated API service modules for appointment, service, and therapist data:

```typescript
// Appointment API service
export const fetchAppointments = async (userId, startDate, endDate) => {
  // Implementation
};

export const fetchAvailableSlots = async (serviceId, therapistId, startDate, endDate) => {
  // Implementation
};

export const createAppointment = async (appointmentData) => {
  // Implementation
};

// Service API service
export const fetchServices = async () => {
  // Implementation
};

// Therapist API service
export const fetchTherapists = async (filters = {}) => {
  // Implementation
};
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
import { appointmentApi } from '../api';

export function useAppointments(clientId) {
  const [appointments, setAppointments] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  
  useEffect(() => {
    let isMounted = true;
    
    async function fetchData() {
      try {
        setIsLoading(true);
        const response = await appointmentApi.getAppointmentsByClientId(clientId);
        
        if (isMounted) {
          setAppointments(response.data);
          setError(null);
        }
      } catch (err) {
        if (isMounted) {
          setError('Failed to load appointments');
          console.error(err);
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }
    
    fetchData();
    
    return () => {
      isMounted = false;
    };
  }, [clientId]);
  
  return { appointments, isLoading, error };
}
```

## Core Components

### Appointment Calendar View

The Appointment Calendar View provides a weekly visualization of appointments and available slots, making it easy for staff to see the schedule at a glance.

```jsx
<AppointmentCalendarView 
  therapistId="guid-here" 
  startDate="2023-05-01" 
  onAppointmentClick={handleAppointmentClick}
  onAvailableSlotClick={handleAvailableSlotClick}
/>
```

Features:
- Color-coded visualization of appointment status (scheduled, completed, cancelled, no-show)
- Highlighting of available slots
- Week navigation (previous/next)
- Responsive design adapting to different screen sizes
- Built-in appointment booking dialog when clicking available slots

API Integration:
- Fetches appointments: `GET /api/appointments/therapist/{therapistId}?startDate={date}&endDate={date}`
- Fetches available slots: `GET /api/therapists/{therapistId}/availability?startDate={date}&endDate={date}`
- Creates appointments: `POST /api/appointments`

Data Models:
```typescript
interface Appointment {
  appointmentId: string;
  clientId: string;
  clientName: string;
  therapistId: string;
  therapistName: string;
  serviceId: string;
  serviceName: string;
  startTime: string;
  endTime: string;
  status: 'Scheduled' | 'Completed' | 'Cancelled' | 'NoShow';
  notes: string;
}

interface AvailableSlot {
  therapistId: string;
  date: string;
  startTime: string;
  endTime: string;
  isAvailable: boolean;
}
```

### Admin Dashboard

The Admin Dashboard provides comprehensive reporting and management features for administrators.

#### Dashboard Components

1. **Summary Dashboard**
   - Display key metrics (appointments, clients, revenue)
   - Visual charts for appointment trends
   - Quick access to common actions

2. **Appointment Reports**
   - Filter by date range, therapist, or service
   - View appointments by status (scheduled, completed, cancelled, no-show)
   - Export reports to CSV

3. **Revenue Reports**
   - Revenue by service type
   - Revenue by therapist
   - Period comparisons (daily, weekly, monthly)
   - Revenue forecasting

4. **User Management**
   - View and manage clients and therapists
   - User activity logs
   - Permission management

API Endpoints:
- `GET /api/admin/dashboard` - Get summary statistics
- `GET /api/admin/reports/appointments` - Get appointment reports
- `GET /api/admin/reports/revenue` - Get revenue reports
- `GET /api/admin/audit-logs` - Get audit logs

Sample Response for Dashboard Statistics:
```json
{
  "totalAppointmentsThisMonth": 125,
  "completedAppointmentsThisMonth": 78,
  "cancelledAppointmentsThisMonth": 12,
  "upcomingAppointments": 35,
  "activeTherapistsCount": 8,
  "totalClientsCount": 230,
  "dailyAppointmentCounts": [
    { "date": "2023-05-01", "count": 12 },
    { "date": "2023-05-02", "count": 15 },
    // ...more daily counts
  ]
}
```

### Email Notifications

The Email Notification system provides automated communication with clients and therapists about appointments and account activities.

#### Email Service

The `EmailService` is responsible for sending various types of emails:

```csharp
public interface IEmailService
{
    Task<bool> SendAppointmentConfirmationAsync(Appointment appointment, Client client, Therapist therapist, Service service);
    Task<bool> SendAppointmentReminderAsync(Appointment appointment, Client client, Therapist therapist, Service service);
    Task<bool> SendAppointmentCancellationAsync(Appointment appointment, Client client, Therapist therapist, Service service);
    Task<bool> SendTherapistAppointmentNotificationAsync(Appointment appointment, Client client, Therapist therapist, Service service);
    Task<bool> SendPasswordResetAsync(string email, string resetToken);
    Task<bool> SendWelcomeEmailAsync(Client client);
}
```

Implementation Details:
- Uses SMTP for email delivery
- HTML-formatted email templates
- Configured through `EmailSettings` in appsettings.json
- Includes tracking and logging of email sending status

Email Settings Configuration:
```json
"EmailSettings": {
  "SmtpServer": "smtp.example.com",
  "SmtpPort": 587,
  "SmtpUsername": "notifications@example.com",
  "SmtpPassword": "SecurePassword123",
  "EnableSsl": true,
  "SenderEmail": "notifications@example.com",
  "SenderName": "Massage Therapy Booking",
  "WebsiteBaseUrl": "https://massagebooking.example.com"
}
```

### Availability Management

The Availability Management system handles therapist availability for scheduling.

#### Availability Repository

```csharp
public interface IAvailabilityRepository
{
    Task<Availability> GetAvailabilityForDateAsync(Guid therapistId, DateTime date);
    Task<Availability> GetAvailabilityForDayOfWeekAsync(Guid therapistId, DayOfWeek dayOfWeek);
    Task<Availability> UpsertDateAvailabilityAsync(
        Guid therapistId, 
        DateTime date, 
        bool isAvailable, 
        TimeSpan? startTime, 
        TimeSpan? endTime, 
        TimeSpan? breakStartTime, 
        TimeSpan? breakEndTime, 
        string notes);
    Task<Availability> UpsertDayOfWeekAvailabilityAsync(
        Guid therapistId,
        DayOfWeek dayOfWeek,
        bool isAvailable,
        TimeSpan? startTime,
        TimeSpan? endTime,
        TimeSpan? breakStartTime,
        TimeSpan? breakEndTime,
        string notes);
    Task<bool> DeleteDateAvailabilityAsync(Guid therapistId, DateTime date);
    Task<bool> DeleteDayOfWeekAvailabilityAsync(Guid therapistId, DayOfWeek dayOfWeek);
}
```

Features:
- Support for both specific date availability and recurring day-of-week patterns
- Ability to set availability windows including breaks
- Efficient querying for appointment scheduling
- Support for therapist-specific availability rules

#### Availability Model

```csharp
public class Availability
{
    public Guid AvailabilityId { get; set; }
    public Guid TherapistId { get; set; }
    public virtual Therapist Therapist { get; set; }
    public DateTime? SpecificDate { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public bool IsAvailable { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### SOAP Notes

The SOAP Notes feature allows therapists to document client treatments using the SOAP method (Subjective, Objective, Assessment, Plan).

#### SOAP Note Model

```csharp
public class SoapNote
{
    public Guid SoapNoteId { get; set; }
    public Guid AppointmentId { get; set; }
    public virtual Appointment Appointment { get; set; }
    public Guid TherapistId { get; set; }
    public virtual Therapist Therapist { get; set; }
    public Guid ClientId { get; set; }
    public virtual Client Client { get; set; }
    public string Subjective { get; set; }
    public string Objective { get; set; }
    public string Assessment { get; set; }
    public string Plan { get; set; }
    public string AreasOfFocus { get; set; }
    public string TechniquesUsed { get; set; }
    public int? PressureLevel { get; set; }
    public bool IsFinalized { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
}
```

#### SOAP Note Repository

```csharp
public interface ISoapNoteRepository
{
    Task<SoapNote> GetByIdAsync(Guid soapNoteId);
    Task<SoapNote> GetByAppointmentIdAsync(Guid appointmentId);
    Task<List<SoapNote>> GetByClientIdAsync(Guid clientId);
    Task<List<SoapNote>> GetByTherapistIdAsync(Guid therapistId);
    Task<SoapNote> AddAsync(SoapNote soapNote);
    Task<SoapNote> UpdateAsync(SoapNote soapNote);
    Task<bool> FinalizeAsync(Guid soapNoteId);
    Task<bool> DeleteAsync(Guid soapNoteId);
}
```

#### SOAP Notes Controller

The `SoapNotesController` provides API endpoints for SOAP note management:

- `GET /api/soapnotes/{id}` - Get a specific SOAP note
- `GET /api/soapnotes/appointment/{appointmentId}` - Get SOAP note for an appointment
- `GET /api/soapnotes/client/{clientId}` - Get all SOAP notes for a client
- `GET /api/soapnotes/therapist/{therapistId}` - Get all SOAP notes by a therapist
- `POST /api/soapnotes` - Create a new SOAP note
- `PUT /api/soapnotes/{id}` - Update an existing SOAP note
- `POST /api/soapnotes/{id}/finalize` - Finalize a SOAP note
- `DELETE /api/soapnotes/{id}` - Delete a SOAP note

Features:
- Structured documentation with SOAP format
- Support for additional treatment details (techniques, pressure level)
- Note finalization to prevent further edits
- Access controls (therapists can only view their own clients' notes)

## API Documentation

API documentation is available through Swagger UI at `/swagger` when running the application in development mode. This provides interactive documentation for all available endpoints, including request parameters, response schemas, and authentication requirements.
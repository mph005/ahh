using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MassageBookingSystem.Services
{
    /// <summary>
    /// Enum representing the current status of an appointment
    /// </summary>
    public enum AppointmentStatus
    {
        Scheduled,
        Completed,
        Cancelled,
        NoShow
    }

    /// <summary>
    /// DTO for creating a new appointment
    /// </summary>
    public class CreateAppointmentDto
    {
        public Guid ClientId { get; set; }
        public Guid TherapistId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing appointment
    /// </summary>
    public class UpdateAppointmentDto
    {
        public Guid AppointmentId { get; set; }
        public Guid? TherapistId { get; set; }
        public Guid? ServiceId { get; set; }
        public DateTime? StartTime { get; set; }
        public string Notes { get; set; }
        public AppointmentStatus? Status { get; set; }
    }

    /// <summary>
    /// DTO for appointment data
    /// </summary>
    public class AppointmentDto
    {
        public Guid AppointmentId { get; set; }
        public Guid ClientId { get; set; }
        public string ClientName { get; set; }
        public Guid TherapistId { get; set; }
        public string TherapistName { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO for representing available appointment slots
    /// </summary>
    public class AvailableSlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? TherapistId { get; set; }
        public string TherapistName { get; set; }
    }

    /// <summary>
    /// Service class for managing appointments
    /// </summary>
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ITherapistRepository _therapistRepository;
        private readonly ILogger<AppointmentService> _logger;
        
        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IServiceRepository serviceRepository,
            ITherapistRepository therapistRepository,
            ILogger<AppointmentService> logger)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _therapistRepository = therapistRepository ?? throw new ArgumentNullException(nameof(therapistRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Gets an appointment by its ID
        /// </summary>
        /// <param name="id">The appointment ID</param>
        /// <returns>The appointment details</returns>
        public async Task<AppointmentDto> GetAppointmentByIdAsync(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with ID {id} not found");
            }
            
            return MapToDto(appointment);
        }
        
        /// <summary>
        /// Gets all appointments
        /// </summary>
        /// <returns>A collection of all appointments</returns>
        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync()
        {
            var appointments = await _appointmentRepository.GetAllAsync();
            var result = new List<AppointmentDto>();
            
            foreach (var appointment in appointments)
            {
                result.Add(MapToDto(appointment));
            }
            
            return result;
        }
        
        /// <summary>
        /// Creates a new appointment
        /// </summary>
        /// <param name="appointmentDto">The appointment data</param>
        /// <returns>The created appointment</returns>
        public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto appointmentDto)
        {
            if (appointmentDto == null)
            {
                throw new ArgumentNullException(nameof(appointmentDto));
            }
            
            // Validate input
            await ValidateAppointmentCreationAsync(appointmentDto);
            
            // Get service to calculate end time
            var service = await _serviceRepository.GetByIdAsync(appointmentDto.ServiceId);
            var endTime = appointmentDto.StartTime.AddMinutes(service.Duration);
            
            // Create appointment entity
            var appointment = new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                ClientId = appointmentDto.ClientId,
                TherapistId = appointmentDto.TherapistId,
                ServiceId = appointmentDto.ServiceId,
                StartTime = appointmentDto.StartTime,
                EndTime = endTime,
                Status = AppointmentStatus.Scheduled,
                Notes = appointmentDto.Notes,
                CreatedAt = DateTime.UtcNow
            };
            
            await _appointmentRepository.AddAsync(appointment);
            
            _logger.LogInformation("Created appointment {AppointmentId} for client {ClientId} with therapist {TherapistId}", 
                appointment.AppointmentId, appointment.ClientId, appointment.TherapistId);
            
            return MapToDto(appointment);
        }
        
        /// <summary>
        /// Updates an existing appointment
        /// </summary>
        /// <param name="appointmentDto">The updated appointment data</param>
        public async Task UpdateAppointmentAsync(UpdateAppointmentDto appointmentDto)
        {
            if (appointmentDto == null)
            {
                throw new ArgumentNullException(nameof(appointmentDto));
            }
            
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentDto.AppointmentId);
            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with ID {appointmentDto.AppointmentId} not found");
            }
            
            // If appointment is already completed or cancelled, it can't be modified
            if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new InvalidOperationException($"Cannot update appointment with status {appointment.Status}");
            }
            
            // Check if we need to validate scheduling changes
            bool needsSchedulingValidation = 
                (appointmentDto.TherapistId.HasValue && appointmentDto.TherapistId.Value != appointment.TherapistId) ||
                (appointmentDto.ServiceId.HasValue && appointmentDto.ServiceId.Value != appointment.ServiceId) ||
                (appointmentDto.StartTime.HasValue && appointmentDto.StartTime.Value != appointment.StartTime);
            
            DateTime newEndTime = appointment.EndTime;
            
            if (needsSchedulingValidation)
            {
                Guid therapistId = appointmentDto.TherapistId ?? appointment.TherapistId;
                Guid serviceId = appointmentDto.ServiceId ?? appointment.ServiceId;
                DateTime startTime = appointmentDto.StartTime ?? appointment.StartTime;
                
                // Get service to calculate end time
                var service = await _serviceRepository.GetByIdAsync(serviceId);
                newEndTime = startTime.AddMinutes(service.Duration);
                
                // Ensure therapist is available
                bool isAvailable = await _therapistRepository.IsAvailableAsync(
                    therapistId, 
                    startTime, 
                    newEndTime,
                    appointment.AppointmentId);
                
                if (!isAvailable)
                {
                    throw new InvalidOperationException("The therapist is not available at the requested time");
                }
            }
            
            // Update appointment properties
            if (appointmentDto.TherapistId.HasValue)
                appointment.TherapistId = appointmentDto.TherapistId.Value;
                
            if (appointmentDto.ServiceId.HasValue)
                appointment.ServiceId = appointmentDto.ServiceId.Value;
                
            if (appointmentDto.StartTime.HasValue)
            {
                appointment.StartTime = appointmentDto.StartTime.Value;
                appointment.EndTime = newEndTime;
            }
                
            if (appointmentDto.Status.HasValue)
                appointment.Status = appointmentDto.Status.Value;
                
            if (appointmentDto.Notes != null)
                appointment.Notes = appointmentDto.Notes;
                
            appointment.UpdatedAt = DateTime.UtcNow;
            
            await _appointmentRepository.UpdateAsync(appointment);
            
            _logger.LogInformation("Updated appointment {AppointmentId}", appointment.AppointmentId);
        }
        
        /// <summary>
        /// Cancels an existing appointment
        /// </summary>
        /// <param name="id">The appointment ID</param>
        public async Task CancelAppointmentAsync(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with ID {id} not found");
            }
            
            // If appointment is already completed or cancelled, it can't be cancelled
            if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new InvalidOperationException($"Cannot cancel appointment with status {appointment.Status}");
            }
            
            // Check cancellation window - Example: 24 hours before appointment
            TimeSpan cancellationWindow = TimeSpan.FromHours(24);
            if (DateTime.UtcNow + cancellationWindow > appointment.StartTime)
            {
                _logger.LogWarning("Appointment {AppointmentId} cancelled within {Hours} hour window", 
                    appointment.AppointmentId, cancellationWindow.TotalHours);
                // Could add late cancellation fee logic here
            }
            
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;
            
            await _appointmentRepository.UpdateAsync(appointment);
            
            _logger.LogInformation("Cancelled appointment {AppointmentId}", appointment.AppointmentId);
        }
        
        /// <summary>
        /// Gets available appointment slots based on service, therapist, and date range
        /// </summary>
        /// <param name="serviceId">The service ID</param>
        /// <param name="therapistId">The therapist ID (optional)</param>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <returns>A collection of available appointment slots</returns>
        public async Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(
            Guid serviceId, Guid? therapistId, DateTime startDate, DateTime endDate)
        {
            // Validate input
            if (startDate >= endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }
            
            if (startDate < DateTime.UtcNow)
            {
                startDate = DateTime.UtcNow;
            }
            
            // Get the service to know the duration
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null)
            {
                throw new KeyNotFoundException($"Service with ID {serviceId} not found");
            }
            
            // Get available slots from therapist repository
            var slots = await _therapistRepository.GetAvailableSlotsAsync(
                serviceId, therapistId, startDate, endDate, service.Duration);
                
            return slots;
        }
        
        #region Private Methods
        
        /// <summary>
        /// Validates appointment creation data
        /// </summary>
        /// <param name="appointmentDto">The appointment data to validate</param>
        private async Task ValidateAppointmentCreationAsync(CreateAppointmentDto appointmentDto)
        {
            // Check if start time is in the future
            if (appointmentDto.StartTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("Appointment start time must be in the future");
            }
            
            // Check if the service exists
            var service = await _serviceRepository.GetByIdAsync(appointmentDto.ServiceId);
            if (service == null)
            {
                throw new KeyNotFoundException($"Service with ID {appointmentDto.ServiceId} not found");
            }
            
            // Calculate end time
            DateTime endTime = appointmentDto.StartTime.AddMinutes(service.Duration);
            
            // Check if the therapist is available at the requested time
            bool isAvailable = await _therapistRepository.IsAvailableAsync(
                appointmentDto.TherapistId, appointmentDto.StartTime, endTime);
                
            if (!isAvailable)
            {
                throw new InvalidOperationException("The therapist is not available at the requested time");
            }
            
            // Additional validations can be added here
            // - Check if client exists
            // - Check business hours
            // - Check therapist qualifications for the service
            // - Check client membership status
        }
        
        /// <summary>
        /// Maps an Appointment entity to its DTO representation
        /// </summary>
        /// <param name="appointment">The appointment entity</param>
        /// <returns>The appointment DTO</returns>
        private AppointmentDto MapToDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                ClientId = appointment.ClientId,
                ClientName = appointment.Client?.FullName,
                TherapistId = appointment.TherapistId,
                TherapistName = appointment.Therapist?.FullName,
                ServiceId = appointment.ServiceId,
                ServiceName = appointment.Service?.Name,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                Notes = appointment.Notes
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Interface for the AppointmentService
    /// </summary>
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
    
    /// <summary>
    /// Appointment entity
    /// </summary>
    public class Appointment
    {
        public Guid AppointmentId { get; set; }
        public Guid ClientId { get; set; }
        public Client Client { get; set; }
        public Guid TherapistId { get; set; }
        public Therapist Therapist { get; set; }
        public Guid ServiceId { get; set; }
        public Service Service { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 
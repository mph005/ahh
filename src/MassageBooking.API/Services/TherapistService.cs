using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MassageBooking.API.Data.Repositories;
using MassageBooking.API.Models;
using MassageBooking.API.DTOs;

namespace MassageBooking.API.Services
{
    /// <summary>
    /// Service for handling therapist-related business logic
    /// </summary>
    public class TherapistService : ITherapistService
    {
        private readonly ITherapistRepository _therapistRepository;
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger<TherapistService> _logger;

        public TherapistService(
            ITherapistRepository therapistRepository,
            IAvailabilityRepository availabilityRepository,
            IAppointmentRepository appointmentRepository,
            ILogger<TherapistService> logger)
        {
            _therapistRepository = therapistRepository ?? throw new ArgumentNullException(nameof(therapistRepository));
            _availabilityRepository = availabilityRepository ?? throw new ArgumentNullException(nameof(availabilityRepository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<TherapistDetailsDTO> GetTherapistByIdAsync(Guid therapistId)
        {
            try
            {
                var therapist = await _therapistRepository.GetByIdAsync(therapistId);
                if (therapist == null)
                {
                    return null;
                }

                return MapToTherapistDetailsDTO(therapist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving therapist with ID {TherapistId}", therapistId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<List<TherapistListItemDTO>> GetAllTherapistsAsync()
        {
            try
            {
                var therapists = await _therapistRepository.GetAllAsync();
                return therapists.Select(MapToTherapistListItemDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all therapists");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<List<ServiceDTO>> GetTherapistServicesAsync(Guid therapistId)
        {
            try
            {
                var services = await _therapistRepository.GetTherapistServicesAsync(therapistId);
                return services.Select(s => new ServiceDTO
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Description = s.Description,
                    Duration = s.Duration,
                    Price = s.Price,
                    IsActive = s.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services for therapist {TherapistId}", therapistId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<TherapistAvailabilityDTO> GetTherapistAvailabilityAsync(Guid therapistId, DateTime date)
        {
            try
            {
                // Try to get a specific availability override for the date
                var dateAvailability = await _availabilityRepository.GetAvailabilityForDateAsync(therapistId, date);
                
                // If there's no specific override, get the standard day of week availability
                if (dateAvailability == null)
                {
                    dateAvailability = await _availabilityRepository.GetAvailabilityForDayOfWeekAsync(
                        therapistId, date.DayOfWeek);
                }

                if (dateAvailability == null)
                {
                    return null;
                }

                var therapist = await _therapistRepository.GetByIdAsync(therapistId);
                if (therapist == null)
                {
                    return null;
                }

                return new TherapistAvailabilityDTO
                {
                    TherapistId = therapistId,
                    TherapistName = $"{therapist.FirstName} {therapist.LastName}",
                    Date = date,
                    DayOfWeek = date.DayOfWeek,
                    IsAvailable = dateAvailability.IsAvailable,
                    StartTime = dateAvailability.StartTime,
                    EndTime = dateAvailability.EndTime,
                    BreakStartTime = dateAvailability.BreakStartTime,
                    BreakEndTime = dateAvailability.BreakEndTime,
                    Notes = dateAvailability.Notes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving availability for therapist {TherapistId} on date {Date}", 
                    therapistId, date);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<OperationResultDTO> UpdateTherapistAvailabilityAsync(
            Guid therapistId, 
            UpdateAvailabilityRequestDTO request)
        {
            try
            {
                // Validate inputs
                if (!request.Date.HasValue && !request.DayOfWeek.HasValue)
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = "Either Date or DayOfWeek must be provided."
                    };
                }

                if (request.IsAvailable && (!request.StartTime.HasValue || !request.EndTime.HasValue))
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = "Start time and end time must be provided when setting availability to true."
                    };
                }

                if (request.StartTime.HasValue && request.EndTime.HasValue && 
                    request.StartTime.Value >= request.EndTime.Value)
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = "Start time must be before end time."
                    };
                }

                if (request.BreakStartTime.HasValue != request.BreakEndTime.HasValue)
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = "Both break start time and break end time must be provided or neither."
                    };
                }

                if (request.BreakStartTime.HasValue && request.BreakEndTime.HasValue && 
                    request.BreakStartTime.Value >= request.BreakEndTime.Value)
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = "Break start time must be before break end time."
                    };
                }

                // Make sure therapist exists
                var therapist = await _therapistRepository.GetByIdAsync(therapistId);
                if (therapist == null)
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = $"Therapist with ID {therapistId} not found."
                    };
                }

                // Check for scheduling conflicts if updating date availability
                if (request.Date.HasValue && request.IsAvailable)
                {
                    var date = request.Date.Value.Date;
                    var startDateTime = date.Add(request.StartTime.Value);
                    var endDateTime = date.Add(request.EndTime.Value);
                    
                    var hasConflicts = await _appointmentRepository.GetAppointmentsInRangeAsync(
                        startDateTime, endDateTime);
                    
                    if (hasConflicts.Any(a => a.TherapistId == therapistId && a.Status != "Cancelled"))
                    {
                        return new OperationResultDTO
                        {
                            Success = false,
                            ErrorMessage = "There are existing appointments during this time period."
                        };
                    }
                }

                // Create or update availability
                if (request.Date.HasValue)
                {
                    var date = request.Date.Value.Date;
                    
                    // Create date-specific availability
                    await _availabilityRepository.UpsertDateAvailabilityAsync(
                        therapistId,
                        date,
                        request.IsAvailable,
                        request.StartTime,
                        request.EndTime,
                        request.BreakStartTime,
                        request.BreakEndTime,
                        request.Notes);
                    
                    // If it's recurring, also update the day of week
                    if (request.IsRecurring)
                    {
                        await _availabilityRepository.UpsertDayOfWeekAvailabilityAsync(
                            therapistId,
                            date.DayOfWeek,
                            request.IsAvailable,
                            request.StartTime,
                            request.EndTime,
                            request.BreakStartTime,
                            request.BreakEndTime,
                            request.Notes);
                    }
                }
                else if (request.DayOfWeek.HasValue)
                {
                    // Create/update day of week availability
                    await _availabilityRepository.UpsertDayOfWeekAvailabilityAsync(
                        therapistId,
                        request.DayOfWeek.Value,
                        request.IsAvailable,
                        request.StartTime,
                        request.EndTime,
                        request.BreakStartTime,
                        request.BreakEndTime,
                        request.Notes);
                }

                return new OperationResultDTO { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating availability for therapist {TherapistId}: {@Request}", 
                    therapistId, request);
                
                return new OperationResultDTO
                {
                    Success = false,
                    ErrorMessage = "An error occurred while updating the availability. Please try again later."
                };
            }
        }

        /// <inheritdoc />
        public async Task<OperationResultDTO> BlockTherapistTimeAsync(
            Guid therapistId, 
            BlockTimeRequestDTO request)
        {
            try
            {
                // Validate inputs
                if (request.StartDateTime >= request.EndDateTime)
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = "Start time must be before end time."
                    };
                }

                // Make sure therapist exists
                var therapist = await _therapistRepository.GetByIdAsync(therapistId);
                if (therapist == null)
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = $"Therapist with ID {therapistId} not found."
                    };
                }

                // Check for existing appointments
                var existingAppointments = await _appointmentRepository.GetAppointmentsInRangeAsync(
                    request.StartDateTime, request.EndDateTime);
                
                var therapistAppointments = existingAppointments
                    .Where(a => a.TherapistId == therapistId && a.Status != "Cancelled")
                    .ToList();
                
                if (therapistAppointments.Any() && !request.OverrideExistingAppointments)
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = $"There are {therapistAppointments.Count} existing appointments during this time period. Set OverrideExistingAppointments to true to override them."
                    };
                }

                // If overriding, cancel the appointments
                if (therapistAppointments.Any() && request.OverrideExistingAppointments)
                {
                    foreach (var appointment in therapistAppointments)
                    {
                        appointment.Status = "Cancelled";
                        appointment.Notes = $"{appointment.Notes}\nCancelled due to therapist unavailability: {request.Reason}";
                        appointment.UpdatedAt = DateTime.UtcNow;
                        
                        await _appointmentRepository.UpdateAsync(appointment);
                    }
                }

                // Block out each date in the range by creating date-specific availability records
                for (var date = request.StartDateTime.Date; date <= request.EndDateTime.Date; date = date.AddDays(1))
                {
                    // Compute the start and end time for this specific date
                    TimeSpan? startTime = null;
                    TimeSpan? endTime = null;
                    
                    // For the first day, use the start time from the request
                    if (date == request.StartDateTime.Date)
                    {
                        startTime = request.StartDateTime.TimeOfDay;
                    }
                    
                    // For the last day, use the end time from the request
                    if (date == request.EndDateTime.Date)
                    {
                        endTime = request.EndDateTime.TimeOfDay;
                    }
                    
                    // Create a date-specific unavailability record
                    await _availabilityRepository.UpsertDateAvailabilityAsync(
                        therapistId,
                        date,
                        false, // Not available
                        startTime,
                        endTime,
                        null, // No break times for unavailability
                        null,
                        request.Reason);
                }

                return new OperationResultDTO { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking time for therapist {TherapistId}: {@Request}", 
                    therapistId, request);
                
                return new OperationResultDTO
                {
                    Success = false,
                    ErrorMessage = "An error occurred while blocking the time. Please try again later."
                };
            }
        }

        /// <inheritdoc />
        public async Task<OperationResultDTO> AddTherapistAsync(CreateTherapistDTO request)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = "First name and last name are required."
                    };
                }

                // Create therapist entity
                var therapist = new Therapist
                {
                    TherapistId = Guid.NewGuid(),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Title = request.Title,
                    Bio = request.Bio,
                    ImageUrl = request.ImageUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add therapist
                await _therapistRepository.AddAsync(therapist);

                // Set up default availability (Mon-Fri, 9am-5pm)
                for (var day = DayOfWeek.Monday; day <= DayOfWeek.Friday; day++)
                {
                    await _availabilityRepository.UpsertDayOfWeekAvailabilityAsync(
                        therapist.TherapistId,
                        day,
                        true, // Available
                        new TimeSpan(9, 0, 0), // 9:00 AM
                        new TimeSpan(17, 0, 0), // 5:00 PM
                        new TimeSpan(12, 0, 0), // 12:00 PM (lunch break)
                        new TimeSpan(13, 0, 0), // 1:00 PM (end of lunch break)
                        "Default availability");
                }

                // Set up weekend as unavailable by default
                await _availabilityRepository.UpsertDayOfWeekAvailabilityAsync(
                    therapist.TherapistId,
                    DayOfWeek.Saturday,
                    false, // Not available
                    null,
                    null,
                    null,
                    null,
                    "Weekend - Not available by default");

                await _availabilityRepository.UpsertDayOfWeekAvailabilityAsync(
                    therapist.TherapistId,
                    DayOfWeek.Sunday,
                    false, // Not available
                    null,
                    null,
                    null,
                    null,
                    "Weekend - Not available by default");

                return new OperationResultDTO { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating therapist: {@Request}", request);
                
                return new OperationResultDTO
                {
                    Success = false,
                    ErrorMessage = "An error occurred while creating the therapist. Please try again later."
                };
            }
        }

        /// <inheritdoc />
        public async Task<OperationResultDTO> UpdateTherapistAsync(Guid therapistId, UpdateTherapistDTO request)
        {
            try
            {
                // Get existing therapist
                var therapist = await _therapistRepository.GetByIdAsync(therapistId);
                if (therapist == null)
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = $"Therapist with ID {therapistId} not found."
                    };
                }

                // Update properties
                if (!string.IsNullOrWhiteSpace(request.FirstName))
                {
                    therapist.FirstName = request.FirstName;
                }
                
                if (!string.IsNullOrWhiteSpace(request.LastName))
                {
                    therapist.LastName = request.LastName;
                }
                
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    therapist.Email = request.Email;
                }
                
                if (!string.IsNullOrWhiteSpace(request.Phone))
                {
                    therapist.Phone = request.Phone;
                }
                
                if (!string.IsNullOrWhiteSpace(request.Title))
                {
                    therapist.Title = request.Title;
                }
                
                if (!string.IsNullOrWhiteSpace(request.Bio))
                {
                    therapist.Bio = request.Bio;
                }
                
                if (!string.IsNullOrWhiteSpace(request.ImageUrl))
                {
                    therapist.ImageUrl = request.ImageUrl;
                }
                
                if (request.IsActive.HasValue)
                {
                    therapist.IsActive = request.IsActive.Value;
                }
                
                therapist.UpdatedAt = DateTime.UtcNow;

                // Update therapist
                await _therapistRepository.UpdateAsync(therapist);

                return new OperationResultDTO { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating therapist {TherapistId}: {@Request}", 
                    therapistId, request);
                
                return new OperationResultDTO
                {
                    Success = false,
                    ErrorMessage = "An error occurred while updating the therapist. Please try again later."
                };
            }
        }

        /// <inheritdoc />
        public async Task<OperationResultDTO> AddServiceToTherapistAsync(Guid therapistId, Guid serviceId)
        {
            try
            {
                var result = await _therapistRepository.AddServiceToTherapistAsync(therapistId, serviceId);
                
                if (!result)
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = "Unable to add service to therapist. Either the therapist or service does not exist or the association already exists."
                    };
                }
                
                return new OperationResultDTO { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding service {ServiceId} to therapist {TherapistId}", 
                    serviceId, therapistId);
                
                return new OperationResultDTO
                {
                    Success = false,
                    ErrorMessage = "An error occurred while adding the service to the therapist. Please try again later."
                };
            }
        }

        /// <inheritdoc />
        public async Task<OperationResultDTO> RemoveServiceFromTherapistAsync(Guid therapistId, Guid serviceId)
        {
            try
            {
                var result = await _therapistRepository.RemoveServiceFromTherapistAsync(therapistId, serviceId);
                
                if (!result)
                {
                    return new OperationResultDTO
                    {
                        Success = false,
                        ErrorMessage = "Unable to remove service from therapist. Either the therapist or service does not exist or the association does not exist."
                    };
                }
                
                return new OperationResultDTO { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing service {ServiceId} from therapist {TherapistId}", 
                    serviceId, therapistId);
                
                return new OperationResultDTO
                {
                    Success = false,
                    ErrorMessage = "An error occurred while removing the service from the therapist. Please try again later."
                };
            }
        }

        #region Mapping Methods

        private TherapistDetailsDTO MapToTherapistDetailsDTO(Therapist therapist)
        {
            return new TherapistDetailsDTO
            {
                TherapistId = therapist.TherapistId,
                FirstName = therapist.FirstName,
                LastName = therapist.LastName,
                FullName = $"{therapist.FirstName} {therapist.LastName}",
                Email = therapist.Email,
                Phone = therapist.Phone,
                Title = therapist.Title,
                Bio = therapist.Bio,
                ImageUrl = therapist.ImageUrl,
                IsActive = therapist.IsActive,
                CreatedAt = therapist.CreatedAt,
                UpdatedAt = therapist.UpdatedAt
            };
        }

        private TherapistListItemDTO MapToTherapistListItemDTO(Therapist therapist)
        {
            return new TherapistListItemDTO
            {
                TherapistId = therapist.TherapistId,
                FirstName = therapist.FirstName,
                LastName = therapist.LastName,
                FullName = $"{therapist.FirstName} {therapist.LastName}",
                Title = therapist.Title,
                ImageUrl = therapist.ImageUrl,
                IsActive = therapist.IsActive
            };
        }

        #endregion
    }
} 
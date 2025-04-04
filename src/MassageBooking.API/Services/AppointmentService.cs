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
    /// Service for handling appointment-related business logic
    /// </summary>
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ITherapistRepository _therapistRepository;
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IServiceRepository serviceRepository,
            ITherapistRepository therapistRepository,
            IClientRepository clientRepository,
            ILogger<AppointmentService> logger)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _therapistRepository = therapistRepository ?? throw new ArgumentNullException(nameof(therapistRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<AppointmentDetailsDTO> GetAppointmentByIdAsync(Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                return null;
            }

            return MapToAppointmentDetailsDTO(appointment);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AppointmentListItemDTO>> GetClientAppointmentsAsync(Guid clientId)
        {
            var appointments = await _appointmentRepository.GetByClientIdAsync(clientId);
            return appointments.Select(MapToAppointmentListItemDTO);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AppointmentListItemDTO>> GetTherapistScheduleAsync(Guid therapistId, DateTime startDate, DateTime endDate)
        {
            var appointments = await _appointmentRepository.GetByTherapistIdAsync(therapistId, startDate, endDate);
            return appointments.Select(MapToAppointmentListItemDTO);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AvailableSlotDTO>> FindAvailableSlotsAsync(
            Guid serviceId, 
            Guid? therapistId, 
            DateTime startDate, 
            DateTime endDate)
        {
            var availableSlots = await _appointmentRepository.FindAvailableSlotsAsync(
                serviceId, therapistId, startDate, endDate);
            
            return availableSlots.Select(MapToAvailableSlotDTO);
        }

        /// <inheritdoc />
        public async Task<BookingResultDTO> BookAppointmentAsync(AppointmentBookingDTO bookingRequest)
        {
            try
            {
                // Validate that the service exists
                var service = await _serviceRepository.GetByIdAsync(bookingRequest.ServiceId);
                if (service == null)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The selected service does not exist."
                    };
                }

                // Validate that the therapist exists and offers this service
                var therapist = await _therapistRepository.GetByIdAsync(bookingRequest.TherapistId);
                if (therapist == null)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The selected therapist does not exist."
                    };
                }

                var therapistServices = await _therapistRepository.GetTherapistServicesAsync(bookingRequest.TherapistId);
                if (!therapistServices.Any(s => s.ServiceId == bookingRequest.ServiceId))
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The selected therapist does not offer this service."
                    };
                }

                // Validate that the client exists
                var client = await _clientRepository.GetByIdAsync(bookingRequest.ClientId);
                if (client == null)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The client does not exist."
                    };
                }

                // Calculate end time based on service duration
                var appointmentEndTime = bookingRequest.StartTime.AddMinutes(service.Duration);

                // Check for scheduling conflicts
                var hasConflict = await _appointmentRepository.HasSchedulingConflictAsync(
                    bookingRequest.TherapistId,
                    bookingRequest.StartTime,
                    appointmentEndTime);

                if (hasConflict)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The selected time slot is no longer available."
                    };
                }

                // Create new appointment
                var appointment = new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    ClientId = bookingRequest.ClientId,
                    TherapistId = bookingRequest.TherapistId,
                    ServiceId = bookingRequest.ServiceId,
                    StartTime = bookingRequest.StartTime,
                    EndTime = appointmentEndTime,
                    Status = "Scheduled",
                    Notes = bookingRequest.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _appointmentRepository.AddAsync(appointment);

                _logger.LogInformation("Appointment booked successfully. AppointmentId: {AppointmentId}, ClientId: {ClientId}, TherapistId: {TherapistId}",
                    appointment.AppointmentId, appointment.ClientId, appointment.TherapistId);

                return new BookingResultDTO
                {
                    Success = true,
                    AppointmentId = appointment.AppointmentId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking appointment: {@BookingRequest}", bookingRequest);
                return new BookingResultDTO
                {
                    Success = false,
                    ErrorMessage = "An error occurred while booking the appointment. Please try again later."
                };
            }
        }

        /// <inheritdoc />
        public async Task<BookingResultDTO> RescheduleAppointmentAsync(AppointmentRescheduleDTO rescheduleRequest)
        {
            try
            {
                // Get the appointment
                var appointment = await _appointmentRepository.GetByIdAsync(rescheduleRequest.AppointmentId);
                if (appointment == null)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "Appointment not found."
                    };
                }

                // Calculate the duration
                var duration = (appointment.EndTime - appointment.StartTime).TotalMinutes;
                var newEndTime = rescheduleRequest.NewStartTime.AddMinutes(duration);

                // Check for scheduling conflicts
                var hasConflict = await _appointmentRepository.HasSchedulingConflictAsync(
                    appointment.TherapistId,
                    rescheduleRequest.NewStartTime,
                    newEndTime,
                    appointment.AppointmentId); // Exclude the current appointment

                if (hasConflict)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The selected time slot is not available."
                    };
                }

                // Update appointment
                appointment.StartTime = rescheduleRequest.NewStartTime;
                appointment.EndTime = newEndTime;
                appointment.Status = "Rescheduled";
                appointment.UpdatedAt = DateTime.UtcNow;

                await _appointmentRepository.UpdateAsync(appointment);

                _logger.LogInformation("Appointment rescheduled successfully. AppointmentId: {AppointmentId}, NewStartTime: {NewStartTime}",
                    appointment.AppointmentId, appointment.StartTime);

                return new BookingResultDTO
                {
                    Success = true,
                    AppointmentId = appointment.AppointmentId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling appointment: {@RescheduleRequest}", rescheduleRequest);
                return new BookingResultDTO
                {
                    Success = false,
                    ErrorMessage = "An error occurred while rescheduling the appointment. Please try again later."
                };
            }
        }

        /// <inheritdoc />
        public async Task<BookingResultDTO> CancelAppointmentAsync(Guid appointmentId)
        {
            try
            {
                var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "Appointment not found."
                    };
                }

                appointment.Status = "Cancelled";
                appointment.UpdatedAt = DateTime.UtcNow;

                await _appointmentRepository.UpdateAsync(appointment);

                _logger.LogInformation("Appointment cancelled successfully. AppointmentId: {AppointmentId}", appointmentId);

                return new BookingResultDTO
                {
                    Success = true,
                    AppointmentId = appointmentId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment: {AppointmentId}", appointmentId);
                return new BookingResultDTO
                {
                    Success = false,
                    ErrorMessage = "An error occurred while cancelling the appointment. Please try again later."
                };
            }
        }

        /// <inheritdoc />
        public async Task<BookingResultDTO> RebookPreviousAppointmentAsync(RebookRequestDTO rebookRequest)
        {
            try
            {
                // Get previous appointment details
                var previousAppointment = await _appointmentRepository.GetByIdAsync(rebookRequest.PreviousAppointmentId);
                if (previousAppointment == null)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "Previous appointment not found."
                    };
                }

                // Calculate end time
                var endTime = rebookRequest.StartTime.AddMinutes(
                    (previousAppointment.EndTime - previousAppointment.StartTime).TotalMinutes);

                // Check for scheduling conflicts
                var hasConflict = await _appointmentRepository.HasSchedulingConflictAsync(
                    previousAppointment.TherapistId,
                    rebookRequest.StartTime,
                    endTime);

                if (hasConflict)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The selected time slot is not available."
                    };
                }

                // Create new appointment with details from previous one
                var appointment = new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    ClientId = previousAppointment.ClientId,
                    TherapistId = previousAppointment.TherapistId,
                    ServiceId = previousAppointment.ServiceId,
                    StartTime = rebookRequest.StartTime,
                    EndTime = endTime,
                    Status = "Scheduled",
                    Notes = rebookRequest.Notes ?? previousAppointment.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _appointmentRepository.AddAsync(appointment);

                _logger.LogInformation("Appointment rebooked successfully. New AppointmentId: {AppointmentId}, Previous AppointmentId: {PreviousAppointmentId}",
                    appointment.AppointmentId, rebookRequest.PreviousAppointmentId);

                return new BookingResultDTO
                {
                    Success = true,
                    AppointmentId = appointment.AppointmentId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebooking appointment: {@RebookRequest}", rebookRequest);
                return new BookingResultDTO
                {
                    Success = false,
                    ErrorMessage = "An error occurred while rebooking the appointment. Please try again later."
                };
            }
        }

        #region Mapping Methods

        private AppointmentDetailsDTO MapToAppointmentDetailsDTO(Appointment appointment)
        {
            return new AppointmentDetailsDTO
            {
                AppointmentId = appointment.AppointmentId,
                ClientId = appointment.ClientId,
                ClientName = $"{appointment.Client.FirstName} {appointment.Client.LastName}",
                TherapistId = appointment.TherapistId,
                TherapistName = $"{appointment.Therapist.FirstName} {appointment.Therapist.LastName}",
                ServiceId = appointment.ServiceId,
                ServiceName = appointment.Service.Name,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Duration = (int)(appointment.EndTime - appointment.StartTime).TotalMinutes,
                Status = appointment.Status,
                Notes = appointment.Notes,
                Price = appointment.Service.Price,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };
        }

        private AppointmentListItemDTO MapToAppointmentListItemDTO(Appointment appointment)
        {
            return new AppointmentListItemDTO
            {
                AppointmentId = appointment.AppointmentId,
                TherapistName = $"{appointment.Therapist.FirstName} {appointment.Therapist.LastName}",
                ServiceName = appointment.Service.Name,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status
            };
        }

        private AvailableSlotDTO MapToAvailableSlotDTO(AvailableSlot slot)
        {
            return new AvailableSlotDTO
            {
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                TherapistId = slot.TherapistId,
                TherapistName = slot.TherapistName,
                Duration = (int)(slot.EndTime - slot.StartTime).TotalMinutes
            };
        }

        #endregion
    }
} 
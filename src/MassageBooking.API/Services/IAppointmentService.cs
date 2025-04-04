using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.DTOs;

namespace MassageBooking.API.Services
{
    /// <summary>
    /// Service interface for appointment management
    /// </summary>
    public interface IAppointmentService
    {
        /// <summary>
        /// Gets appointment details by ID
        /// </summary>
        /// <param name="appointmentId">The appointment ID</param>
        /// <returns>Appointment details or null if not found</returns>
        Task<AppointmentDetailsDTO> GetAppointmentByIdAsync(Guid appointmentId);
        
        /// <summary>
        /// Gets all appointments for a client
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <returns>Collection of client's appointments</returns>
        Task<IEnumerable<AppointmentListItemDTO>> GetClientAppointmentsAsync(Guid clientId);
        
        /// <summary>
        /// Gets a therapist's schedule for a specific date range
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="startDate">The start date of the range</param>
        /// <param name="endDate">The end date of the range</param>
        /// <returns>Collection of appointments in the therapist's schedule</returns>
        Task<IEnumerable<AppointmentListItemDTO>> GetTherapistScheduleAsync(Guid therapistId, DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Finds available appointment slots
        /// </summary>
        /// <param name="serviceId">The service ID</param>
        /// <param name="therapistId">Optional therapist ID to filter by specific therapist</param>
        /// <param name="startDate">The start date to search from</param>
        /// <param name="endDate">The end date to search to</param>
        /// <returns>Collection of available time slots</returns>
        Task<IEnumerable<AvailableSlotDTO>> FindAvailableSlotsAsync(
            Guid serviceId, 
            Guid? therapistId, 
            DateTime startDate, 
            DateTime endDate);
        
        /// <summary>
        /// Books a new appointment
        /// </summary>
        /// <param name="bookingRequest">The booking request details</param>
        /// <returns>Booking result with success status and error message if applicable</returns>
        Task<BookingResultDTO> BookAppointmentAsync(AppointmentBookingDTO bookingRequest);
        
        /// <summary>
        /// Reschedules an existing appointment
        /// </summary>
        /// <param name="rescheduleRequest">The reschedule request details</param>
        /// <returns>Booking result with success status and error message if applicable</returns>
        Task<BookingResultDTO> RescheduleAppointmentAsync(AppointmentRescheduleDTO rescheduleRequest);
        
        /// <summary>
        /// Cancels an appointment
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment to cancel</param>
        /// <returns>Booking result with success status and error message if applicable</returns>
        Task<BookingResultDTO> CancelAppointmentAsync(Guid appointmentId);
        
        /// <summary>
        /// Rebooks a previous appointment with the same service and therapist but a new date/time
        /// </summary>
        /// <param name="rebookRequest">The rebook request details</param>
        /// <returns>Booking result with success status and error message if applicable</returns>
        Task<BookingResultDTO> RebookPreviousAppointmentAsync(RebookRequestDTO rebookRequest);
    }
} 
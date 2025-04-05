using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.DTOs;
using MassageBooking.API.Models;

namespace MassageBooking.API.Services
{
    /// <summary>
    /// Service interface for managing appointments.
    /// </summary>
    public interface IAppointmentService
    {
        /// <summary>
        /// Gets a specific appointment by its ID.
        /// </summary>
        Task<AppointmentDetailsDTO?> GetAppointmentByIdAsync(Guid id);

        /// <summary>
        /// Gets all appointments for a specific client.
        /// </summary>
        Task<IEnumerable<AppointmentListItemDTO>> GetAppointmentsByClientIdAsync(Guid clientId);

        /// <summary>
        /// Gets all appointments for a specific therapist.
        /// </summary>
        Task<IEnumerable<AppointmentListItemDTO>> GetAppointmentsByTherapistIdAsync(Guid therapistId);

        /// <summary>
        /// Gets a therapist's appointments within a specific date range.
        /// </summary>
        Task<IEnumerable<AppointmentListItemDTO>> GetTherapistScheduleAsync(Guid therapistId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Finds available appointment slots.
        /// </summary>
        Task<IEnumerable<AvailableSlotDTO>> FindAvailableSlotsAsync(Guid serviceId, Guid? therapistId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Books a new appointment.
        /// </summary>
        Task<BookingResultDTO> BookAppointmentAsync(AppointmentBookingDTO bookingRequest);

        /// <summary>
        /// Reschedules an existing appointment.
        /// </summary>
        Task<BookingResultDTO> RescheduleAppointmentAsync(AppointmentRescheduleDTO rescheduleRequest);

        /// <summary>
        /// Cancels an appointment.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment to cancel.</param>
        /// <param name="reason">Optional reason for cancellation.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> CancelAppointmentAsync(Guid appointmentId, string? reason);

        /// <summary>
        /// Deletes an appointment permanently.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment to delete.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> DeleteAppointmentAsync(Guid appointmentId);
        
        /// <summary>
        /// Gets appointments within a specific date range.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>A collection of appointments within the range.</returns>
        Task<IEnumerable<AppointmentDetailsDTO>> GetAppointmentsInRangeAsync(DateTime startDate, DateTime endDate);
        
        // Add other methods like Complete, MarkNoShow if needed
        // Task<bool> CompleteAppointmentAsync(Guid appointmentId);
        // Task<bool> MarkNoShowAsync(Guid appointmentId);
    }
} 
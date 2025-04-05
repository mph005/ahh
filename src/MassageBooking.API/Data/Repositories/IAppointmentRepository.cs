using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.Models;

namespace MassageBooking.API.Data.Repositories
{
    /// <summary>
    /// Repository interface for appointment data access operations
    /// </summary>
    public interface IAppointmentRepository
    {
        /// <summary>
        /// Gets an appointment by its ID with related entities
        /// </summary>
        /// <param name="id">The appointment ID</param>
        /// <returns>The appointment with related entities or null if not found</returns>
        Task<Appointment?> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Gets all appointments for a specific client
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <returns>Collection of appointments</returns>
        Task<IEnumerable<Appointment>> GetByClientIdAsync(Guid clientId);
        
        /// <summary>
        /// Gets a therapist's schedule for a specific date range
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="startDate">The start date of the range</param>
        /// <param name="endDate">The end date of the range</param>
        /// <returns>Collection of appointments in the specified date range</returns>
        Task<IEnumerable<Appointment>> GetByTherapistIdAsync(Guid therapistId);
        
        /// <summary>
        /// Gets a therapist's schedule for a specific date range
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="startDate">The start date of the range</param>
        /// <param name="endDate">The end date of the range</param>
        /// <returns>Collection of appointments in the specified date range</returns>
        Task<IEnumerable<Appointment>> GetByTherapistIdAsync(Guid therapistId, DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Gets all appointments within a date range
        /// </summary>
        /// <param name="startDate">The start date of the range</param>
        /// <param name="endDate">The end date of the range</param>
        /// <returns>Collection of appointments in the specified date range</returns>
        Task<IEnumerable<Appointment>> GetAppointmentsInRangeAsync(DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Finds available time slots based on therapist availability and existing appointments
        /// </summary>
        /// <param name="serviceId">The service ID</param>
        /// <param name="therapistId">Optional therapist ID to filter by specific therapist</param>
        /// <param name="startDate">The start date to search from</param>
        /// <param name="endDate">The end date to search to</param>
        /// <returns>Collection of available time slots</returns>
        Task<IEnumerable<AvailableSlot>> FindAvailableSlotsAsync(Guid serviceId, Guid? therapistId, DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Checks if there is a scheduling conflict for a therapist at a specific time
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="startTime">The appointment start time</param>
        /// <param name="endTime">The appointment end time</param>
        /// <param name="excludeAppointmentId">Optional appointment ID to exclude from conflict check</param>
        /// <returns>True if there is a conflict, false otherwise</returns>
        Task<bool> HasSchedulingConflictAsync(Guid therapistId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null);
        
        /// <summary>
        /// Adds a new appointment
        /// </summary>
        /// <param name="appointment">The appointment to add</param>
        Task AddAsync(Appointment appointment);
        
        /// <summary>
        /// Updates an existing appointment
        /// </summary>
        /// <param name="appointment">The appointment to update</param>
        Task UpdateAsync(Appointment appointment);
        
        /// <summary>
        /// Deletes an appointment by ID
        /// </summary>
        /// <param name="id">The ID of the appointment to delete</param>
        Task DeleteAsync(Guid id);
        
        /// <summary>
        /// Gets all appointments (use with caution for performance).
        /// </summary>
        /// <returns>A collection of all appointments.</returns>
        Task<IEnumerable<Appointment>> GetAllAsync();
    }

    public class TimeSlot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TherapistId { get; set; }
        public string TherapistName { get; set; } = string.Empty;
    }
} 
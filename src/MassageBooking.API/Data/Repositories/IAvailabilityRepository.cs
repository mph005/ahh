using System;
using System.Threading.Tasks;
using MassageBooking.API.Models;

namespace MassageBooking.API.Data.Repositories
{
    /// <summary>
    /// Repository interface for managing therapist availability
    /// </summary>
    public interface IAvailabilityRepository
    {
        /// <summary>
        /// Gets availability for a specific date
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="date">The specific date</param>
        /// <returns>Availability for the specified date or null if not found</returns>
        Task<Availability> GetAvailabilityForDateAsync(Guid therapistId, DateTime date);
        
        /// <summary>
        /// Gets availability for a day of week
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="dayOfWeek">The day of week</param>
        /// <returns>Availability for the specified day of week or null if not found</returns>
        Task<Availability> GetAvailabilityForDayOfWeekAsync(Guid therapistId, DayOfWeek dayOfWeek);
        
        /// <summary>
        /// Creates or updates availability for a specific date
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="date">The specific date</param>
        /// <param name="isAvailable">Whether the therapist is available</param>
        /// <param name="startTime">Start time of availability</param>
        /// <param name="endTime">End time of availability</param>
        /// <param name="breakStartTime">Break start time</param>
        /// <param name="breakEndTime">Break end time</param>
        /// <param name="notes">Additional notes</param>
        /// <returns>The created or updated availability</returns>
        Task<Availability> UpsertDateAvailabilityAsync(
            Guid therapistId,
            DateTime date,
            bool isAvailable,
            TimeSpan? startTime,
            TimeSpan? endTime,
            TimeSpan? breakStartTime,
            TimeSpan? breakEndTime,
            string notes);
        
        /// <summary>
        /// Creates or updates availability for a day of week
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="dayOfWeek">The day of week</param>
        /// <param name="isAvailable">Whether the therapist is available</param>
        /// <param name="startTime">Start time of availability</param>
        /// <param name="endTime">End time of availability</param>
        /// <param name="breakStartTime">Break start time</param>
        /// <param name="breakEndTime">Break end time</param>
        /// <param name="notes">Additional notes</param>
        /// <returns>The created or updated availability</returns>
        Task<Availability> UpsertDayOfWeekAvailabilityAsync(
            Guid therapistId,
            DayOfWeek dayOfWeek,
            bool isAvailable,
            TimeSpan? startTime,
            TimeSpan? endTime,
            TimeSpan? breakStartTime,
            TimeSpan? breakEndTime,
            string notes);
        
        /// <summary>
        /// Deletes availability for a specific date
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="date">The specific date</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteDateAvailabilityAsync(Guid therapistId, DateTime date);
        
        /// <summary>
        /// Deletes availability for a day of week
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="dayOfWeek">The day of week</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteDayOfWeekAvailabilityAsync(Guid therapistId, DayOfWeek dayOfWeek);
    }
} 
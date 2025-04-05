using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.Models;

namespace MassageBooking.API.Data.Repositories
{
    /// <summary>
    /// Repository interface for availability data access operations
    /// </summary>
    public interface IAvailabilityRepository
    {
        /// <summary>
        /// Gets availability by its ID.
        /// </summary>
        Task<Availability?> GetByIdAsync(Guid availabilityId);

        /// <summary>
        /// Gets availability for a specific therapist within a date range.
        /// </summary>
        Task<IEnumerable<Availability>> GetAvailabilityInRangeAsync(Guid therapistId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets all availability entries for a specific therapist.
        /// </summary>
        Task<IEnumerable<Availability>> GetTherapistAvailabilityAsync(Guid therapistId);

        /// <summary>
        /// Adds a new availability entry.
        /// </summary>
        Task AddAsync(Availability availability);

        /// <summary>
        /// Updates an existing availability entry.
        /// </summary>
        Task UpdateAsync(Availability availability);

        /// <summary>
        /// Deletes an availability entry by its ID.
        /// </summary>
        Task DeleteAsync(Guid availabilityId);

        /// <summary>
        /// Deletes multiple availability entries by their IDs.
        /// </summary>
        Task DeleteRangeAsync(IEnumerable<Guid> availabilityIds);

        /// <summary>
        /// Deletes all availability entries for a specific therapist.
        /// </summary>
        /// <returns>True if entries were found and deleted, false otherwise.</returns>
        Task<bool> DeleteByTherapistIdAsync(Guid therapistId);
        
        /// <summary>
        /// Gets availability for a specific date
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="date">The specific date</param>
        /// <returns>Availability for the specified date or null if not found</returns>
        Task<Availability?> GetAvailabilityForDateAsync(Guid therapistId, DateTime date);
        
        /// <summary>
        /// Gets availability for a day of week
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="dayOfWeek">The day of week</param>
        /// <returns>Availability for the specified day of week or null if not found</returns>
        Task<Availability?> GetAvailabilityForDayOfWeekAsync(Guid therapistId, DayOfWeek dayOfWeek);
        
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
            string? notes);
        
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
            string? notes);
        
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
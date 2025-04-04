using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MassageBooking.API.Models;

namespace MassageBooking.API.Data.Repositories
{
    /// <summary>
    /// Repository for managing therapist availability
    /// </summary>
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AvailabilityRepository> _logger;

        public AvailabilityRepository(
            ApplicationDbContext context,
            ILogger<AvailabilityRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<Availability> GetAvailabilityForDateAsync(Guid therapistId, DateTime date)
        {
            try
            {
                var dateOnly = date.Date; // Get just the date part
                
                return await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.TherapistId == therapistId && 
                                            a.SpecificDate.HasValue && 
                                            a.SpecificDate.Value == dateOnly);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving availability for therapist {TherapistId} on date {Date}", 
                    therapistId, date);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Availability> GetAvailabilityForDayOfWeekAsync(Guid therapistId, DayOfWeek dayOfWeek)
        {
            try
            {
                return await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.TherapistId == therapistId && 
                                            a.DayOfWeek.HasValue && 
                                            a.DayOfWeek.Value == dayOfWeek);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving availability for therapist {TherapistId} on day {DayOfWeek}", 
                    therapistId, dayOfWeek);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Availability> UpsertDateAvailabilityAsync(
            Guid therapistId,
            DateTime date,
            bool isAvailable,
            TimeSpan? startTime,
            TimeSpan? endTime,
            TimeSpan? breakStartTime,
            TimeSpan? breakEndTime,
            string notes)
        {
            try
            {
                var dateOnly = date.Date; // Get just the date part
                
                // Check if there's an existing record
                var availability = await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.TherapistId == therapistId && 
                                            a.SpecificDate.HasValue && 
                                            a.SpecificDate.Value == dateOnly);
                
                if (availability == null)
                {
                    // Create a new record
                    availability = new Availability
                    {
                        AvailabilityId = Guid.NewGuid(),
                        TherapistId = therapistId,
                        SpecificDate = dateOnly,
                        DayOfWeek = null, // This is a specific date availability
                        IsAvailable = isAvailable,
                        StartTime = startTime,
                        EndTime = endTime,
                        BreakStartTime = breakStartTime,
                        BreakEndTime = breakEndTime,
                        Notes = notes,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    await _context.Availabilities.AddAsync(availability);
                }
                else
                {
                    // Update existing record
                    availability.IsAvailable = isAvailable;
                    availability.StartTime = startTime;
                    availability.EndTime = endTime;
                    availability.BreakStartTime = breakStartTime;
                    availability.BreakEndTime = breakEndTime;
                    availability.Notes = notes;
                    availability.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Availabilities.Update(availability);
                }
                
                await _context.SaveChangesAsync();
                return availability;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting date availability for therapist {TherapistId} on date {Date}", 
                    therapistId, date);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Availability> UpsertDayOfWeekAvailabilityAsync(
            Guid therapistId,
            DayOfWeek dayOfWeek,
            bool isAvailable,
            TimeSpan? startTime,
            TimeSpan? endTime,
            TimeSpan? breakStartTime,
            TimeSpan? breakEndTime,
            string notes)
        {
            try
            {
                // Check if there's an existing record
                var availability = await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.TherapistId == therapistId && 
                                            a.DayOfWeek.HasValue && 
                                            a.DayOfWeek.Value == dayOfWeek);
                
                if (availability == null)
                {
                    // Create a new record
                    availability = new Availability
                    {
                        AvailabilityId = Guid.NewGuid(),
                        TherapistId = therapistId,
                        SpecificDate = null, // This is a day of week availability
                        DayOfWeek = dayOfWeek,
                        IsAvailable = isAvailable,
                        StartTime = startTime,
                        EndTime = endTime,
                        BreakStartTime = breakStartTime,
                        BreakEndTime = breakEndTime,
                        Notes = notes,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    await _context.Availabilities.AddAsync(availability);
                }
                else
                {
                    // Update existing record
                    availability.IsAvailable = isAvailable;
                    availability.StartTime = startTime;
                    availability.EndTime = endTime;
                    availability.BreakStartTime = breakStartTime;
                    availability.BreakEndTime = breakEndTime;
                    availability.Notes = notes;
                    availability.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Availabilities.Update(availability);
                }
                
                await _context.SaveChangesAsync();
                return availability;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting day of week availability for therapist {TherapistId} on day {DayOfWeek}", 
                    therapistId, dayOfWeek);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDateAvailabilityAsync(Guid therapistId, DateTime date)
        {
            try
            {
                var dateOnly = date.Date; // Get just the date part
                
                var availability = await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.TherapistId == therapistId && 
                                            a.SpecificDate.HasValue && 
                                            a.SpecificDate.Value == dateOnly);
                
                if (availability == null)
                {
                    return false;
                }
                
                _context.Availabilities.Remove(availability);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting date availability for therapist {TherapistId} on date {Date}", 
                    therapistId, date);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDayOfWeekAvailabilityAsync(Guid therapistId, DayOfWeek dayOfWeek)
        {
            try
            {
                var availability = await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.TherapistId == therapistId && 
                                            a.DayOfWeek.HasValue && 
                                            a.DayOfWeek.Value == dayOfWeek);
                
                if (availability == null)
                {
                    return false;
                }
                
                _context.Availabilities.Remove(availability);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting day of week availability for therapist {TherapistId} on day {DayOfWeek}", 
                    therapistId, dayOfWeek);
                throw;
            }
        }
    }
} 
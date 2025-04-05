using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MassageBooking.API.Models;
using System.Linq;
using MassageBooking.API.Data;

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
        public async Task<Availability?> GetAvailabilityForDateAsync(Guid therapistId, DateTime date)
        {
            try
            {
                return await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.TherapistId == therapistId && 
                                            a.DateOverride.HasValue &&
                                            a.DateOverride.Value.Date == date.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving specific date availability for Therapist {TherapistId} on {Date}", therapistId, date.Date);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Availability?> GetAvailabilityForDayOfWeekAsync(Guid therapistId, DayOfWeek dayOfWeek)
        {
            try
            {
                return await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.TherapistId == therapistId && 
                                            a.DayOfWeek.HasValue && 
                                            a.DayOfWeek.Value == dayOfWeek &&
                                            !a.DateOverride.HasValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving day of week availability for Therapist {TherapistId} on {DayOfWeek}", therapistId, dayOfWeek);
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
            string? notes)
        {
            try
            {
                var availability = await GetAvailabilityForDateAsync(therapistId, date);
                
                if (availability == null)
                {
                    availability = new Availability
                    {
                        TherapistId = therapistId,
                        DateOverride = date.Date,
                        DayOfWeek = null,
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
                    _logger.LogInformation("Created new specific date availability for Therapist {TherapistId} on {Date}", therapistId, date.Date);
                }
                else
                {
                    availability.IsAvailable = isAvailable;
                    availability.StartTime = startTime;
                    availability.EndTime = endTime;
                    availability.BreakStartTime = breakStartTime;
                    availability.BreakEndTime = breakEndTime;
                    availability.Notes = notes;
                    availability.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Availabilities.Update(availability);
                    _logger.LogInformation("Updated specific date availability for Therapist {TherapistId} on {Date}", therapistId, date.Date);
                }
                
                await _context.SaveChangesAsync();
                return availability;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting specific date availability for Therapist {TherapistId} on {Date}", therapistId, date.Date);
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
            string? notes)
        {
            try
            {
                var availability = await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.TherapistId == therapistId && 
                                            a.DayOfWeek.HasValue && 
                                            a.DayOfWeek.Value == dayOfWeek &&
                                            !a.DateOverride.HasValue);

                if (availability == null)
                {
                    availability = new Availability
                    {
                        TherapistId = therapistId,
                        DayOfWeek = dayOfWeek,
                        DateOverride = null,
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
                    _logger.LogInformation("Created new recurring availability for Therapist {TherapistId} on {DayOfWeek}", therapistId, dayOfWeek);
                }
                else
                {
                    availability.IsAvailable = isAvailable;
                    availability.StartTime = startTime;
                    availability.EndTime = endTime;
                    availability.BreakStartTime = breakStartTime;
                    availability.BreakEndTime = breakEndTime;
                    availability.Notes = notes;
                    availability.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Availabilities.Update(availability);
                    _logger.LogInformation("Updated recurring availability for Therapist {TherapistId} on {DayOfWeek}", therapistId, dayOfWeek);
                }
                
                await _context.SaveChangesAsync();
                return availability;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting recurring availability for Therapist {TherapistId} on {DayOfWeek}", therapistId, dayOfWeek);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDateAvailabilityAsync(Guid therapistId, DateTime date)
        {
            try
            {
                var availability = await GetAvailabilityForDateAsync(therapistId, date);
                
                if (availability == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent specific date availability for Therapist {TherapistId} on {Date}", therapistId, date.Date);
                    return false;
                }
                
                _context.Availabilities.Remove(availability);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted specific date availability for Therapist {TherapistId} on {Date}", therapistId, date.Date);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting specific date availability for Therapist {TherapistId} on {Date}", therapistId, date.Date);
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
                                            a.DayOfWeek.Value == dayOfWeek &&
                                            !a.DateOverride.HasValue);
                                            
                if (availability == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent recurring availability for Therapist {TherapistId} on {DayOfWeek}", therapistId, dayOfWeek);
                    return false;
                }
                
                _context.Availabilities.Remove(availability);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted recurring availability for Therapist {TherapistId} on {DayOfWeek}", therapistId, dayOfWeek);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recurring availability for Therapist {TherapistId} on {DayOfWeek}", therapistId, dayOfWeek);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Availability> GetByIdAsync(Guid availabilityId)
        {
            try
            {
                return await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.AvailabilityId == availabilityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving availability with ID {AvailabilityId}", availabilityId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Availability>> GetTherapistAvailabilityAsync(Guid therapistId)
        {
            try
            {
                return await _context.Availabilities
                    .Where(a => a.TherapistId == therapistId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all availability for Therapist {TherapistId}", therapistId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task AddAsync(Availability availability)
        {
            _logger.LogInformation("Adding new availability for Therapist {TherapistId} from {StartTime} to {EndTime}", 
                availability.TherapistId, availability.StartTime, availability.EndTime);
            if (availability == null)
            {
                throw new ArgumentNullException(nameof(availability));
            }
            availability.AvailabilityId = Guid.NewGuid(); // Ensure new ID
            try
            {
                await _context.Availabilities.AddAsync(availability);
                await _context.SaveChangesAsync();
                 _logger.LogInformation("Successfully added availability ID: {AvailabilityId}", availability.AvailabilityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding availability for Therapist {TherapistId}", availability.TherapistId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid availabilityId)
        {
             _logger.LogInformation("Deleting availability ID: {AvailabilityId}", availabilityId);
             var availability = await _context.Availabilities.FindAsync(availabilityId);
             if (availability != null)
             {
                 _context.Availabilities.Remove(availability);
                 await _context.SaveChangesAsync();
                  _logger.LogInformation("Successfully deleted availability ID: {AvailabilityId}", availabilityId);
             }
             else
             {
                 _logger.LogWarning("Attempted to delete non-existent availability ID: {AvailabilityId}", availabilityId);
             }
        }

        /// <inheritdoc />
        public async Task DeleteRangeAsync(IEnumerable<Guid> availabilityIds)
        {
             var idsList = availabilityIds?.ToList() ?? new List<Guid>();
             _logger.LogInformation("Deleting {Count} availability entries by ID range.", idsList.Count);
             if (!idsList.Any())
             {
                 return; // Nothing to delete
             }
             
             try
             {
                 var availabilitiesToDelete = await _context.Availabilities
                                                        .Where(a => idsList.Contains(a.AvailabilityId))
                                                        .ToListAsync();
                                                        
                 if(availabilitiesToDelete.Any())
                 {
                     _context.Availabilities.RemoveRange(availabilitiesToDelete);
                     await _context.SaveChangesAsync();
                     _logger.LogInformation("Successfully deleted {Count} availability entries.", availabilitiesToDelete.Count);
                 }
                 else
                 {
                     _logger.LogWarning("No availability entries found matching the provided IDs for deletion.");
                 }
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error deleting availability range.");
                 throw;
             }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Availability>> GetAvailabilityInRangeAsync(Guid therapistId, DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Getting availability for {TherapistId} in range {StartDate} - {EndDate}", therapistId, startDate, endDate);
            // Filter for specific date availabilities within the range
            return await _context.Availabilities
                 .Where(a => a.TherapistId == therapistId && 
                             a.DateOverride.HasValue && // Ensure there's a date
                             a.StartTime.HasValue &&    // Ensure there's a start time
                             a.EndTime.HasValue &&      // Ensure there's an end time
                             (a.DateOverride.Value.Date + a.StartTime.Value) < endDate && // Construct full DateTime for start comparison
                             (a.DateOverride.Value.Date + a.EndTime.Value) > startDate) // Construct full DateTime for end comparison
                 .OrderBy(a => a.StartTime)
                 .ToListAsync();
        }
        
        /// <inheritdoc />
        public async Task UpdateAsync(Availability availability)
        {
            _logger.LogInformation("Updating availability ID: {AvailabilityId}", availability.AvailabilityId);
            if (availability == null)
            {
                throw new ArgumentNullException(nameof(availability));
            }

            // Ensure the entity is tracked by the context
            var existingAvailability = await _context.Availabilities.FindAsync(availability.AvailabilityId);
            if (existingAvailability == null)
            {
                 _logger.LogWarning("Attempted to update non-existent availability ID: {AvailabilityId}", availability.AvailabilityId);
                 return; // Or throw an exception, depending on desired behavior
            }

            // Update the tracked entity's values
            _context.Entry(existingAvailability).CurrentValues.SetValues(availability);
            _context.Entry(existingAvailability).State = EntityState.Modified;
            existingAvailability.UpdatedAt = DateTime.UtcNow; // Ensure UpdatedAt is set

            try
            {
                 await _context.SaveChangesAsync();
                 _logger.LogInformation("Successfully updated availability ID: {AvailabilityId}", availability.AvailabilityId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                 _logger.LogError(ex, "Concurrency error updating availability ID: {AvailabilityId}", availability.AvailabilityId);
                 throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating availability ID: {AvailabilityId}", availability.AvailabilityId);
                throw;
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> DeleteByTherapistIdAsync(Guid therapistId)
        {
            _logger.LogInformation("Deleting all availability for Therapist ID: {TherapistId}", therapistId);
            try
            {
                var availabilitiesToDelete = await _context.Availabilities
                                                        .Where(a => a.TherapistId == therapistId)
                                                        .ToListAsync();
                                                        
                if (!availabilitiesToDelete.Any())
                {
                    _logger.LogWarning("No availability entries found for therapist {TherapistId} to delete.", therapistId);
                    return false; // Nothing to delete
                }
                
                _context.Availabilities.RemoveRange(availabilitiesToDelete);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted {Count} availability entries for therapist {TherapistId}.", availabilitiesToDelete.Count, therapistId);
                return true;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error deleting availability entries for therapist {TherapistId}", therapistId);
                 throw; // Re-throw the exception
            }
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MassageBooking.API.Models;
using MassageBooking.API.Data;

namespace MassageBooking.API.Data.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentRepository> _logger;

        public AppointmentRepository(
            ApplicationDbContext context,
            ILogger<AppointmentRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Appointment?> GetByIdAsync(Guid id)
        {
            return await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Therapist)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all appointments");
            // Include necessary related data for potential DTO mapping later
            return await _context.Appointments
                                 .Include(a => a.Client)
                                 .Include(a => a.Therapist)
                                 .Include(a => a.Service)
                                 .OrderBy(a => a.StartTime) // Example ordering
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByClientIdAsync(Guid clientId)
        {
            return await _context.Appointments
                .Include(a => a.Therapist)
                .Include(a => a.Service)
                .Where(a => a.ClientId == clientId)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByTherapistIdAsync(Guid therapistId)
        {
            return await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Where(a => a.TherapistId == therapistId)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByTherapistIdAsync(Guid therapistId, DateTime startDate, DateTime endDate)
        {
            return await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Where(a => a.TherapistId == therapistId && 
                            a.StartTime >= startDate && 
                            a.StartTime <= endDate)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsInRangeAsync(DateTime startDate, DateTime endDate)
        {
             _logger.LogInformation("Retrieving appointments from {StartDate} to {EndDate}", startDate, endDate);
             // Ensure dates are handled correctly (e.g., inclusive/exclusive, time component)
             // This example assumes filtering on the date part only for StartTime
             var startDateOnly = startDate.Date;
             var endDateOnly = endDate.Date.AddDays(1); // Make range exclusive of the end date's time

            return await _context.Appointments
                                 .Include(a => a.Client)
                                 .Include(a => a.Therapist)
                                 .Include(a => a.Service)
                                 .Where(a => a.StartTime >= startDateOnly && a.StartTime < endDateOnly)
                                 .OrderBy(a => a.StartTime)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<AvailableSlot>> FindAvailableSlotsAsync(Guid serviceId, Guid? therapistId, DateTime startDate, DateTime endDate)
        {
            // Get the service to determine duration
            var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == serviceId);
            if (service == null)
            {
                _logger.LogWarning("Service with ID {ServiceId} not found when finding available slots.", serviceId);
                return new List<AvailableSlot>();
            }

            // Get therapists who offer this service
            var query = _context.Therapists.Where(t => t.IsActive);
            if (therapistId.HasValue)
            {
                query = query.Where(t => t.TherapistId == therapistId.Value);
            }
            else
            {
                // Filter by therapists who offer the service
                query = query.Where(t => t.Services.Any(ts => ts.ServiceId == serviceId));
            }
            
            var therapists = await query.ToListAsync();

            if (!therapists.Any())
            {
                _logger.LogInformation("No active therapists found offering service ID {ServiceId} (Therapist requested: {TherapistId})", 
                    serviceId, therapistId.HasValue ? therapistId.Value.ToString() : "Any");
                return new List<AvailableSlot>();
            }

            var therapistIds = therapists.Select(t => t.TherapistId).ToList();

            // Get all appointments in the date range for these therapists
            var appointments = await _context.Appointments
                .Where(a => therapistIds.Contains(a.TherapistId) && 
                            a.Status != AppointmentStatus.Cancelled &&
                            a.EndTime > startDate && // Use > for EndTime
                            a.StartTime < endDate) // Use < for StartTime
                .ToListAsync();

            // Get all availability records for these therapists
            var availabilities = await _context.Availabilities
                .Where(a => therapistIds.Contains(a.TherapistId) && a.IsAvailable)
                .ToListAsync();

            var availableSlots = new List<AvailableSlot>();
            int slotIncrementMinutes = 30; // Check every 30 minutes

            // For each therapist, for each day in the range
            foreach (var therapist in therapists)
            {
                for (var day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
                {
                    // Determine availability for this day (specific date overrides day of week)
                    var dateAvailability = availabilities
                        .FirstOrDefault(a => a.TherapistId == therapist.TherapistId && 
                                            a.DateOverride.HasValue &&
                                            a.DateOverride.Value.Date == day);

                    Availability? effectiveAvailability = dateAvailability;
                    
                    if (effectiveAvailability == null)
                    {
                        // If no specific date availability, check day of week
                        effectiveAvailability = availabilities
                            .FirstOrDefault(a => a.TherapistId == therapist.TherapistId && 
                                               a.DayOfWeek.HasValue && 
                                               a.DayOfWeek.Value == day.DayOfWeek &&
                                               !a.DateOverride.HasValue);
                    }

                    // Skip if therapist is marked unavailable for the specific date, or no recurring/specific entry exists
                    if (effectiveAvailability == null || !effectiveAvailability.IsAvailable || 
                        !effectiveAvailability.StartTime.HasValue || !effectiveAvailability.EndTime.HasValue)
                    {
                        continue; 
                    }
                    
                    // Calculate the therapist's working window for the day
                    var workStart = day.Add(effectiveAvailability.StartTime.Value);
                    var workEnd = day.Add(effectiveAvailability.EndTime.Value);
                    
                    // Adjust the potential slot start/end based on the query range
                    var potentialSlotStart = (workStart > startDate) ? workStart : startDate;
                    var potentialSlotEnd = (workEnd < endDate) ? workEnd : endDate;

                    // Iterate through potential time slots
                    for (var slotStart = potentialSlotStart; slotStart.AddMinutes(service.DurationMinutes) <= potentialSlotEnd; slotStart = slotStart.AddMinutes(slotIncrementMinutes))
                    {
                        var slotEnd = slotStart.AddMinutes(service.DurationMinutes);
                        
                        // Ensure the slot is fully within the working hours
                        if (slotEnd > workEnd) continue;
                        
                        // Check if slot overlaps with break time
                        bool overlapsBreak = false;
                        if (effectiveAvailability.BreakStartTime.HasValue && effectiveAvailability.BreakEndTime.HasValue)
                        {
                            var breakStart = day.Add(effectiveAvailability.BreakStartTime.Value);
                            var breakEnd = day.Add(effectiveAvailability.BreakEndTime.Value);
                            // Check for overlap: (SlotStart < BreakEnd) and (SlotEnd > BreakStart)
                            overlapsBreak = slotStart < breakEnd && slotEnd > breakStart;
                        }
                        if (overlapsBreak) continue;

                        // Check if slot conflicts with existing appointments
                        bool hasConflict = appointments.Any(a => 
                            a.TherapistId == therapist.TherapistId && 
                            slotStart < a.EndTime && // Slot starts before appointment ends
                            slotEnd > a.StartTime); // Slot ends after appointment starts
                        
                        if (hasConflict) continue;

                        // Add available slot if it hasn't been added already (possible due to increment)
                        if (!availableSlots.Any(s => s.StartTime == slotStart && s.TherapistId == therapist.TherapistId))
                        {
                             availableSlots.Add(new AvailableSlot
                             {
                                 StartTime = slotStart,
                                 EndTime = slotEnd,
                                 TherapistId = therapist.TherapistId,
                                 TherapistName = $"{therapist.FirstName} {therapist.LastName}",
                                 ServiceId = serviceId,
                                 ServiceName = service.Name,
                                 Duration = service.DurationMinutes
                             });
                        }
                    }
                }
            }

            return availableSlots.OrderBy(s => s.StartTime).ThenBy(s => s.TherapistName).ToList();
        }

        public async Task<bool> HasSchedulingConflictAsync(Guid therapistId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null)
        {
            var query = _context.Appointments
                .Where(a => a.TherapistId == therapistId &&
                            a.Status != AppointmentStatus.Cancelled &&
                            startTime < a.EndTime && 
                            endTime > a.StartTime);
            
            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.AppointmentId != excludeAppointmentId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task AddAsync(Appointment appointment)
        {
             if (appointment.AppointmentId == Guid.Empty)
            {
                appointment.AppointmentId = Guid.NewGuid();
            }
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            appointment.UpdatedAt = DateTime.UtcNow;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                 _logger.LogInformation("Deleted appointment {AppointmentId}", appointmentId);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent appointment {AppointmentId}", appointmentId);
            }
        }
    }
} 
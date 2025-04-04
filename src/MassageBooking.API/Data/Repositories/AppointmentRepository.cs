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

        public async Task<Appointment> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Appointments
                    .Include(a => a.Client)
                    .Include(a => a.Therapist)
                    .Include(a => a.Service)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment with ID {AppointmentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Appointment>> GetByClientIdAsync(Guid clientId)
        {
            try
            {
                return await _context.Appointments
                    .Include(a => a.Service)
                    .Include(a => a.Therapist)
                    .Where(a => a.ClientId == clientId)
                    .OrderByDescending(a => a.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for client {ClientId}", clientId);
                throw;
            }
        }

        public async Task<IEnumerable<Appointment>> GetByTherapistIdAsync(Guid therapistId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Appointments
                    .Include(a => a.Client)
                    .Include(a => a.Service)
                    .Where(a => a.TherapistId == therapistId && 
                               a.StartTime >= startDate && 
                               a.EndTime <= endDate)
                    .OrderBy(a => a.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule for therapist {TherapistId} between {StartDate} and {EndDate}", 
                    therapistId, startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsInRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Appointments
                    .Include(a => a.Client)
                    .Include(a => a.Therapist)
                    .Include(a => a.Service)
                    .Where(a => a.StartTime >= startDate && a.EndTime <= endDate)
                    .OrderBy(a => a.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments between {StartDate} and {EndDate}", 
                    startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<AvailableSlot>> FindAvailableSlotsAsync(
            Guid serviceId, 
            Guid? therapistId, 
            DateTime startDate, 
            DateTime endDate)
        {
            try
            {
                // Get the service to know its duration
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId);
                
                if (service == null)
                {
                    _logger.LogWarning("Service with ID {ServiceId} not found", serviceId);
                    return Enumerable.Empty<AvailableSlot>();
                }

                var serviceDuration = service.Duration;
                var slots = new List<AvailableSlot>();

                // Find therapists who offer this service
                var query = _context.Therapists
                    .Include(t => t.Services)
                    .Where(t => t.Services.Any(s => s.ServiceId == serviceId));

                // If therapistId is provided, filter to just that therapist
                if (therapistId.HasValue)
                {
                    query = query.Where(t => t.TherapistId == therapistId.Value);
                }

                var therapists = await query.ToListAsync();

                foreach (var therapist in therapists)
                {
                    // Get therapist's availability for the date range
                    var availabilities = await _context.Availabilities
                        .Where(a => a.TherapistId == therapist.TherapistId &&
                                   a.DayOfWeek >= startDate.DayOfWeek &&
                                   a.DayOfWeek <= endDate.DayOfWeek)
                        .ToListAsync();

                    // Get therapist's existing appointments
                    var appointments = await _context.Appointments
                        .Where(a => a.TherapistId == therapist.TherapistId &&
                                   a.StartTime >= startDate &&
                                   a.EndTime <= endDate &&
                                   a.Status != "Cancelled")
                        .ToListAsync();

                    // For each day in the range
                    for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                    {
                        // Find availability for this day of week
                        var dayAvailability = availabilities.FirstOrDefault(a => a.DayOfWeek == date.DayOfWeek);
                        
                        if (dayAvailability == null)
                        {
                            continue; // Therapist not available on this day
                        }

                        var startTime = new DateTime(
                            date.Year, date.Month, date.Day,
                            dayAvailability.StartTime.Hours,
                            dayAvailability.StartTime.Minutes, 0);
                            
                        var endTime = new DateTime(
                            date.Year, date.Month, date.Day,
                            dayAvailability.EndTime.Hours,
                            dayAvailability.EndTime.Minutes, 0);

                        // Generate potential slots based on service duration
                        for (var slotStart = startTime; slotStart.AddMinutes(serviceDuration) <= endTime; slotStart = slotStart.AddMinutes(30))
                        {
                            var slotEnd = slotStart.AddMinutes(serviceDuration);
                            
                            // Check if this slot conflicts with existing appointments
                            if (!HasSchedulingConflict(appointments, slotStart, slotEnd))
                            {
                                slots.Add(new AvailableSlot
                                {
                                    StartTime = slotStart,
                                    EndTime = slotEnd,
                                    TherapistId = therapist.TherapistId,
                                    TherapistName = $"{therapist.FirstName} {therapist.LastName}"
                                });
                            }
                        }
                    }
                }

                return slots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding available slots for service {ServiceId} between {StartDate} and {EndDate}",
                    serviceId, startDate, endDate);
                throw;
            }
        }

        public async Task<bool> HasSchedulingConflictAsync(Guid therapistId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null)
        {
            try
            {
                var query = _context.Appointments
                    .Where(a => a.TherapistId == therapistId &&
                               a.Status != "Cancelled" &&
                               ((a.StartTime < endTime && a.EndTime > startTime) || 
                                (a.StartTime == startTime && a.EndTime == endTime)));

                if (excludeAppointmentId.HasValue)
                {
                    query = query.Where(a => a.AppointmentId != excludeAppointmentId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking scheduling conflict for therapist {TherapistId} at {StartTime}", 
                    therapistId, startTime);
                throw;
            }
        }

        private bool HasSchedulingConflict(IEnumerable<Appointment> appointments, DateTime startTime, DateTime endTime)
        {
            return appointments.Any(a => 
                (a.StartTime < endTime && a.EndTime > startTime) || 
                (a.StartTime == startTime && a.EndTime == endTime));
        }

        public async Task AddAsync(Appointment appointment)
        {
            try
            {
                await _context.Appointments.AddAsync(appointment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding appointment {@Appointment}", appointment);
                throw;
            }
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            try
            {
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment {@Appointment}", appointment);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment != null)
                {
                    _context.Appointments.Remove(appointment);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment with ID {AppointmentId}", id);
                throw;
            }
        }
    }
} 
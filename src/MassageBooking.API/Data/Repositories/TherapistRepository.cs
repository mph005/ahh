using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MassageBooking.API.Models;
using Microsoft.Extensions.Logging;

namespace MassageBooking.API.Data.Repositories
{
    public class TherapistRepository : ITherapistRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TherapistRepository> _logger;

        public TherapistRepository(ApplicationDbContext context, ILogger<TherapistRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Therapist?> GetByIdAsync(Guid id)
        {
            return await _context.Therapists
                .Include(t => t.Services) // Include related services if needed
                    .ThenInclude(ts => ts.Service)
                .FirstOrDefaultAsync(t => t.TherapistId == id);
        }

        public async Task<IEnumerable<Therapist>> GetAllAsync()
        {
            return await _context.Therapists
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> GetTherapistServicesAsync(Guid therapistId)
        {
            return await _context.TherapistServices
                .Where(ts => ts.TherapistId == therapistId)
                .Include(ts => ts.Service)
                .Select(ts => ts.Service)
                .ToListAsync();
        }

        public async Task AddAsync(Therapist therapist)
        {
            if (therapist.TherapistId == Guid.Empty)
            {
                therapist.TherapistId = Guid.NewGuid();
            }
            
            therapist.CreatedAt = DateTime.UtcNow;
            therapist.UpdatedAt = DateTime.UtcNow;
            
            await _context.Therapists.AddAsync(therapist);
            await _context.SaveChangesAsync();
             _logger.LogInformation("Added new therapist with ID {TherapistId}", therapist.TherapistId);
        }

        public async Task UpdateAsync(Therapist therapist)
        {
            var existingTherapist = await _context.Therapists.AsNoTracking().FirstOrDefaultAsync(t => t.TherapistId == therapist.TherapistId);
            if (existingTherapist == null)
            {
                 _logger.LogWarning("Attempted to update non-existent therapist with ID {TherapistId}", therapist.TherapistId);
                 // Optionally throw an exception or handle as appropriate
                 return; 
            }

            therapist.UpdatedAt = DateTime.UtcNow;
            therapist.CreatedAt = existingTherapist.CreatedAt; // Ensure CreatedAt is not overwritten

            _context.Therapists.Update(therapist);
            await _context.SaveChangesAsync();
             _logger.LogInformation("Updated therapist with ID {TherapistId}", therapist.TherapistId);
        }

        public async Task DeleteAsync(Guid id)
        {
            var therapist = await _context.Therapists
                .FirstOrDefaultAsync(t => t.TherapistId == id);
                
            if (therapist != null)
            {
                // Consider soft delete by setting IsActive = false instead of hard delete
                // therapist.IsActive = false;
                // _context.Therapists.Update(therapist);
                _context.Therapists.Remove(therapist);
                await _context.SaveChangesAsync();
                 _logger.LogInformation("Deleted therapist with ID {Id}", id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent therapist with ID {Id}", id);
            }
        }

        public async Task<bool> AddServiceToTherapistAsync(Guid therapistId, Guid serviceId)
        {
            // Check if therapist and service exist
            bool therapistExists = await _context.Therapists.AnyAsync(t => t.TherapistId == therapistId);
            bool serviceExists = await _context.Services.AnyAsync(s => s.ServiceId == serviceId);
            
            if (!therapistExists || !serviceExists)
            {
                 _logger.LogWarning("AddServiceToTherapist failed: Therapist {TherapistId} or Service {ServiceId} not found.", therapistId, serviceId);
                return false;
            }
            
            // Check if relationship already exists
            var exists = await _context.TherapistServices
                .AnyAsync(ts => ts.TherapistId == therapistId && ts.ServiceId == serviceId);
                
            if (exists)
            {
                 _logger.LogInformation("Service {ServiceId} already associated with Therapist {TherapistId}", serviceId, therapistId);
                return true; // Already added
            }
            
            // Add the relationship
            var therapistService = new TherapistService
            {
                TherapistId = therapistId,
                ServiceId = serviceId
            };
            
            await _context.TherapistServices.AddAsync(therapistService);
            await _context.SaveChangesAsync();
             _logger.LogInformation("Associated Service {ServiceId} with Therapist {TherapistId}", serviceId, therapistId);
            return true;
        }

        public async Task<bool> RemoveServiceFromTherapistAsync(Guid therapistId, Guid serviceId)
        {
            var therapistService = await _context.TherapistServices
                .FirstOrDefaultAsync(ts => ts.TherapistId == therapistId && ts.ServiceId == serviceId);
                
            if (therapistService == null)
            {
                _logger.LogWarning("RemoveServiceFromTherapist failed: Association between Therapist {TherapistId} and Service {ServiceId} not found.", therapistId, serviceId);
                return false; // Relationship doesn't exist
            }
            
            _context.TherapistServices.Remove(therapistService);
            await _context.SaveChangesAsync();
             _logger.LogInformation("Removed association between Service {ServiceId} and Therapist {TherapistId}", serviceId, therapistId);
            return true;
        }
    }
} 
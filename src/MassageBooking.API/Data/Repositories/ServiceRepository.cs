using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MassageBooking.API.Models;
using Microsoft.Extensions.Logging;

namespace MassageBooking.API.Data.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceRepository> _logger;

        public ServiceRepository(ApplicationDbContext context, ILogger<ServiceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Service?> GetByIdAsync(Guid id)
        {
            return await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == id);
        }

        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _context.Services
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> GetActiveServicesAsync()
        {
            return await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Service service)
        {
            if (service.ServiceId == Guid.Empty)
            {
                service.ServiceId = Guid.NewGuid();
            }
            service.CreatedAt = DateTime.UtcNow;
            service.UpdatedAt = DateTime.UtcNow;
            
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
             _logger.LogInformation("Added new service with ID {ServiceId}", service.ServiceId);
        }

        public async Task UpdateAsync(Service service)
        {
            var existingService = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.ServiceId == service.ServiceId);
            if (existingService == null)
            {
                _logger.LogWarning("Attempted to update non-existent service with ID {ServiceId}", service.ServiceId);
                return; 
            }

            service.UpdatedAt = DateTime.UtcNow;
            service.CreatedAt = existingService.CreatedAt;

            _context.Services.Update(service);
            await _context.SaveChangesAsync();
             _logger.LogInformation("Updated service with ID {ServiceId}", service.ServiceId);
        }

        public async Task DeleteAsync(Guid id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == id);
            if (service != null)
            {
                 // Consider soft delete
                // service.IsActive = false;
                // _context.Services.Update(service);
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                 _logger.LogInformation("Deleted service with ID {Id}", id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent service with ID {Id}", id);
            }
        }
    }
} 
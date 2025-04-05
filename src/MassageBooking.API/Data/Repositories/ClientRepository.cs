using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MassageBooking.API.Models;
using Microsoft.Extensions.Logging;

namespace MassageBooking.API.Data.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientRepository> _logger;

        public ClientRepository(ApplicationDbContext context, ILogger<ClientRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Client?> GetByIdAsync(Guid id)
        {
            return await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == id);
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _context.Clients
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<Client?> GetByEmailAsync(string email)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task AddAsync(Client client)
        {
            if (client.ClientId == Guid.Empty)
            {
                client.ClientId = Guid.NewGuid();
            }
            client.CreatedAt = DateTime.UtcNow;
            client.UpdatedAt = DateTime.UtcNow;
            
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added new client with ID {ClientId}", client.ClientId);
        }

        public async Task UpdateAsync(Client client)
        {
             var existingClient = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.ClientId == client.ClientId);
            if (existingClient == null)
            {
                 _logger.LogWarning("Attempted to update non-existent client with ID {ClientId}", client.ClientId);
                 return; 
            }

            client.UpdatedAt = DateTime.UtcNow;
            client.CreatedAt = existingClient.CreatedAt; // Preserve original creation date
            
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated client with ID {ClientId}", client.ClientId);
        }

        public async Task DeleteAsync(Guid id)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == id);
            if (client != null)
            {
                // Consider soft delete
                // client.IsActive = false;
                // _context.Clients.Update(client);
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted client with ID {Id}", id);
            }
            else
            {
                 _logger.LogWarning("Attempted to delete non-existent client with ID {Id}", id);
            }
        }
    }
} 
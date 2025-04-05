using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassageBooking.API.Data.Repositories;
using MassageBooking.API.Models;
using Microsoft.Extensions.Logging;

namespace MassageBooking.API.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<ClientService> _logger;
        // Inject other dependencies if needed (e.g., IEmailService for welcome email)

        public ClientService(IClientRepository clientRepository, ILogger<ClientService> logger)
        {
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Client?> GetClientByIdAsync(Guid id)
        {
            _logger.LogInformation("Retrieving client by ID: {ClientId}", id);
            try
            {
                return await _clientRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client by ID: {ClientId}", id);
                throw;
            }
        }

        public async Task<Client?> GetClientByEmailAsync(string email)
        {
             _logger.LogInformation("Retrieving client by email: {Email}", email);
             try
             {
                 // Assuming repository has GetByEmailAsync
                 return await _clientRepository.GetByEmailAsync(email);
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error retrieving client by email: {Email}", email);
                 throw;
             }
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            _logger.LogInformation("Retrieving all clients");
            try
            {
                 return await _clientRepository.GetAllAsync();
            }
             catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all clients");
                throw;
            }
        }

        public async Task<IEnumerable<Client>> GetActiveClientsAsync()
        {
             _logger.LogInformation("Retrieving active clients");
             try
             {
                 // Assuming repository has GetActiveAsync or similar
                 var allClients = await _clientRepository.GetAllAsync();
                 return allClients.Where(c => c.IsActive);
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error retrieving active clients");
                 throw;
             }
        }

        public async Task<Client> CreateClientAsync(Client client)
        {
            _logger.LogInformation("Creating new client: {FirstName} {LastName}, Email: {Email}", client.FirstName, client.LastName, client.Email);
             if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            
            // Add validation? e.g., check if email exists?
            var existingClient = await _clientRepository.GetByEmailAsync(client.Email);
            if (existingClient != null)
            {
                _logger.LogWarning("Client creation failed: Email {Email} already exists.", client.Email);
                throw new InvalidOperationException("A client with this email already exists.");
            }
            
            client.ClientId = Guid.NewGuid();
            client.CreatedAt = DateTime.UtcNow;
            client.UpdatedAt = DateTime.UtcNow;
            client.IsActive = true; // Default to active
            
            try
            {
                await _clientRepository.AddAsync(client);
                 _logger.LogInformation("Successfully created client ID: {ClientId}", client.ClientId);
                 // Optionally send welcome email here
                 // await _emailService.SendWelcomeEmailAsync(client);
                return client;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error creating client: {Email}", client.Email);
                 throw;
            }
        }

        public async Task<bool> UpdateClientAsync(Client client)
        {
            _logger.LogInformation("Updating client ID: {ClientId}", client.ClientId);
             if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var existingClient = await _clientRepository.GetByIdAsync(client.ClientId);
            if (existingClient == null)
            {
                _logger.LogWarning("Update failed: Client ID {ClientId} not found.", client.ClientId);
                return false;
            }
            
            // Check for email collision if email is being changed
            if (!string.Equals(existingClient.Email, client.Email, StringComparison.OrdinalIgnoreCase))
            {
                 var emailCollision = await _clientRepository.GetByEmailAsync(client.Email);
                 if (emailCollision != null && emailCollision.ClientId != client.ClientId)
                 {
                    _logger.LogWarning("Client update failed: Email {Email} already exists for another client.", client.Email);
                    throw new InvalidOperationException("A client with this email already exists.");
                 }
            }

            // Preserve creation date, update modification date
            client.CreatedAt = existingClient.CreatedAt;
            client.UpdatedAt = DateTime.UtcNow;
            
            try
            {
                 // Use repository's UpdateAsync which likely handles attaching/state
                 await _clientRepository.UpdateAsync(client);
                 _logger.LogInformation("Successfully updated client ID: {ClientId}", client.ClientId);
                 return true;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error updating client ID: {ClientId}", client.ClientId);
                 return false; // Or rethrow depending on desired error handling
            }
        }

        public async Task<bool> DeleteClientAsync(Guid id)
        {
             _logger.LogInformation("Deleting client ID: {ClientId}", id);
             // Add business logic? Check for active appointments?
             // Soft delete might be better (set IsActive = false)
             try
             {
                 var client = await _clientRepository.GetByIdAsync(id);
                 if (client == null)
                 {
                     _logger.LogWarning("Delete failed: Client ID {ClientId} not found.", id);
                     return false;
                 }
                 
                 // Soft delete example:
                 // client.IsActive = false;
                 // client.LastUpdatedAt = DateTime.UtcNow;
                 // await _clientRepository.UpdateAsync(client);
                 
                 // Hard delete:
                 await _clientRepository.DeleteAsync(id);
                 _logger.LogInformation("Successfully deleted client ID: {ClientId}", id);
                 return true;
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error deleting client ID: {ClientId}", id);
                 return false; // Or rethrow
             }
        }

        public async Task<int> GetClientCountAsync()
        {
            _logger.LogInformation("Getting total client count");
            try
            {
                // Assuming repository has a CountAsync method or similar
                // return await _clientRepository.CountAsync();
                
                // Alternative: Get all and count
                var allClients = await _clientRepository.GetAllAsync();
                return allClients.Count();
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error getting client count");
                 throw;
            }
        }
    }
} 
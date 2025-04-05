using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.Models;

namespace MassageBooking.API.Services
{
    public interface IClientService
    {
        Task<Client?> GetClientByIdAsync(Guid id);
        
        Task<Client?> GetClientByEmailAsync(string email);
        
        Task<IEnumerable<Client>> GetAllClientsAsync();
        
        Task<IEnumerable<Client>> GetActiveClientsAsync();
        
        Task<Client> CreateClientAsync(Client client);
        
        Task<bool> UpdateClientAsync(Client client);
        
        Task<bool> DeleteClientAsync(Guid id);
        
        Task<int> GetClientCountAsync();
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.Models;

namespace MassageBooking.API.Data.Repositories
{
    public interface IClientRepository
    {
        Task<Client?> GetByIdAsync(Guid id);
        Task<IEnumerable<Client>> GetAllAsync();
        Task<Client?> GetByEmailAsync(string email);
        Task AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task DeleteAsync(Guid id);
    }
} 
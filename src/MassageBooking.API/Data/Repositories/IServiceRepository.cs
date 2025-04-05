using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.Models;

namespace MassageBooking.API.Data.Repositories
{
    public interface IServiceRepository
    {
        Task<Service?> GetByIdAsync(Guid id);
        Task<IEnumerable<Service>> GetAllAsync();
        Task<IEnumerable<Service>> GetActiveServicesAsync();
        Task AddAsync(Service service);
        Task UpdateAsync(Service service);
        Task DeleteAsync(Guid id);
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.Models;

namespace MassageBooking.API.Data.Repositories
{
    public interface ITherapistRepository
    {
        Task<Therapist?> GetByIdAsync(Guid id);
        Task<IEnumerable<Therapist>> GetAllAsync();
        Task<IEnumerable<Service>> GetTherapistServicesAsync(Guid therapistId);
        Task AddAsync(Therapist therapist);
        Task UpdateAsync(Therapist therapist);
        Task DeleteAsync(Guid id);
        Task<bool> AddServiceToTherapistAsync(Guid therapistId, Guid serviceId);
        Task<bool> RemoveServiceFromTherapistAsync(Guid therapistId, Guid serviceId);
    }
} 
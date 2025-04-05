using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.Models;

namespace MassageBooking.API.Services
{
    /// <summary>
    /// Interface for managing Service entities.
    /// </summary>
    public interface IServiceService
    {
        /// <summary>
        /// Gets all services.
        /// </summary>
        /// <returns>A collection of all services.</returns>
        Task<IEnumerable<Service>> GetAllServicesAsync();

        /// <summary>
        /// Gets a specific service by its ID.
        /// </summary>
        /// <param name="serviceId">The ID of the service.</param>
        /// <returns>The service, or null if not found.</returns>
        Task<Service> GetServiceByIdAsync(Guid serviceId);

        /// <summary>
        /// Creates a new service.
        /// </summary>
        /// <param name="service">The service to create.</param>
        /// <returns>The created service with its generated ID.</returns>
        Task<Service> CreateServiceAsync(Service service);

        /// <summary>
        /// Updates an existing service.
        /// </summary>
        /// <param name="service">The service with updated information.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdateServiceAsync(Service service);

        /// <summary>
        /// Deletes a service by its ID.
        /// </summary>
        /// <param name="serviceId">The ID of the service to delete.</param>
        /// <returns>True if the deletion was successful, false otherwise.</returns>
        Task<bool> DeleteServiceAsync(Guid serviceId);
        
        /// <summary>
        /// Gets all active services.
        /// </summary>
        /// <returns>A collection of active services.</returns>
        Task<IEnumerable<Service>> GetActiveServicesAsync();
    }
} 
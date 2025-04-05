using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.Data.Repositories;
using MassageBooking.API.Models;
using Microsoft.Extensions.Logging;

namespace MassageBooking.API.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<ServiceService> _logger;

        public ServiceService(IServiceRepository serviceRepository, ILogger<ServiceService> logger)
        {
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            _logger.LogInformation("Retrieving all services");
            return await _serviceRepository.GetAllAsync();
        }

        public async Task<Service> GetServiceByIdAsync(Guid serviceId)
        {
             _logger.LogInformation("Retrieving service by ID: {ServiceId}", serviceId);
             var service = await _serviceRepository.GetByIdAsync(serviceId);
             if (service == null)
             {
                 _logger.LogWarning("Service with ID {ServiceId} not found.", serviceId);
             }
             return service;
        }

        public async Task<Service> CreateServiceAsync(Service service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
            
            _logger.LogInformation("Creating new service: {ServiceName}", service.Name);
            service.ServiceId = Guid.NewGuid(); // Ensure new Guid
            service.CreatedAt = DateTime.UtcNow;
            service.UpdatedAt = DateTime.UtcNow;
            
            await _serviceRepository.AddAsync(service);
            _logger.LogInformation("Service created successfully with ID: {ServiceId}", service.ServiceId);
            
            // Repository AddAsync doesn't return the entity, so we fetch it if needed elsewhere,
            // but here we can just return the input object as it has the ID.
            return service; 
        }

        public async Task<bool> UpdateServiceAsync(Service service)
        {
             if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            _logger.LogInformation("Attempting to update service ID: {ServiceId}", service.ServiceId);
            var existingService = await _serviceRepository.GetByIdAsync(service.ServiceId);
            if (existingService == null)
            {
                _logger.LogWarning("Update failed: Service with ID {ServiceId} not found.", service.ServiceId);
                return false;
            }

            // Update properties
            existingService.Name = service.Name;
            existingService.Description = service.Description;
            existingService.DurationMinutes = service.DurationMinutes;
            existingService.Price = service.Price;
            existingService.IsActive = service.IsActive;
            existingService.UpdatedAt = DateTime.UtcNow;

            await _serviceRepository.UpdateAsync(existingService);
            _logger.LogInformation("Service ID: {ServiceId} updated successfully.", service.ServiceId);
            return true;
        }

        public async Task<bool> DeleteServiceAsync(Guid serviceId)
        {
            _logger.LogInformation("Attempting to delete service ID: {ServiceId}", serviceId);
            var serviceToDelete = await _serviceRepository.GetByIdAsync(serviceId);
            if (serviceToDelete == null)
            {
                 _logger.LogWarning("Delete failed: Service with ID {ServiceId} not found.", serviceId);
                return false;
            }

            await _serviceRepository.DeleteAsync(serviceId);
            _logger.LogInformation("Service ID: {ServiceId} deleted successfully.", serviceId);
            return true;
        }
        
         public async Task<IEnumerable<Service>> GetActiveServicesAsync()
        {
            _logger.LogInformation("Retrieving all active services");
            // Assuming repository has or will have a method to filter by IsActive
            // If not, we'd get all and filter here, but that's less efficient.
            // For now, let's assume GetActiveAsync exists or will be added.
            // return await _serviceRepository.GetActiveAsync(); 
            
            // Alternative if GetActiveAsync doesn't exist:
            var allServices = await _serviceRepository.GetAllAsync();
            return allServices.Where(s => s.IsActive);
        }
    }
} 
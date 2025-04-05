using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.DTOs;
using MassageBooking.API.Models;

namespace MassageBooking.API.Services
{
    /// <summary>
    /// Interface for therapist management services
    /// </summary>
    public interface ITherapistService
    {
        /// <summary>
        /// Gets a therapist by ID
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <returns>The therapist details or null if not found</returns>
        Task<TherapistDetailsDTO> GetTherapistByIdAsync(Guid therapistId);
        
        /// <summary>
        /// Gets all therapists
        /// </summary>
        /// <returns>List of all therapists</returns>
        Task<List<TherapistListItemDTO>> GetAllTherapistsListAsync();
        
        /// <summary>
        /// Gets all active therapists
        /// </summary>
        /// <returns>List of active therapists</returns>
        Task<IEnumerable<TherapistListItemDTO>> GetActiveTherapistsAsync();
        
        /// <summary>
        /// Gets all services offered by a therapist
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <returns>List of services offered by the therapist</returns>
        Task<List<ServiceDTO>> GetTherapistServicesAsync(Guid therapistId);
        
        /// <summary>
        /// Gets the availability of a therapist for a specific date
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="date">The date to check availability for</param>
        /// <returns>Availability information for the specified date</returns>
        Task<TherapistAvailabilityDTO> GetTherapistAvailabilityAsync(Guid therapistId, DateTime date);
        
        /// <summary>
        /// Updates the availability of a therapist for a specific date or day of week
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="request">The update request</param>
        /// <returns>Result of the operation</returns>
        Task<OperationResultDTO> UpdateTherapistAvailabilityAsync(Guid therapistId, UpdateAvailabilityRequestDTO request);
        
        /// <summary>
        /// Blocks a specific time period for a therapist
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="request">The block time request</param>
        /// <returns>Result of the operation</returns>
        Task<OperationResultDTO> BlockTherapistTimeAsync(Guid therapistId, BlockTimeRequestDTO request);
        
        /// <summary>
        /// Adds a new therapist
        /// </summary>
        /// <param name="request">The therapist information</param>
        /// <returns>Result of the operation</returns>
        Task<OperationResultDTO> AddTherapistAsync(CreateTherapistDTO request);
        
        /// <summary>
        /// Updates an existing therapist
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="request">The updated information</param>
        /// <returns>Result of the operation</returns>
        Task<OperationResultDTO> UpdateTherapistAsync(Guid therapistId, UpdateTherapistDTO request);
        
        /// <summary>
        /// Deletes a therapist (soft delete recommended)
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <returns>Result of the operation</returns>
        Task<OperationResultDTO> DeleteTherapistAsync(Guid therapistId);
        
        /// <summary>
        /// Adds a service to a therapist's offered services
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="serviceId">The service ID</param>
        /// <returns>Result of the operation</returns>
        Task<OperationResultDTO> AddServiceToTherapistAsync(Guid therapistId, Guid serviceId);
        
        /// <summary>
        /// Removes a service from a therapist's offered services
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="serviceId">The service ID</param>
        /// <returns>Result of the operation</returns>
        Task<OperationResultDTO> RemoveServiceFromTherapistAsync(Guid therapistId, Guid serviceId);

        /// <summary>
        /// Gets a therapist entity by ID (used internally for updates)
        /// </summary>
        Task<Therapist?> GetTherapistEntityByIdAsync(Guid therapistId);
    }
} 
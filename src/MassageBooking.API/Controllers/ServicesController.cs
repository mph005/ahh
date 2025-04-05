using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.DTOs;
using MassageBooking.API.Services;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MassageBooking.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace MassageBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // GET actions require authentication, CUD actions require Admin role
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService; // Use Service layer
        private readonly IMapper _mapper;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(IServiceService serviceService, IMapper mapper, ILogger<ServicesController> logger)
        {
            _serviceService = serviceService ?? throw new ArgumentNullException(nameof(serviceService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/services
        [HttpGet]
        // [AllowAnonymous] // Changed: Require authentication to view services
        [Authorize] 
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServices()
        {
            _logger.LogInformation("Getting all services");
            var services = await _serviceService.GetAllServicesAsync();
            var serviceDtos = _mapper.Map<IEnumerable<ServiceDTO>>(services);
            return Ok(serviceDtos);
        }

        // GET: api/services/{id}
        [HttpGet("{id:guid}")]
        // [AllowAnonymous] // Changed: Require authentication to view specific service
        [Authorize]
        public async Task<ActionResult<ServiceDTO>> GetService(Guid id)
        {
            _logger.LogInformation("Getting service by ID: {ServiceId}", id);
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
            {
                _logger.LogWarning("Service with ID {ServiceId} not found.", id);
                return NotFound();
            }
            var serviceDto = _mapper.Map<ServiceDTO>(service);
            return Ok(serviceDto);
        }

        // POST: api/services
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admins can create services
        public async Task<ActionResult<ServiceDTO>> CreateService([FromBody] CreateServiceDTO createDto)
        {
            _logger.LogInformation("Attempting to create a new service: {Name}", createDto.Name);
            var service = _mapper.Map<Service>(createDto);

            // Service layer handles creation and returns the created entity
            var createdService = await _serviceService.CreateServiceAsync(service);

            _logger.LogInformation("Service created successfully with ID: {ServiceId}", createdService.ServiceId);
            var dto = _mapper.Map<ServiceDTO>(createdService);

            return CreatedAtAction(nameof(GetService), new { id = dto.ServiceId }, dto);
        }

        // PUT: api/services/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")] // Only Admins can update services
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceDTO updateDto)
        {
            _logger.LogInformation("Attempting to update service ID: {ServiceId}", id);
             if (id != updateDto.ServiceId) // Ensure IDs match
            {
                 _logger.LogWarning("Mismatched Service ID in route ({RouteId}) and body ({BodyId}).", id, updateDto.ServiceId);
                return BadRequest("ID mismatch");
            }

            var serviceToUpdate = await _serviceService.GetServiceByIdAsync(id);
            if (serviceToUpdate == null)
            {
                _logger.LogWarning("Update failed: Service with ID {ServiceId} not found.", id);
                return NotFound();
            }

            _mapper.Map(updateDto, serviceToUpdate); // Apply updates from DTO

            var success = await _serviceService.UpdateServiceAsync(serviceToUpdate);

            if (!success)
            {
                 _logger.LogError("Failed to update service ID: {ServiceId}.", id);
                return StatusCode(500, "An error occurred while updating the service.");
            }

            _logger.LogInformation("Service ID: {ServiceId} updated successfully.", id);
            return NoContent();
        }

        // DELETE: api/services/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")] // Only Admins can delete services
        public async Task<IActionResult> DeleteService(Guid id)
        {
            _logger.LogInformation("Attempting to delete service ID: {ServiceId}", id);
            var success = await _serviceService.DeleteServiceAsync(id);
            if (!success)
            {
                _logger.LogWarning("Delete failed: Service with ID {ServiceId} not found or could not be deleted.", id);
                return NotFound();
            }
            _logger.LogInformation("Service ID: {ServiceId} deleted successfully.", id);
            return NoContent();
        }
    }
} 
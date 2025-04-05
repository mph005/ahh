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
    // [Authorize(Roles = "Admin,Therapist")] // Secure endpoints appropriately - Changed to Admin Only
    [Authorize(Roles = "Admin")] // Require Admin role for all client management actions
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService; // Use Service layer
        private readonly IMapper _mapper;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IClientService clientService, IMapper mapper, ILogger<ClientsController> logger)
        {
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientListItemDTO>>> GetClients()
        {
            _logger.LogInformation("Getting all clients");
            var clients = await _clientService.GetAllClientsAsync();
            var clientDtos = _mapper.Map<IEnumerable<ClientListItemDTO>>(clients);
            return Ok(clientDtos);
        }

        // GET: api/clients/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClientDetailsDTO>> GetClient(Guid id)
        {
            _logger.LogInformation("Getting client by ID: {ClientId}", id);
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
            {
                _logger.LogWarning("Client with ID {ClientId} not found.", id);
                return NotFound();
            }
            var clientDto = _mapper.Map<ClientDetailsDTO>(client);
            return Ok(clientDto);
        }

        // POST: api/clients
        [HttpPost]
        // [Authorize(Roles = "Admin")] // Redundant due to controller-level attribute, but kept for clarity if needed
        public async Task<ActionResult<ClientDetailsDTO>> CreateClient([FromBody] CreateClientDTO createDto)
        {
             _logger.LogInformation("Attempting to create a new client: {FirstName} {LastName}", createDto.FirstName, createDto.LastName);
            var client = _mapper.Map<Client>(createDto);
            
            // Service layer handles creation and returns the created entity
            var createdClient = await _clientService.CreateClientAsync(client);

             _logger.LogInformation("Client created successfully with ID: {ClientId}", createdClient.ClientId);
            var dto = _mapper.Map<ClientDetailsDTO>(createdClient);

            return CreatedAtAction(nameof(GetClient), new { id = dto.ClientId }, dto);
        }

        // PUT: api/clients/{id}
        [HttpPut("{id:guid}")]
        // [Authorize(Roles = "Admin")] // Redundant due to controller-level attribute, but kept for clarity if needed
        public async Task<IActionResult> UpdateClient(Guid id, [FromBody] UpdateClientDTO updateDto)
        {
             _logger.LogInformation("Attempting to update client ID: {ClientId}", id);
            if (id != updateDto.ClientId) // Ensure IDs match
            {
                _logger.LogWarning("Mismatched Client ID in route ({RouteId}) and body ({BodyId}).", id, updateDto.ClientId);
                return BadRequest("ID mismatch");
            }

            var clientToUpdate = await _clientService.GetClientByIdAsync(id);
            if (clientToUpdate == null)
            {
                _logger.LogWarning("Update failed: Client with ID {ClientId} not found.", id);
                return NotFound();
            }

            _mapper.Map(updateDto, clientToUpdate); // Apply updates from DTO
            
            var success = await _clientService.UpdateClientAsync(clientToUpdate);
            
            if (!success)
            {
                 _logger.LogError("Failed to update client ID: {ClientId}.", id);
                 // Service should ideally throw specific exceptions, but for now...
                 return StatusCode(500, "An error occurred while updating the client.");
            }
            
            _logger.LogInformation("Client ID: {ClientId} updated successfully.", id);
            return NoContent();
        }

        // DELETE: api/clients/{id}
        [HttpDelete("{id:guid}")]
        // [Authorize(Roles = "Admin")] // Redundant due to controller-level attribute, but kept for clarity if needed
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            _logger.LogInformation("Attempting to delete client ID: {ClientId}", id);
            var success = await _clientService.DeleteClientAsync(id);
            if (!success)
            {
                // Could be NotFound or some other error
                _logger.LogWarning("Delete failed: Client with ID {ClientId} not found or could not be deleted.", id);
                return NotFound(); // Or handle other potential failure reasons from service
            }
            _logger.LogInformation("Client ID: {ClientId} deleted successfully.", id);
            return NoContent();
        }
    }
} 
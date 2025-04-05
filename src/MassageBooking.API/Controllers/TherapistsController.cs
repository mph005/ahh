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
    public class TherapistsController : ControllerBase
    {
        private readonly ITherapistService _therapistService; // Use Service layer
        private readonly IMapper _mapper;
        private readonly ILogger<TherapistsController> _logger;

        public TherapistsController(ITherapistService therapistService, IMapper mapper, ILogger<TherapistsController> logger)
        {
            _therapistService = therapistService ?? throw new ArgumentNullException(nameof(therapistService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/therapists
        [HttpGet]
        [AllowAnonymous] // Allow anyone to see available therapists
        public async Task<ActionResult<IEnumerable<TherapistListItemDTO>>> GetTherapists()
        {
            _logger.LogInformation("Getting all therapists");
            var therapistDtos = await _therapistService.GetAllTherapistsListAsync();
            return Ok(therapistDtos);
        }

        // GET: api/therapists/{id}
        [HttpGet("{id:guid}")]
        [AllowAnonymous] // Allow anyone to see specific therapist details
        public async Task<ActionResult<TherapistDetailsDTO>> GetTherapist(Guid id)
        {
            _logger.LogInformation("Getting therapist by ID: {TherapistId}", id);
            var therapist = await _therapistService.GetTherapistByIdAsync(id);
            if (therapist == null)
            {
                _logger.LogWarning("Therapist with ID {TherapistId} not found.", id);
                return NotFound();
            }
            var therapistDto = _mapper.Map<TherapistDetailsDTO>(therapist);
            return Ok(therapistDto);
        }
        
        // GET: api/therapists/active
        [HttpGet("active")]
        [AllowAnonymous] // Allow anyone to see active therapists
        public async Task<ActionResult<IEnumerable<TherapistListItemDTO>>> GetActiveTherapists()
        {
            _logger.LogInformation("Getting all active therapists");
            var therapistDtos = await _therapistService.GetActiveTherapistsAsync();
            return Ok(therapistDtos);
        }

        // POST: api/therapists
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admins can create therapists
        public async Task<ActionResult<TherapistDetailsDTO>> CreateTherapist([FromBody] CreateTherapistDTO createDto)
        {
            _logger.LogInformation("Attempting to create a new therapist: {FirstName} {LastName}", createDto.FirstName, createDto.LastName);
            var result = await _therapistService.AddTherapistAsync(createDto);

            if (!result.Success)
            {
                _logger.LogError("Failed to create therapist: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            _logger.LogInformation("Therapist created successfully.");
            return CreatedAtAction(nameof(GetTherapist), new { /* id = ??? */ }, createDto);
        }

        // PUT: api/therapists/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")] // Only Admins can update therapists
        public async Task<IActionResult> UpdateTherapist(Guid id, [FromBody] UpdateTherapistDTO updateDto)
        {
            _logger.LogInformation("Attempting to update therapist ID: {TherapistId}", id);
             if (id != updateDto.TherapistId) // Ensure IDs match
            {
                _logger.LogWarning("Mismatched Therapist ID in route ({RouteId}) and body ({BodyId}).", id, updateDto.TherapistId);
                return BadRequest("ID mismatch");
            }

            var result = await _therapistService.UpdateTherapistAsync(id, updateDto);

            if (!result.Success)
            {
                _logger.LogError("Failed to update therapist ID: {TherapistId}. Error: {ErrorMessage}", id, result.ErrorMessage);
                if (result.ErrorMessage != null && result.ErrorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result.ErrorMessage);
                }
                return StatusCode(500, result.ErrorMessage ?? "An error occurred while updating the therapist.");
            }

            _logger.LogInformation("Therapist ID: {TherapistId} updated successfully.", id);
            return NoContent();
        }

        // DELETE: api/therapists/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")] // Only Admins can delete therapists
        public async Task<IActionResult> DeleteTherapist(Guid id)
        {
             _logger.LogInformation("Attempting to delete therapist ID: {TherapistId}", id);
             
            var result = await _therapistService.DeleteTherapistAsync(id);
            
            if (!result.Success)
            {
                _logger.LogWarning("Delete failed for therapist ID {TherapistId}: {ErrorMessage}", id, result.ErrorMessage);
                if (result.ErrorMessage != null && result.ErrorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result.ErrorMessage);
                }
                return BadRequest(result.ErrorMessage ?? "Could not delete therapist.");
            }
            
            _logger.LogInformation("Therapist ID: {TherapistId} deleted successfully.", id);
            return NoContent();
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MassageBooking.API.Services;
using MassageBooking.API.DTOs;
using MassageBooking.API.Data.Repositories;
using MassageBooking.API.Models;

namespace MassageBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SoapNotesController : ControllerBase
    {
        private readonly ISoapNoteRepository _soapNoteRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ITherapistRepository _therapistRepository;
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<SoapNotesController> _logger;

        public SoapNotesController(
            ISoapNoteRepository soapNoteRepository,
            IAppointmentRepository appointmentRepository,
            ITherapistRepository therapistRepository,
            IClientRepository clientRepository,
            ILogger<SoapNotesController> logger)
        {
            _soapNoteRepository = soapNoteRepository ?? throw new ArgumentNullException(nameof(soapNoteRepository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _therapistRepository = therapistRepository ?? throw new ArgumentNullException(nameof(therapistRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a SOAP note by ID
        /// </summary>
        /// <param name="id">The SOAP note ID</param>
        /// <returns>The SOAP note details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult<SoapNoteDTO>> GetById(Guid id)
        {
            try
            {
                var soapNote = await _soapNoteRepository.GetByIdAsync(id);
                if (soapNote == null)
                {
                    return NotFound();
                }

                return Ok(MapToSoapNoteDTO(soapNote));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SOAP note {SoapNoteId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets a SOAP note for a specific appointment
        /// </summary>
        /// <param name="appointmentId">The appointment ID</param>
        /// <returns>The SOAP note for the appointment</returns>
        [HttpGet("appointment/{appointmentId}")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult<SoapNoteDTO>> GetByAppointmentId(Guid appointmentId)
        {
            try
            {
                var soapNote = await _soapNoteRepository.GetByAppointmentIdAsync(appointmentId);
                if (soapNote == null)
                {
                    return NotFound();
                }

                return Ok(MapToSoapNoteDTO(soapNote));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SOAP note for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets all SOAP notes for a client
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <returns>List of SOAP notes for the client</returns>
        [HttpGet("client/{clientId}")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult<List<SoapNoteListItemDTO>>> GetByClientId(Guid clientId)
        {
            try
            {
                var soapNotes = await _soapNoteRepository.GetByClientIdAsync(clientId);
                return Ok(soapNotes.ConvertAll(s => MapToSoapNoteListItemDTO(s)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SOAP notes for client {ClientId}", clientId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets all SOAP notes created by a therapist
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <returns>List of SOAP notes created by the therapist</returns>
        [HttpGet("therapist/{therapistId}")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult<List<SoapNoteListItemDTO>>> GetByTherapistId(Guid therapistId)
        {
            try
            {
                var soapNotes = await _soapNoteRepository.GetByTherapistIdAsync(therapistId);
                return Ok(soapNotes.ConvertAll(s => MapToSoapNoteListItemDTO(s)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SOAP notes for therapist {TherapistId}", therapistId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Creates a new SOAP note
        /// </summary>
        /// <param name="request">The SOAP note data</param>
        /// <returns>The created SOAP note</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult<SoapNoteDTO>> Create([FromBody] CreateSoapNoteDTO request)
        {
            try
            {
                // Validate appointment exists
                var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId);
                if (appointment == null)
                {
                    return BadRequest($"Appointment with ID {request.AppointmentId} not found.");
                }

                // Validate therapist exists
                var therapist = await _therapistRepository.GetByIdAsync(request.TherapistId);
                if (therapist == null)
                {
                    return BadRequest($"Therapist with ID {request.TherapistId} not found.");
                }

                // Validate client exists
                var client = await _clientRepository.GetByIdAsync(request.ClientId);
                if (client == null)
                {
                    return BadRequest($"Client with ID {request.ClientId} not found.");
                }

                // Check if SOAP note already exists for this appointment
                var existingNote = await _soapNoteRepository.GetByAppointmentIdAsync(request.AppointmentId);
                if (existingNote != null)
                {
                    return Conflict($"A SOAP note already exists for appointment {request.AppointmentId}.");
                }

                // Create SOAP note
                var soapNote = new SoapNote
                {
                    SoapNoteId = Guid.NewGuid(),
                    AppointmentId = request.AppointmentId,
                    TherapistId = request.TherapistId,
                    ClientId = request.ClientId,
                    Subjective = request.Subjective,
                    Objective = request.Objective,
                    Assessment = request.Assessment,
                    Plan = request.Plan,
                    AreasOfFocus = request.AreasOfFocus,
                    TechniquesUsed = request.TechniquesUsed,
                    PressureLevel = request.PressureLevel,
                    IsFinalized = false
                };

                var createdNote = await _soapNoteRepository.AddAsync(soapNote);
                var result = MapToSoapNoteDTO(createdNote);
                
                // Add therapist and client names for the response
                result.TherapistName = $"{therapist.FirstName} {therapist.LastName}";
                result.ClientName = $"{client.FirstName} {client.LastName}";
                result.AppointmentDate = appointment.StartTime;
                
                return CreatedAtAction(nameof(GetById), new { id = result.SoapNoteId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SOAP note for appointment {AppointmentId}", request.AppointmentId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Updates an existing SOAP note
        /// </summary>
        /// <param name="id">The SOAP note ID</param>
        /// <param name="request">The updated SOAP note data</param>
        /// <returns>The updated SOAP note</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult<SoapNoteDTO>> Update(Guid id, [FromBody] UpdateSoapNoteDTO request)
        {
            try
            {
                var existingNote = await _soapNoteRepository.GetByIdAsync(id);
                if (existingNote == null)
                {
                    return NotFound();
                }

                if (existingNote.IsFinalized)
                {
                    return BadRequest("Cannot update a finalized SOAP note.");
                }

                // Update properties
                existingNote.Subjective = request.Subjective;
                existingNote.Objective = request.Objective;
                existingNote.Assessment = request.Assessment;
                existingNote.Plan = request.Plan;
                existingNote.AreasOfFocus = request.AreasOfFocus;
                existingNote.TechniquesUsed = request.TechniquesUsed;
                existingNote.PressureLevel = request.PressureLevel;

                var updatedNote = await _soapNoteRepository.UpdateAsync(existingNote);
                return Ok(MapToSoapNoteDTO(updatedNote));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SOAP note {SoapNoteId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Finalizes a SOAP note, making it read-only
        /// </summary>
        /// <param name="id">The SOAP note ID</param>
        /// <returns>Success or error message</returns>
        [HttpPost("{id}/finalize")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult> Finalize(Guid id)
        {
            try
            {
                var result = await _soapNoteRepository.FinalizeAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { Message = "SOAP note finalized successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing SOAP note {SoapNoteId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Deletes a SOAP note
        /// </summary>
        /// <param name="id">The SOAP note ID</param>
        /// <returns>Success or error message</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _soapNoteRepository.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { Message = "SOAP note deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting SOAP note {SoapNoteId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        #region Helper Methods

        private SoapNoteDTO MapToSoapNoteDTO(SoapNote soapNote)
        {
            var therapistName = soapNote.Therapist != null
                ? $"{soapNote.Therapist.FirstName} {soapNote.Therapist.LastName}"
                : string.Empty;

            var clientName = soapNote.Client != null
                ? $"{soapNote.Client.FirstName} {soapNote.Client.LastName}"
                : string.Empty;

            var appointmentDate = soapNote.Appointment?.StartTime ?? DateTime.MinValue;
            var serviceName = soapNote.Appointment?.Service?.Name ?? string.Empty;

            return new SoapNoteDTO
            {
                SoapNoteId = soapNote.SoapNoteId,
                AppointmentId = soapNote.AppointmentId,
                TherapistId = soapNote.TherapistId,
                TherapistName = therapistName,
                ClientId = soapNote.ClientId,
                ClientName = clientName,
                AppointmentDate = appointmentDate,
                ServiceName = serviceName,
                Subjective = soapNote.Subjective,
                Objective = soapNote.Objective,
                Assessment = soapNote.Assessment,
                Plan = soapNote.Plan,
                AreasOfFocus = soapNote.AreasOfFocus,
                TechniquesUsed = soapNote.TechniquesUsed,
                PressureLevel = soapNote.PressureLevel,
                IsFinalized = soapNote.IsFinalized,
                CreatedAt = soapNote.CreatedAt,
                UpdatedAt = soapNote.UpdatedAt,
                FinalizedAt = soapNote.FinalizedAt
            };
        }

        private SoapNoteListItemDTO MapToSoapNoteListItemDTO(SoapNote soapNote)
        {
            var therapistName = soapNote.Therapist != null
                ? $"{soapNote.Therapist.FirstName} {soapNote.Therapist.LastName}"
                : string.Empty;

            var clientName = soapNote.Client != null
                ? $"{soapNote.Client.FirstName} {soapNote.Client.LastName}"
                : string.Empty;

            var appointmentDate = soapNote.Appointment?.StartTime ?? DateTime.MinValue;
            var serviceName = soapNote.Appointment?.Service?.Name ?? string.Empty;

            return new SoapNoteListItemDTO
            {
                SoapNoteId = soapNote.SoapNoteId,
                AppointmentId = soapNote.AppointmentId,
                TherapistId = soapNote.TherapistId,
                TherapistName = therapistName,
                ClientId = soapNote.ClientId,
                ClientName = clientName,
                AppointmentDate = appointmentDate,
                ServiceName = serviceName,
                IsFinalized = soapNote.IsFinalized,
                CreatedAt = soapNote.CreatedAt
            };
        }

        #endregion
    }
} 
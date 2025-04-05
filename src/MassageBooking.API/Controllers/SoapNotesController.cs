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
using AutoMapper;

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
        private readonly IMapper _mapper;

        public SoapNotesController(
            ISoapNoteRepository soapNoteRepository,
            IAppointmentRepository appointmentRepository,
            ITherapistRepository therapistRepository,
            IClientRepository clientRepository,
            ILogger<SoapNotesController> logger,
            IMapper mapper)
        {
            _soapNoteRepository = soapNoteRepository ?? throw new ArgumentNullException(nameof(soapNoteRepository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _therapistRepository = therapistRepository ?? throw new ArgumentNullException(nameof(therapistRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Gets a SOAP note by ID
        /// </summary>
        /// <param name="id">The SOAP note ID</param>
        /// <returns>The SOAP note details</returns>
        [HttpGet("{id:guid}")]
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

                return Ok(_mapper.Map<SoapNoteDTO>(soapNote));
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
        [HttpGet("appointment/{appointmentId:guid}")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult<IEnumerable<SoapNoteListItemDTO>>> GetByAppointmentId(Guid appointmentId)
        {
            try
            {
                var soapNotes = await _soapNoteRepository.GetByAppointmentIdAsync(appointmentId);
                if (soapNotes == null)
                {
                    return NotFound();
                }

                return Ok(_mapper.Map<IEnumerable<SoapNoteListItemDTO>>(soapNotes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SOAP notes for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets all SOAP notes for a client
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <returns>List of SOAP notes for the client</returns>
        [HttpGet("client/{clientId:guid}")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult<IEnumerable<SoapNoteListItemDTO>>> GetByClientId(Guid clientId)
        {
            try
            {
                var soapNotes = await _soapNoteRepository.GetByClientIdAsync(clientId);
                return Ok(_mapper.Map<IEnumerable<SoapNoteListItemDTO>>(soapNotes));
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
        [HttpGet("therapist/{therapistId:guid}")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult<IEnumerable<SoapNoteListItemDTO>>> GetByTherapistId(Guid therapistId)
        {
            try
            {
                var soapNotes = await _soapNoteRepository.GetByTherapistIdAsync(therapistId);
                return Ok(_mapper.Map<IEnumerable<SoapNoteListItemDTO>>(soapNotes));
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

                await _soapNoteRepository.AddAsync(soapNote);
                var result = _mapper.Map<SoapNoteDTO>(soapNote);
                
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
        [HttpPut("{id:guid}")]
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
                _mapper.Map(request, existingNote);
                existingNote.UpdatedAt = DateTime.UtcNow;

                await _soapNoteRepository.UpdateAsync(existingNote);
                return Ok(_mapper.Map<SoapNoteDTO>(existingNote));
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
        [HttpPut("{id:guid}/finalize")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult> Finalize(Guid id)
        {
            try
            {
                bool success = await _soapNoteRepository.FinalizeAsync(id);
                if (!success)
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
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                // Call the void repository method
                await _soapNoteRepository.DeleteAsync(id);
                
                // Return NoContent (204) on successful processing
                return NoContent(); 
            }
            catch (InvalidOperationException ex) // Catch specific exception from repo
            {
                 _logger.LogWarning(ex, "Attempted operation on finalized SOAP note {SoapNoteId}", id); // Log specific warning
                 return BadRequest(ex.Message); // Return 400 for business rule violation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting SOAP note {SoapNoteId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
} 
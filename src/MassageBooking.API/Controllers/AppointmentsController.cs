using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MassageBooking.API.DTOs;
using MassageBooking.API.Models;
using MassageBooking.API.Services;
using MassageBooking.API.Data.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace MassageBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a specific appointment by ID
        /// </summary>
        /// <param name="id">The appointment ID</param>
        /// <returns>The appointment details</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AppointmentDetailsDTO>> GetAppointment(Guid id)
        {
            _logger.LogInformation("Getting appointment by ID: {Id}", id);
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                _logger.LogWarning("Appointment with ID {Id} not found", id);
                return NotFound();
            }
            return Ok(appointment);
        }

        /// <summary>
        /// Gets all appointments for a client
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <returns>List of appointments</returns>
        [HttpGet("client/{clientId:guid}")]
        public async Task<ActionResult<IEnumerable<AppointmentListItemDTO>>> GetAppointmentsByClient(Guid clientId)
        {
            _logger.LogInformation("Getting appointments for client ID: {ClientId}", clientId);
            var appointments = await _appointmentService.GetAppointmentsByClientIdAsync(clientId);
            return Ok(appointments);
        }

        /// <summary>
        /// Gets a therapist's schedule for a date range
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="startDate">The start date (format: yyyy-MM-dd)</param>
        /// <param name="endDate">The end date (format: yyyy-MM-dd)</param>
        /// <returns>List of appointments in the therapist's schedule</returns>
        [HttpGet("therapist/{therapistId:guid}")]
        public async Task<ActionResult<IEnumerable<AppointmentListItemDTO>>> GetAppointmentsByTherapist(Guid therapistId)
        {
            _logger.LogInformation("Getting appointments for therapist ID: {TherapistId}", therapistId);
            var appointments = await _appointmentService.GetAppointmentsByTherapistIdAsync(therapistId);
            return Ok(appointments);
        }

        /// <summary>
        /// Gets available appointment slots for booking
        /// </summary>
        /// <param name="serviceId">The service ID</param>
        /// <param name="therapistId">Optional therapist ID to filter by</param>
        /// <param name="startDate">The start date (format: yyyy-MM-dd)</param>
        /// <param name="endDate">The end date (format: yyyy-MM-dd)</param>
        /// <returns>List of available appointment slots</returns>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<AvailableSlotDTO>>> FindAvailableSlots(
            [FromQuery] Guid serviceId,
            [FromQuery] Guid? therapistId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            _logger.LogInformation("Finding available slots for Service: {ServiceId}, Therapist: {TherapistId}, Range: {StartDate} - {EndDate}",
                serviceId, therapistId?.ToString() ?? "Any", startDate, endDate);

            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date.");
            }

            var slots = await _appointmentService.FindAvailableSlotsAsync(serviceId, therapistId, startDate, endDate);
            return Ok(slots);
        }

        /// <summary>
        /// Gets appointments within a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>A list of appointments within the range.</returns>
        [HttpGet("range")]
        public async Task<ActionResult<IEnumerable<AppointmentDetailsDTO>>> GetAppointmentsInRange(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            _logger.LogInformation("Getting appointments in range {StartDate} to {EndDate}", startDate, endDate);

            if (startDate >= endDate)
            {
                _logger.LogWarning("Invalid date range requested: StartDate {StartDate} >= EndDate {EndDate}", startDate, endDate);
                return BadRequest("Start date must be before end date.");
            }

            try
            {
                var appointments = await _appointmentService.GetAppointmentsInRangeAsync(startDate, endDate);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for range {StartDate} to {EndDate}", startDate, endDate);
                // Return a generic 500 error to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while retrieving appointments.");
            }
        }

        // POST: api/appointments
        [HttpPost]
        public async Task<ActionResult<BookingResultDTO>> BookAppointment([FromBody] AppointmentBookingDTO bookingDto)
        {
            _logger.LogInformation("Attempting to book appointment for Client: {ClientId}, Service: {ServiceId}, Therapist: {TherapistId} at {StartTime}",
                bookingDto.ClientId, bookingDto.ServiceId, bookingDto.TherapistId, bookingDto.StartTime);

            var result = await _appointmentService.BookAppointmentAsync(bookingDto);

            if (!result.Success)
            {
                _logger.LogWarning("Booking failed: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(result);
            }

            _logger.LogInformation("Appointment booked successfully with ID: {AppointmentId}", result.AppointmentId);
            var createdAppointment = await _appointmentService.GetAppointmentByIdAsync(result.AppointmentId);
            // Return CreatedAtAction using the GetAppointment route name and the Guid ID
            return CreatedAtAction(nameof(GetAppointment), new { id = result.AppointmentId }, createdAppointment);
        }

        // PUT: api/appointments/{id}/cancel
        [HttpPut("{id:guid}/cancel")]
        public async Task<IActionResult> CancelAppointment(Guid id, [FromBody] string? reason)
        {
            _logger.LogInformation("Attempting to cancel appointment ID: {Id} with reason: {Reason}", id, reason ?? "N/A");
            var success = await _appointmentService.CancelAppointmentAsync(id, reason);
            if (!success)
            {
                _logger.LogWarning("Failed to cancel appointment ID: {Id}. It might not exist or is already completed.", id);
                return NotFound();
            }
            _logger.LogInformation("Appointment ID: {Id} cancelled successfully.", id);
            return NoContent();
        }

        // PUT: api/appointments/reschedule
        [HttpPut("reschedule")]
        public async Task<ActionResult<BookingResultDTO>> RescheduleAppointment([FromBody] AppointmentRescheduleDTO rescheduleDto)
        {
            _logger.LogInformation("Attempting to reschedule appointment ID: {AppointmentId} to {NewStartTime}",
                rescheduleDto.AppointmentId, rescheduleDto.NewStartTime);

            var result = await _appointmentService.RescheduleAppointmentAsync(rescheduleDto);
            if (!result.Success)
            {
                _logger.LogWarning("Reschedule failed for Appointment ID {AppointmentId}: {ErrorMessage}", rescheduleDto.AppointmentId, result.ErrorMessage);
                return BadRequest(result);
            }
            _logger.LogInformation("Appointment ID: {AppointmentId} rescheduled successfully.", result.AppointmentId);
            return Ok(result);
        }

        // DELETE: api/appointments/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAppointment(Guid id)
        {
            _logger.LogInformation("Attempting to delete appointment ID: {Id}", id);
            var success = await _appointmentService.DeleteAppointmentAsync(id);
            if (!success)
            {
                _logger.LogWarning("Failed to delete appointment ID: {Id}. It might not exist.", id);
                return NotFound();
            }
            _logger.LogInformation("Appointment ID: {Id} deleted successfully.", id);
            return NoContent();
        }
    }
} 
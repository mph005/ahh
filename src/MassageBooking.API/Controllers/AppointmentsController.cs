using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MassageBooking.API.Services;
using MassageBooking.API.DTOs;

namespace MassageBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IAppointmentService appointmentService,
            ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a specific appointment by ID
        /// </summary>
        /// <param name="id">The appointment ID</param>
        /// <returns>The appointment details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDetailsDTO>> GetAppointment(Guid id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment {AppointmentId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets all appointments for a client
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <returns>List of appointments</returns>
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<AppointmentListItemDTO>>> GetClientAppointments(Guid clientId)
        {
            try
            {
                var appointments = await _appointmentService.GetClientAppointmentsAsync(clientId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for client {ClientId}", clientId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets a therapist's schedule for a date range
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="startDate">The start date (format: yyyy-MM-dd)</param>
        /// <param name="endDate">The end date (format: yyyy-MM-dd)</param>
        /// <returns>List of appointments in the therapist's schedule</returns>
        [HttpGet("therapist/{therapistId}")]
        public async Task<ActionResult<IEnumerable<AppointmentListItemDTO>>> GetTherapistSchedule(
            Guid therapistId, 
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                var appointments = await _appointmentService.GetTherapistScheduleAsync(therapistId, startDate, endDate);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule for therapist {TherapistId} between {StartDate} and {EndDate}", 
                    therapistId, startDate, endDate);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets available appointment slots for booking
        /// </summary>
        /// <param name="serviceId">The service ID</param>
        /// <param name="therapistId">Optional therapist ID to filter by</param>
        /// <param name="startDate">The start date (format: yyyy-MM-dd)</param>
        /// <param name="endDate">The end date (format: yyyy-MM-dd)</param>
        /// <returns>List of available appointment slots</returns>
        [HttpGet("available-slots")]
        public async Task<ActionResult<IEnumerable<AvailableSlotDTO>>> GetAvailableSlots(
            [FromQuery] Guid serviceId,
            [FromQuery] Guid? therapistId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("Start date must be before or equal to end date.");
                }

                var availableSlots = await _appointmentService.FindAvailableSlotsAsync(
                    serviceId, therapistId, startDate, endDate);
                return Ok(availableSlots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available slots for service {ServiceId} between {StartDate} and {EndDate}", 
                    serviceId, startDate, endDate);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Books a new appointment
        /// </summary>
        /// <param name="bookingRequest">The booking details</param>
        /// <returns>Result of the booking operation</returns>
        [HttpPost]
        public async Task<ActionResult<BookingResultDTO>> BookAppointment(AppointmentBookingDTO bookingRequest)
        {
            try
            {
                var result = await _appointmentService.BookAppointmentAsync(bookingRequest);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return CreatedAtAction(nameof(GetAppointment), new { id = result.AppointmentId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking appointment: {@BookingRequest}", bookingRequest);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Reschedules an existing appointment
        /// </summary>
        /// <param name="rescheduleRequest">The reschedule details</param>
        /// <returns>Result of the reschedule operation</returns>
        [HttpPut("reschedule")]
        public async Task<ActionResult<BookingResultDTO>> RescheduleAppointment(AppointmentRescheduleDTO rescheduleRequest)
        {
            try
            {
                var result = await _appointmentService.RescheduleAppointmentAsync(rescheduleRequest);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling appointment: {@RescheduleRequest}", rescheduleRequest);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Cancels an appointment
        /// </summary>
        /// <param name="id">The appointment ID</param>
        /// <returns>Result of the cancel operation</returns>
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<BookingResultDTO>> CancelAppointment(Guid id)
        {
            try
            {
                var result = await _appointmentService.CancelAppointmentAsync(id);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Rebooks a previous appointment
        /// </summary>
        /// <param name="rebookRequest">The rebook details</param>
        /// <returns>Result of the rebook operation</returns>
        [HttpPost("rebook")]
        public async Task<ActionResult<BookingResultDTO>> RebookAppointment(RebookRequestDTO rebookRequest)
        {
            try
            {
                var result = await _appointmentService.RebookPreviousAppointmentAsync(rebookRequest);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return CreatedAtAction(nameof(GetAppointment), new { id = result.AppointmentId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebooking appointment: {@RebookRequest}", rebookRequest);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
} 
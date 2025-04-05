using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MassageBooking.API.Services;
using MassageBooking.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace MassageBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ITherapistService _therapistService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(
            IAppointmentService appointmentService,
            ITherapistService therapistService,
            ILogger<ScheduleController> logger)
        {
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _therapistService = therapistService ?? throw new ArgumentNullException(nameof(therapistService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the daily schedule for a specific date
        /// </summary>
        /// <param name="date">The date to get the schedule for (format: yyyy-MM-dd)</param>
        /// <param name="therapistId">Optional therapist ID to filter the schedule by</param>
        /// <returns>The daily schedule with all appointments</returns>
        [HttpGet("daily")]
        public async Task<ActionResult<DailyScheduleDTO>> GetDailySchedule(
            [FromQuery] DateTime date,
            [FromQuery] Guid? therapistId = null)
        {
            try
            {
                // Set the time to midnight for consistent results
                date = date.Date;
                
                // Get all appointments for the specified date
                var endDate = date.AddDays(1).AddSeconds(-1); // End of the day
                
                var schedule = new DailyScheduleDTO
                {
                    Date = date,
                    TherapistId = therapistId,
                    TimeSlots = new List<ScheduleTimeSlotDTO>()
                };

                // If a therapist ID is provided, get schedule for just that therapist
                if (therapistId.HasValue)
                {
                    var therapistAppointments = await _appointmentService.GetTherapistScheduleAsync(
                        therapistId.Value, date, endDate);
                    
                    var therapist = await _therapistService.GetTherapistByIdAsync(therapistId.Value);
                    if (therapist == null)
                    {
                        return NotFound($"Therapist with ID {therapistId.Value} not found");
                    }
                    
                    schedule.TherapistName = therapist.FullName;
                    schedule.TimeSlots = MapAppointmentsToTimeSlots(therapistAppointments);
                }
                else
                {
                    // Get all active therapists
                    var therapists = await _therapistService.GetAllTherapistsListAsync();
                    
                    foreach (var therapist in therapists)
                    {
                        var therapistAppointments = await _appointmentService.GetTherapistScheduleAsync(
                            therapist.TherapistId, date, endDate);
                        
                        // Add this therapist's appointments to the schedule
                        var therapistTimeSlots = MapAppointmentsToTimeSlots(therapistAppointments);
                        schedule.TimeSlots.AddRange(therapistTimeSlots);
                    }
                }
                
                // Sort time slots by start time
                schedule.TimeSlots = schedule.TimeSlots
                    .OrderBy(ts => ts.StartTime)
                    .ToList();
                
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily schedule for date {Date}", date);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets the therapist availability for a specific date
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="date">The date to get availability for (format: yyyy-MM-dd)</param>
        /// <returns>The therapist's availability for the specified date</returns>
        [HttpGet("availability/{therapistId}")]
        public async Task<ActionResult<TherapistAvailabilityDTO>> GetTherapistAvailability(
            Guid therapistId,
            [FromQuery] DateTime date)
        {
            try
            {
                var availability = await _therapistService.GetTherapistAvailabilityAsync(therapistId, date);
                if (availability == null)
                {
                    return NotFound($"No availability found for therapist {therapistId} on {date:yyyy-MM-dd}");
                }
                
                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving availability for therapist {TherapistId} on date {Date}", 
                    therapistId, date);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Sets the therapist availability for a specific date or day of week
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="request">The availability update request</param>
        /// <returns>Result of the update operation</returns>
        [HttpPost("availability/{therapistId}")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult<OperationResultDTO>> SetTherapistAvailability(
            Guid therapistId,
            [FromBody] UpdateAvailabilityRequestDTO request)
        {
            try
            {
                // Verify the user has permission (either admin or the therapist themselves)
                // This would be implemented with proper auth checks
                
                var result = await _therapistService.UpdateTherapistAvailabilityAsync(therapistId, request);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating availability for therapist {TherapistId}", therapistId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Sets a specific date as unavailable for a therapist (for time off, etc.)
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <param name="request">The block time request</param>
        /// <returns>Result of the operation</returns>
        [HttpPost("block-time/{therapistId}")]
        [Authorize(Roles = "Admin,Therapist")]
        public async Task<ActionResult<OperationResultDTO>> BlockTherapistTime(
            Guid therapistId,
            [FromBody] BlockTimeRequestDTO request)
        {
            try
            {
                // Verify the user has permission (either admin or the therapist themselves)
                // This would be implemented with proper auth checks
                
                var result = await _therapistService.BlockTherapistTimeAsync(therapistId, request);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking time for therapist {TherapistId}", therapistId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        #region Helper Methods

        private List<ScheduleTimeSlotDTO> MapAppointmentsToTimeSlots(IEnumerable<AppointmentListItemDTO> appointments)
        {
            return appointments.Select(a => new ScheduleTimeSlotDTO
            {
                AppointmentId = a.AppointmentId,
                ClientName = a.ClientName,
                ServiceName = a.ServiceName,
                TherapistName = a.TherapistName,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status.ToString()
            }).ToList();
        }

        #endregion
    }
} 
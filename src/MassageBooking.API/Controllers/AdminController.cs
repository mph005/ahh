using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MassageBooking.API.Services;
using MassageBooking.API.DTOs;

namespace MassageBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ITherapistService _therapistService;
        private readonly IClientService _clientService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAppointmentService appointmentService,
            ITherapistService therapistService,
            IClientService clientService,
            ILogger<AdminController> logger)
        {
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _therapistService = therapistService ?? throw new ArgumentNullException(nameof(therapistService));
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets summary statistics for the dashboard
        /// </summary>
        /// <returns>Dashboard statistics</returns>
        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardStatsDTO>> GetDashboardStats()
        {
            try
            {
                // Get appointment stats for the current month
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                
                var monthlyAppointments = await _appointmentService.GetAppointmentsInRangeAsync(startOfMonth, endOfMonth);
                
                // Get totals
                var activeTherapistsCount = (await _therapistService.GetAllTherapistsAsync())
                    .Count(t => t.IsActive);
                
                var totalClientsCount = await _clientService.GetClientCountAsync();
                
                // Calculate statistics
                var completedAppointments = monthlyAppointments.Count(a => a.Status == "Completed");
                var cancelledAppointments = monthlyAppointments.Count(a => a.Status == "Cancelled");
                var upcomingAppointments = monthlyAppointments.Count(a => a.Status == "Scheduled" && a.StartTime > today);
                
                // Group appointments by day for the chart data
                var appointmentsByDay = monthlyAppointments
                    .GroupBy(a => a.StartTime.Date)
                    .Select(g => new DailyAppointmentCountDTO
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                
                var stats = new DashboardStatsDTO
                {
                    TotalAppointmentsThisMonth = monthlyAppointments.Count,
                    CompletedAppointmentsThisMonth = completedAppointments,
                    CancelledAppointmentsThisMonth = cancelledAppointments,
                    UpcomingAppointments = upcomingAppointments,
                    ActiveTherapistsCount = activeTherapistsCount,
                    TotalClientsCount = totalClientsCount,
                    DailyAppointmentCounts = appointmentsByDay
                };
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets appointment statistics for a specified date range
        /// </summary>
        /// <param name="startDate">Start date for the statistics</param>
        /// <param name="endDate">End date for the statistics</param>
        /// <returns>Appointment statistics for the specified date range</returns>
        [HttpGet("reports/appointments")]
        public async Task<ActionResult<AppointmentReportDTO>> GetAppointmentReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("Start date must be before end date.");
                }
                
                var appointments = await _appointmentService.GetAppointmentsInRangeAsync(startDate, endDate);
                
                // Calculate statistics
                var completedAppointments = appointments.Count(a => a.Status == "Completed");
                var cancelledAppointments = appointments.Count(a => a.Status == "Cancelled");
                var noShowAppointments = appointments.Count(a => a.Status == "No-Show");
                
                // Group by service
                var appointmentsByService = appointments
                    .GroupBy(a => a.ServiceName)
                    .Select(g => new AppointmentsByServiceDTO
                    {
                        ServiceName = g.Key,
                        Count = g.Count(),
                        CompletedCount = g.Count(a => a.Status == "Completed"),
                        CancelledCount = g.Count(a => a.Status == "Cancelled"),
                        NoShowCount = g.Count(a => a.Status == "No-Show")
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList();
                
                // Group by therapist
                var appointmentsByTherapist = appointments
                    .GroupBy(a => new { a.TherapistId, a.TherapistName })
                    .Select(g => new AppointmentsByTherapistDTO
                    {
                        TherapistId = g.Key.TherapistId,
                        TherapistName = g.Key.TherapistName,
                        Count = g.Count(),
                        CompletedCount = g.Count(a => a.Status == "Completed"),
                        CancelledCount = g.Count(a => a.Status == "Cancelled"),
                        NoShowCount = g.Count(a => a.Status == "No-Show")
                    })
                    .OrderByDescending(t => t.Count)
                    .ToList();
                
                var report = new AppointmentReportDTO
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalAppointments = appointments.Count,
                    CompletedAppointments = completedAppointments,
                    CancelledAppointments = cancelledAppointments,
                    NoShowAppointments = noShowAppointments,
                    AppointmentsByService = appointmentsByService,
                    AppointmentsByTherapist = appointmentsByTherapist
                };
                
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment report for date range {StartDate} to {EndDate}", 
                    startDate, endDate);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets revenue statistics for a specified date range
        /// </summary>
        /// <param name="startDate">Start date for the statistics</param>
        /// <param name="endDate">End date for the statistics</param>
        /// <returns>Revenue statistics for the specified date range</returns>
        [HttpGet("reports/revenue")]
        public async Task<ActionResult<RevenueReportDTO>> GetRevenueReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("Start date must be before end date.");
                }
                
                var appointments = await _appointmentService.GetAppointmentsInRangeAsync(startDate, endDate);
                
                // Filter only completed appointments for revenue
                var completedAppointments = appointments
                    .Where(a => a.Status == "Completed")
                    .ToList();
                
                // Calculate total revenue
                var totalRevenue = completedAppointments.Sum(a => a.Price);
                
                // Group by service
                var revenueByService = completedAppointments
                    .GroupBy(a => a.ServiceName)
                    .Select(g => new RevenueByServiceDTO
                    {
                        ServiceName = g.Key,
                        AppointmentCount = g.Count(),
                        Revenue = g.Sum(a => a.Price)
                    })
                    .OrderByDescending(s => s.Revenue)
                    .ToList();
                
                // Group by therapist
                var revenueByTherapist = completedAppointments
                    .GroupBy(a => new { a.TherapistId, a.TherapistName })
                    .Select(g => new RevenueByTherapistDTO
                    {
                        TherapistId = g.Key.TherapistId,
                        TherapistName = g.Key.TherapistName,
                        AppointmentCount = g.Count(),
                        Revenue = g.Sum(a => a.Price)
                    })
                    .OrderByDescending(t => t.Revenue)
                    .ToList();
                
                // Group by month if the date range is longer than a month
                List<RevenueByPeriodDTO> revenueByPeriod = null;
                var monthDiff = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;
                
                if (monthDiff > 1)
                {
                    // Group by month
                    revenueByPeriod = completedAppointments
                        .GroupBy(a => new DateTime(a.StartTime.Year, a.StartTime.Month, 1))
                        .Select(g => new RevenueByPeriodDTO
                        {
                            Period = g.Key.ToString("MMM yyyy"),
                            AppointmentCount = g.Count(),
                            Revenue = g.Sum(a => a.Price)
                        })
                        .OrderBy(p => DateTime.ParseExact(p.Period, "MMM yyyy", null))
                        .ToList();
                }
                else
                {
                    // Group by day
                    revenueByPeriod = completedAppointments
                        .GroupBy(a => a.StartTime.Date)
                        .Select(g => new RevenueByPeriodDTO
                        {
                            Period = g.Key.ToString("yyyy-MM-dd"),
                            AppointmentCount = g.Count(),
                            Revenue = g.Sum(a => a.Price)
                        })
                        .OrderBy(p => DateTime.ParseExact(p.Period, "yyyy-MM-dd", null))
                        .ToList();
                }
                
                var report = new RevenueReportDTO
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalAppointments = completedAppointments.Count,
                    TotalRevenue = totalRevenue,
                    AverageRevenuePerAppointment = completedAppointments.Count > 0 ? 
                        totalRevenue / completedAppointments.Count : 0,
                    RevenueByService = revenueByService,
                    RevenueByTherapist = revenueByTherapist,
                    RevenueByPeriod = revenueByPeriod
                };
                
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revenue report for date range {StartDate} to {EndDate}", 
                    startDate, endDate);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets the list of audit logs for administrative review
        /// </summary>
        /// <param name="startDate">Start date for audit logs</param>
        /// <param name="endDate">End date for audit logs</param>
        /// <param name="userId">Optional user ID to filter logs by</param>
        /// <param name="entityType">Optional entity type to filter logs by</param>
        /// <returns>List of audit logs matching the criteria</returns>
        [HttpGet("audit-logs")]
        public async Task<ActionResult<List<AuditLogDTO>>> GetAuditLogs(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] string entityType = null)
        {
            try
            {
                // This would be implemented with an actual audit log repository
                // For now, we'll return a placeholder message
                return Ok(new List<AuditLogDTO>
                {
                    new AuditLogDTO
                    {
                        AuditLogId = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        UserName = "admin@example.com",
                        EntityType = "Appointment",
                        EntityId = Guid.NewGuid(),
                        Action = "Create",
                        Timestamp = DateTime.UtcNow.AddHours(-1),
                        Details = "Created new appointment"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
} 
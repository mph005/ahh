using MassageBooking.API.Data.Repositories;
using MassageBooking.API.DTOs;
using MassageBooking.API.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MassageBooking.API.Services
{
    public class AdminService : IAdminService
    {
        private readonly IClientRepository _clientRepository;
        private readonly ITherapistRepository _therapistRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IClientRepository clientRepository,
            ITherapistRepository therapistRepository,
            IAppointmentRepository appointmentRepository,
            ILogger<AdminService> logger)
        {
            _clientRepository = clientRepository;
            _therapistRepository = therapistRepository;
            _appointmentRepository = appointmentRepository;
            _logger = logger;
        }

        public async Task<AdminDashboardStatsDTO> GetDashboardStatsAsync()
        {
            _logger.LogInformation("Calculating admin dashboard stats.");
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                // Fetch data sequentially to avoid DbContext concurrency issues
                var allClients = await _clientRepository.GetAllAsync();
                var allTherapists = await _therapistRepository.GetAllAsync();
                // Get appointments relevant for stats (today and future)
                var relevantAppointments = await _appointmentRepository.GetAppointmentsInRangeAsync(today, DateTime.UtcNow.AddYears(1));

                // Calculate stats
                var stats = new AdminDashboardStatsDTO
                {
                    TotalClients = allClients.Count(),
                    ActiveClients = allClients.Count(c => c.IsActive),
                    TotalTherapists = allTherapists.Count(),
                    ActiveTherapists = allTherapists.Count(t => t.IsActive),
                    UpcomingAppointments = relevantAppointments.Count(a => a.StartTime >= DateTime.UtcNow && a.Status == AppointmentStatus.Scheduled),
                    CompletedAppointmentsToday = relevantAppointments.Count(a => 
                        a.StartTime >= today && a.StartTime < tomorrow && a.Status == AppointmentStatus.Completed),
                    TotalRevenueToday = relevantAppointments
                        .Where(a => a.StartTime >= today && a.StartTime < tomorrow && a.Status == AppointmentStatus.Completed && a.Service != null)
                        .Sum(a => a.Service.Price) // Assumes Price is on the included Service entity
                };

                _logger.LogInformation("Admin stats calculated successfully.");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating admin dashboard stats.");
                // Depending on requirements, either throw or return a DTO indicating an error
                // Returning a new DTO with 0s might hide the error, so throwing is often better.
                throw; 
            }
        }
    }
} 
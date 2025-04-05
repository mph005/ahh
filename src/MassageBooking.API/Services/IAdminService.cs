using MassageBooking.API.DTOs;
using System.Threading.Tasks;

namespace MassageBooking.API.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardStatsDTO> GetDashboardStatsAsync();
    }
} 
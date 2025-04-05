using System;
using System.Collections.Generic;

namespace MassageBooking.API.DTOs
{
    /// <summary>
    /// DTO for admin dashboard statistics
    /// </summary>
    public class DashboardStatsDTO
    {
        /// <summary>
        /// Total number of appointments in the current month
        /// </summary>
        public int TotalAppointmentsThisMonth { get; set; }
        
        /// <summary>
        /// Number of completed appointments in the current month
        /// </summary>
        public int CompletedAppointmentsThisMonth { get; set; }
        
        /// <summary>
        /// Number of cancelled appointments in the current month
        /// </summary>
        public int CancelledAppointmentsThisMonth { get; set; }
        
        /// <summary>
        /// Number of upcoming appointments
        /// </summary>
        public int UpcomingAppointments { get; set; }
        
        /// <summary>
        /// Number of active therapists
        /// </summary>
        public int ActiveTherapistsCount { get; set; }
        
        /// <summary>
        /// Total number of clients
        /// </summary>
        public int TotalClientsCount { get; set; }
        
        /// <summary>
        /// Daily appointment counts for chart data
        /// </summary>
        public List<DailyAppointmentCountDTO> DailyAppointmentCounts { get; set; }
    }

    /// <summary>
    /// DTO for daily appointment count data
    /// </summary>
    public class DailyAppointmentCountDTO
    {
        /// <summary>
        /// The date
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// The number of appointments on that date
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// DTO for appointment report
    /// </summary>
    public class AppointmentReportDTO
    {
        /// <summary>
        /// Start date of the report period
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// End date of the report period
        /// </summary>
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Total number of appointments in the period
        /// </summary>
        public int TotalAppointments { get; set; }
        
        /// <summary>
        /// Number of completed appointments in the period
        /// </summary>
        public int CompletedAppointments { get; set; }
        
        /// <summary>
        /// Number of cancelled appointments in the period
        /// </summary>
        public int CancelledAppointments { get; set; }
        
        /// <summary>
        /// Number of no-show appointments in the period
        /// </summary>
        public int NoShowAppointments { get; set; }
        
        /// <summary>
        /// Appointment statistics grouped by service
        /// </summary>
        public List<AppointmentsByServiceDTO> AppointmentsByService { get; set; }
        
        /// <summary>
        /// Appointment statistics grouped by therapist
        /// </summary>
        public List<AppointmentsByTherapistDTO> AppointmentsByTherapist { get; set; }
    }

    /// <summary>
    /// DTO for appointments grouped by service
    /// </summary>
    public class AppointmentsByServiceDTO
    {
        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName { get; set; }
        
        /// <summary>
        /// Total appointment count for this service
        /// </summary>
        public int Count { get; set; }
        
        /// <summary>
        /// Number of completed appointments for this service
        /// </summary>
        public int CompletedCount { get; set; }
        
        /// <summary>
        /// Number of cancelled appointments for this service
        /// </summary>
        public int CancelledCount { get; set; }
        
        /// <summary>
        /// Number of no-show appointments for this service
        /// </summary>
        public int NoShowCount { get; set; }
    }

    /// <summary>
    /// DTO for appointments grouped by therapist
    /// </summary>
    public class AppointmentsByTherapistDTO
    {
        /// <summary>
        /// Therapist ID
        /// </summary>
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// Therapist name
        /// </summary>
        public string TherapistName { get; set; }
        
        /// <summary>
        /// Total appointment count for this therapist
        /// </summary>
        public int Count { get; set; }
        
        /// <summary>
        /// Number of completed appointments for this therapist
        /// </summary>
        public int CompletedCount { get; set; }
        
        /// <summary>
        /// Number of cancelled appointments for this therapist
        /// </summary>
        public int CancelledCount { get; set; }
        
        /// <summary>
        /// Number of no-show appointments for this therapist
        /// </summary>
        public int NoShowCount { get; set; }
    }

    /// <summary>
    /// DTO for revenue report
    /// </summary>
    public class RevenueReportDTO
    {
        /// <summary>
        /// Start date of the report period
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// End date of the report period
        /// </summary>
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Total number of appointments in the period
        /// </summary>
        public int TotalAppointments { get; set; }
        
        /// <summary>
        /// Total revenue in the period
        /// </summary>
        public decimal TotalRevenue { get; set; }
        
        /// <summary>
        /// Average revenue per appointment
        /// </summary>
        public decimal AverageRevenuePerAppointment { get; set; }
        
        /// <summary>
        /// Revenue by service
        /// </summary>
        public List<RevenueByServiceDTO> RevenueByService { get; set; }
        
        /// <summary>
        /// Revenue by therapist
        /// </summary>
        public List<RevenueByTherapistDTO> RevenueByTherapist { get; set; }
        
        /// <summary>
        /// Revenue by time period (day/month depending on report range)
        /// </summary>
        public List<RevenueByPeriodDTO> RevenueByPeriod { get; set; }
    }

    /// <summary>
    /// DTO for revenue grouped by service
    /// </summary>
    public class RevenueByServiceDTO
    {
        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName { get; set; }
        
        /// <summary>
        /// Number of appointments for this service
        /// </summary>
        public int AppointmentCount { get; set; }
        
        /// <summary>
        /// Total revenue from this service
        /// </summary>
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// DTO for revenue grouped by therapist
    /// </summary>
    public class RevenueByTherapistDTO
    {
        /// <summary>
        /// Therapist ID
        /// </summary>
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// Therapist name
        /// </summary>
        public string TherapistName { get; set; }
        
        /// <summary>
        /// Number of appointments for this therapist
        /// </summary>
        public int AppointmentCount { get; set; }
        
        /// <summary>
        /// Total revenue from this therapist
        /// </summary>
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// DTO for revenue grouped by time period
    /// </summary>
    public class RevenueByPeriodDTO
    {
        /// <summary>
        /// Period description (e.g., "Jan 2023" or "2023-01-15")
        /// </summary>
        public string Period { get; set; }
        
        /// <summary>
        /// Number of appointments in this period
        /// </summary>
        public int AppointmentCount { get; set; }
        
        /// <summary>
        /// Total revenue in this period
        /// </summary>
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// DTO for audit log records
    /// </summary>
    public class AuditLogDTO
    {
        /// <summary>
        /// Audit log ID
        /// </summary>
        public Guid AuditLogId { get; set; }
        
        /// <summary>
        /// User ID who performed the action
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// User name who performed the action
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Type of entity affected (Appointment, Therapist, Client, etc.)
        /// </summary>
        public string EntityType { get; set; }
        
        /// <summary>
        /// ID of the entity affected
        /// </summary>
        public Guid EntityId { get; set; }
        
        /// <summary>
        /// Action performed (Create, Update, Delete, etc.)
        /// </summary>
        public string Action { get; set; }
        
        /// <summary>
        /// When the action was performed
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Additional details about the action
        /// </summary>
        public string Details { get; set; }
    }

    public class AdminDashboardStatsDTO
    {
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int TotalTherapists { get; set; }
        public int ActiveTherapists { get; set; }
        public int UpcomingAppointments { get; set; }
        public int CompletedAppointmentsToday { get; set; }
        public decimal TotalRevenueToday { get; set; }
        // Add more stats as needed
    }
    
    // Add other Admin specific DTOs here later if needed
} 
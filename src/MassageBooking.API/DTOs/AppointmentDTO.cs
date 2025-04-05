using System;
using MassageBooking.API.Models;

namespace MassageBooking.API.DTOs
{
    public class ApiAppointmentDTO
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int TherapistId { get; set; }
        public string TherapistName { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public int ServiceDuration { get; set; }
    }
    
    public class ApiAppointmentCreateDTO
    {
        public int ClientId { get; set; }
        public int TherapistId { get; set; }
        public int ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public string? Notes { get; set; }
    }
    
    public class ApiAppointmentUpdateDTO
    {
        public DateTime? StartTime { get; set; }
        public string? Notes { get; set; }
        public AppointmentStatus? Status { get; set; }
    }
    
    public class ApiAppointmentRescheduleDTO
    {
        public int AppointmentId { get; set; }
        public DateTime NewStartTime { get; set; }
    }
    
    public class ApiAppointmentCancelDTO
    {
        public int AppointmentId { get; set; }
        public string? CancellationReason { get; set; }
    }
    
    public class ApiAvailableSlotDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TherapistId { get; set; }
        public string TherapistName { get; set; } = string.Empty;
    }
    
    public class ApiBookingResultDTO
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public ApiAppointmentDTO? Appointment { get; set; }
    }
    
    public class ApiRebookRequestDTO
    {
        public int PreviousAppointmentId { get; set; }
        public DateTime NewStartTime { get; set; }
    }
} 
using System;
using System.ComponentModel.DataAnnotations;

namespace MassageBooking.API.DTOs
{
    /// <summary>
    /// DTO for detailed appointment information
    /// </summary>
    public class AppointmentDetailsDTO
    {
        public Guid AppointmentId { get; set; }
        public Guid ClientId { get; set; }
        public string ClientName { get; set; }
        public Guid TherapistId { get; set; }
        public string TherapistName { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for appointment list item display
    /// </summary>
    public class AppointmentListItemDTO
    {
        public Guid AppointmentId { get; set; }
        public string TherapistName { get; set; }
        public string ServiceName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// DTO for available appointment time slot
    /// </summary>
    public class AvailableSlotDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid TherapistId { get; set; }
        public string TherapistName { get; set; }
        public int Duration { get; set; }
    }

    /// <summary>
    /// DTO for appointment booking request
    /// </summary>
    public class AppointmentBookingDTO
    {
        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public Guid TherapistId { get; set; }

        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO for appointment rescheduling request
    /// </summary>
    public class AppointmentRescheduleDTO
    {
        [Required]
        public Guid AppointmentId { get; set; }

        [Required]
        public DateTime NewStartTime { get; set; }
    }

    /// <summary>
    /// DTO for rebooking a previous appointment
    /// </summary>
    public class RebookRequestDTO
    {
        [Required]
        public Guid PreviousAppointmentId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO for booking operation result
    /// </summary>
    public class BookingResultDTO
    {
        public bool Success { get; set; }
        public Guid AppointmentId { get; set; }
        public string ErrorMessage { get; set; }
    }
} 
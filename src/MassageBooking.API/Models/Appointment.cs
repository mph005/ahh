using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MassageBooking.API.Models
{
    public class Appointment
    {
        [Key]
        [Required]
        public Guid AppointmentId { get; set; } = Guid.NewGuid(); // Public/Logical ID

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public Guid TherapistId { get; set; }

        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [MaxLength(50)] // Max length might not be needed for enum storage
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled; // Changed from string

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }

        // Navigation properties
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; } = null!;

        [ForeignKey("TherapistId")]
        public virtual Therapist Therapist { get; set; } = null!;

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; } = null!;
    }
    
    public enum AppointmentStatus
    {
        Scheduled,
        Completed,
        Cancelled,
        NoShow,
        Rescheduled
    }
} 
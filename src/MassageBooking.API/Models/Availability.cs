using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MassageBooking.API.Models
{
    /// <summary>
    /// Represents therapist availability
    /// </summary>
    public class Availability
    {
        /// <summary>
        /// The unique identifier for the availability record
        /// </summary>
        [Key]
        public Guid AvailabilityId { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// The ID of the therapist this availability is for
        /// </summary>
        [Required]
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// Specific date for one-time availabilities or overrides
        /// </summary>
        public DateTime? DateOverride { get; set; }
        
        /// <summary>
        /// For recurring availabilities (e.g., every Monday)
        /// </summary>
        public DayOfWeek? DayOfWeek { get; set; }
        
        /// <summary>
        /// Whether the therapist is available
        /// </summary>
        [Required]
        public bool IsAvailable { get; set; } = true;
        
        /// <summary>
        /// Start time of availability
        /// </summary>
        public TimeSpan? StartTime { get; set; }
        
        /// <summary>
        /// End time of availability
        /// </summary>
        public TimeSpan? EndTime { get; set; }
        
        /// <summary>
        /// Break start time of availability
        /// </summary>
        public TimeSpan? BreakStartTime { get; set; }
        
        /// <summary>
        /// Break end time of availability
        /// </summary>
        public TimeSpan? BreakEndTime { get; set; }
        
        /// <summary>
        /// Additional notes about the availability
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        /// <summary>
        /// When the availability record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When the availability record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Reference to the therapist
        /// </summary>
        [ForeignKey("TherapistId")]
        public virtual Therapist Therapist { get; set; } = null!;
        
        /// <summary>
        /// Whether the availability is recurring
        /// </summary>
        [NotMapped]
        public bool IsRecurring => DayOfWeek.HasValue && !DateOverride.HasValue;
    }
} 
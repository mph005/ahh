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
        public Guid AvailabilityId { get; set; }
        
        /// <summary>
        /// The ID of the therapist this availability is for
        /// </summary>
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// Reference to the therapist
        /// </summary>
        [ForeignKey("TherapistId")]
        public virtual Therapist Therapist { get; set; }
        
        /// <summary>
        /// Specific date this availability is for (null if recurring)
        /// </summary>
        public DateTime? SpecificDate { get; set; }
        
        /// <summary>
        /// Day of week this availability is for (used for recurring availability)
        /// </summary>
        public DayOfWeek? DayOfWeek { get; set; }
        
        /// <summary>
        /// Whether the therapist is available
        /// </summary>
        public bool IsAvailable { get; set; }
        
        /// <summary>
        /// Start time of availability
        /// </summary>
        public TimeSpan? StartTime { get; set; }
        
        /// <summary>
        /// End time of availability
        /// </summary>
        public TimeSpan? EndTime { get; set; }
        
        /// <summary>
        /// Start time of break period
        /// </summary>
        public TimeSpan? BreakStartTime { get; set; }
        
        /// <summary>
        /// End time of break period
        /// </summary>
        public TimeSpan? BreakEndTime { get; set; }
        
        /// <summary>
        /// Additional notes about the availability
        /// </summary>
        public string Notes { get; set; }
        
        /// <summary>
        /// When the availability record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the availability record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
} 
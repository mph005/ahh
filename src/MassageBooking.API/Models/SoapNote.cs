using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MassageBooking.API.Models
{
    /// <summary>
    /// Represents a SOAP note for a massage therapy appointment
    /// SOAP = Subjective, Objective, Assessment, Plan
    /// </summary>
    public class SoapNote
    {
        /// <summary>
        /// Unique identifier for the SOAP note (Primary Key)
        /// </summary>
        [Key]
        public Guid SoapNoteId { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// ID of the appointment this note is for
        /// </summary>
        [Required]
        public Guid AppointmentId { get; set; }
        
        /// <summary>
        /// Reference to the appointment
        /// </summary>
        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; } = null!;
        
        /// <summary>
        /// ID of the client associated with the appointment
        /// </summary>
        [Required]
        public Guid ClientId { get; set; }
        
        /// <summary>
        /// Reference to the client
        /// </summary>
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; } = null!;
        
        /// <summary>
        /// ID of the therapist who created this note
        /// </summary>
        [Required]
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// Reference to the therapist
        /// </summary>
        [ForeignKey("TherapistId")]
        public virtual Therapist Therapist { get; set; } = null!;
        
        /// <summary>
        /// When the note was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When the note was finalized (if applicable)
        /// </summary>
        public DateTime? FinalizedAt { get; set; }
        
        /// <summary>
        /// Subjective information - client's statements about their condition
        /// </summary>
        [MaxLength(2000)]
        public string? Subjective { get; set; }
        
        /// <summary>
        /// Objective information - therapist's observations and findings
        /// </summary>
        [MaxLength(2000)]
        public string? Objective { get; set; }
        
        /// <summary>
        /// Assessment - therapist's assessment of the client's condition
        /// </summary>
        [MaxLength(2000)]
        public string? Assessment { get; set; }
        
        /// <summary>
        /// Plan - treatment plan and recommendations
        /// </summary>
        [MaxLength(2000)]
        public string? Plan { get; set; }
        
        /// <summary>
        /// Areas of focus during the treatment
        /// </summary>
        [MaxLength(500)]
        public string? AreasOfFocus { get; set; }
        
        /// <summary>
        /// Techniques used during the treatment
        /// </summary>
        [MaxLength(500)]
        public string? TechniquesUsed { get; set; }
        
        /// <summary>
        /// Pressure level used during massage (e.g., scale 1-5)
        /// </summary>
        public int? PressureLevel { get; set; }
        
        /// <summary>
        /// Recommendations for the client
        /// </summary>
        [MaxLength(1000)]
        public string? Recommendations { get; set; }
        
        /// <summary>
        /// Therapist's signature
        /// </summary>
        [MaxLength(200)]
        public string? TherapistSignature { get; set; }
        
        /// <summary>
        /// Indicates whether the note is finalized
        /// </summary>
        public bool IsFinalized { get; set; } = false;
        
        /// <summary>
        /// When the note was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 
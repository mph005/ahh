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
        /// Unique identifier for the SOAP note
        /// </summary>
        [Key]
        public Guid SoapNoteId { get; set; }
        
        /// <summary>
        /// ID of the appointment this note is for
        /// </summary>
        public Guid AppointmentId { get; set; }
        
        /// <summary>
        /// Reference to the appointment
        /// </summary>
        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }
        
        /// <summary>
        /// ID of the therapist who created this note
        /// </summary>
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// Reference to the therapist
        /// </summary>
        [ForeignKey("TherapistId")]
        public virtual Therapist Therapist { get; set; }
        
        /// <summary>
        /// ID of the client this note is about
        /// </summary>
        public Guid ClientId { get; set; }
        
        /// <summary>
        /// Reference to the client
        /// </summary>
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }
        
        /// <summary>
        /// Subjective information - client's statements about their condition
        /// </summary>
        public string Subjective { get; set; }
        
        /// <summary>
        /// Objective information - therapist's observations and findings
        /// </summary>
        public string Objective { get; set; }
        
        /// <summary>
        /// Assessment - therapist's assessment of the client's condition
        /// </summary>
        public string Assessment { get; set; }
        
        /// <summary>
        /// Plan - treatment plan and recommendations
        /// </summary>
        public string Plan { get; set; }
        
        /// <summary>
        /// Areas of focus during the treatment
        /// </summary>
        public string AreasOfFocus { get; set; }
        
        /// <summary>
        /// Techniques used during the treatment
        /// </summary>
        public string TechniquesUsed { get; set; }
        
        /// <summary>
        /// Pressure level used (1-5, where 1 is lightest and 5 is deepest)
        /// </summary>
        public int? PressureLevel { get; set; }
        
        /// <summary>
        /// Whether the note has been finalized/locked
        /// </summary>
        public bool IsFinalized { get; set; }
        
        /// <summary>
        /// When the note was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the note was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// When the note was finalized (if applicable)
        /// </summary>
        public DateTime? FinalizedAt { get; set; }
    }
} 
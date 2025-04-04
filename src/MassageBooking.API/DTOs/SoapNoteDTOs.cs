using System;
using System.ComponentModel.DataAnnotations;

namespace MassageBooking.API.DTOs
{
    /// <summary>
    /// DTO for SOAP note details
    /// </summary>
    public class SoapNoteDTO
    {
        /// <summary>
        /// The SOAP note ID
        /// </summary>
        public Guid SoapNoteId { get; set; }
        
        /// <summary>
        /// The appointment ID
        /// </summary>
        public Guid AppointmentId { get; set; }
        
        /// <summary>
        /// The therapist ID
        /// </summary>
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// The therapist's name
        /// </summary>
        public string TherapistName { get; set; }
        
        /// <summary>
        /// The client ID
        /// </summary>
        public Guid ClientId { get; set; }
        
        /// <summary>
        /// The client's name
        /// </summary>
        public string ClientName { get; set; }
        
        /// <summary>
        /// The appointment date
        /// </summary>
        public DateTime AppointmentDate { get; set; }
        
        /// <summary>
        /// The service provided during the appointment
        /// </summary>
        public string ServiceName { get; set; }
        
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

    /// <summary>
    /// DTO for SOAP note list item
    /// </summary>
    public class SoapNoteListItemDTO
    {
        /// <summary>
        /// The SOAP note ID
        /// </summary>
        public Guid SoapNoteId { get; set; }
        
        /// <summary>
        /// The appointment ID
        /// </summary>
        public Guid AppointmentId { get; set; }
        
        /// <summary>
        /// The therapist ID
        /// </summary>
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// The therapist's name
        /// </summary>
        public string TherapistName { get; set; }
        
        /// <summary>
        /// The client ID
        /// </summary>
        public Guid ClientId { get; set; }
        
        /// <summary>
        /// The client's name
        /// </summary>
        public string ClientName { get; set; }
        
        /// <summary>
        /// The appointment date
        /// </summary>
        public DateTime AppointmentDate { get; set; }
        
        /// <summary>
        /// The service provided during the appointment
        /// </summary>
        public string ServiceName { get; set; }
        
        /// <summary>
        /// Whether the note has been finalized/locked
        /// </summary>
        public bool IsFinalized { get; set; }
        
        /// <summary>
        /// When the note was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a new SOAP note
    /// </summary>
    public class CreateSoapNoteDTO
    {
        /// <summary>
        /// The appointment ID
        /// </summary>
        [Required]
        public Guid AppointmentId { get; set; }
        
        /// <summary>
        /// The therapist ID
        /// </summary>
        [Required]
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// The client ID
        /// </summary>
        [Required]
        public Guid ClientId { get; set; }
        
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
    }

    /// <summary>
    /// DTO for updating an existing SOAP note
    /// </summary>
    public class UpdateSoapNoteDTO
    {
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
    }
} 
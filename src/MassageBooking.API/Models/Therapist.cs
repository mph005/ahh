using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MassageBooking.API.Models
{
    public class Therapist
    {
        [Key]
        [Required]
        public Guid TherapistId { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        [MaxLength(100)]
        public string? Title { get; set; }
        
        [MaxLength(1000)]
        public string? Bio { get; set; }
        
        [MaxLength(500)]
        public string? Specialties { get; set; }
        
        [MaxLength(200)]
        public string? ProfileImageUrl { get; set; }
        
        // Kept for potential legacy use, prefer ProfileImageUrl
        [MaxLength(200)] 
        public string? ImageUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<TherapistService> Services { get; set; } = new List<TherapistService>();
        public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<SoapNote> SoapNotes { get; set; } = new List<SoapNote>();
    }
} 
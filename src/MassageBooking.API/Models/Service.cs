using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MassageBooking.API.Models
{
    public class Service
    {
        [Key]
        [Required]
        public Guid ServiceId { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int DurationMinutes { get; set; }
        
        [NotMapped] // Alias, not stored in DB
        public int Duration 
        { 
            get => DurationMinutes; 
            set => DurationMinutes = value; 
        }
        
        [Required]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [MaxLength(200)]
        public string? ImageUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<TherapistService> Therapists { get; set; } = new List<TherapistService>();
    }
} 
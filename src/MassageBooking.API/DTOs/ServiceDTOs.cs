using System;
using System.ComponentModel.DataAnnotations;

namespace MassageBooking.API.DTOs
{
    // DTO for displaying service information (used for listing and details)
    public class ServiceDTO
    {
        public Guid ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } // Whether the service is currently offered
    }

    // DTO for creating a new service
    public class CreateServiceDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        [Required]
        [Range(15, 240)] // Example range for duration in minutes
        public int DurationMinutes { get; set; }
        [Required]
        [Range(0.01, 10000.00)] // Example price range
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true; // Default to active
    }

    // DTO for updating an existing service
    public class UpdateServiceDTO
    {
        [Required]
        public Guid ServiceId { get; set; } // Include ID for identification
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        [Required]
        [Range(15, 240)]
        public int DurationMinutes { get; set; }
        [Required]
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }
        [Required]
        public bool IsActive { get; set; }
    }
} 
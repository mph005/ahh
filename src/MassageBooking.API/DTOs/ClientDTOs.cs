using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MassageBooking.API.DTOs
{
    // DTO for listing clients (basic info)
    public class ClientListItemDTO
    {
        public Guid ClientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; } // Indicate if the client account is active
    }

    // DTO for detailed client view
    public class ClientDetailsDTO
    {
        public Guid ClientId { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Phone]
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } // Consider splitting into structured address fields if needed
        public string MedicalHistory { get; set; } // Summary or key notes
        public string Preferences { get; set; } // Massage preferences, pressure, etc.
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool IsActive { get; set; }
        // Could include a list of recent appointments DTOs if needed
        // public List<AppointmentSummaryDTO> RecentAppointments { get; set; }
    }

    // DTO for creating a new client
    public class CreateClientDTO
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Phone]
        public string Phone { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string MedicalHistory { get; set; }
        public string Preferences { get; set; }
        // Password setting might be handled separately (e.g., initial setup link)
    }

    // DTO for updating an existing client
    public class UpdateClientDTO
    {
        [Required]
        public Guid ClientId { get; set; } // Include ID for identification
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Phone]
        public string Phone { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string MedicalHistory { get; set; }
        public string Preferences { get; set; }
        public bool IsActive { get; set; }
    }
} 
using Microsoft.AspNetCore.Identity;
using System;

namespace MassageBooking.API.Models
{
    // Add any custom profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<Guid> // Use Guid as the primary key type
    {
        // Add custom properties here if needed in the future
        // public string? FirstName { get; set; }
        // public string? LastName { get; set; }
        // public Guid? AssociatedClientId { get; set; } // Link to Client
        // public Guid? AssociatedTherapistId { get; set; } // Link to Therapist
    }
} 
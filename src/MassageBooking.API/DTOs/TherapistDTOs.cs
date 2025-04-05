using System;
using System.ComponentModel.DataAnnotations;

namespace MassageBooking.API.DTOs
{
    /// <summary>
    /// DTO for detailed therapist information
    /// </summary>
    public class TherapistDetailsDTO
    {
        /// <summary>
        /// The therapist's unique identifier
        /// </summary>
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// The therapist's first name
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// The therapist's last name
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// The therapist's full name (first + last)
        /// </summary>
        public string FullName { get; set; }
        
        /// <summary>
        /// The therapist's email address
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// The therapist's phone number
        /// </summary>
        public string Phone { get; set; }
        
        /// <summary>
        /// The therapist's professional title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Biographical information about the therapist
        /// </summary>
        public string Bio { get; set; }
        
        /// <summary>
        /// URL to the therapist's profile image
        /// </summary>
        public string ImageUrl { get; set; }
        
        /// <summary>
        /// Whether the therapist is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// When the therapist record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the therapist record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for therapist list display
    /// </summary>
    public class TherapistListItemDTO
    {
        /// <summary>
        /// The therapist's unique identifier
        /// </summary>
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// The therapist's first name
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// The therapist's last name
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// The therapist's full name (first + last)
        /// </summary>
        public string FullName { get; set; }
        
        /// <summary>
        /// The therapist's professional title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// URL to the therapist's profile image
        /// </summary>
        public string ImageUrl { get; set; }
        
        /// <summary>
        /// Whether the therapist is active
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for creating a new therapist
    /// </summary>
    public class CreateTherapistDTO
    {
        /// <summary>
        /// The therapist's first name
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        
        /// <summary>
        /// The therapist's last name
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        
        /// <summary>
        /// The therapist's email address
        /// </summary>
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        
        /// <summary>
        /// The therapist's phone number
        /// </summary>
        [Phone]
        [MaxLength(20)]
        public string Phone { get; set; }
        
        /// <summary>
        /// The therapist's professional title
        /// </summary>
        [MaxLength(100)]
        public string Title { get; set; }
        
        /// <summary>
        /// Biographical information about the therapist
        /// </summary>
        public string Bio { get; set; }
        
        /// <summary>
        /// URL to the therapist's profile image
        /// </summary>
        public string ImageUrl { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing therapist
    /// </summary>
    public class UpdateTherapistDTO
    {
        /// <summary>
        /// The therapist's unique identifier
        /// </summary>
        [Required]
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// The therapist's first name
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// The therapist's last name
        /// </summary>
        [MaxLength(50)]
        public string LastName { get; set; }
        
        /// <summary>
        /// The therapist's email address
        /// </summary>
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        
        /// <summary>
        /// The therapist's phone number
        /// </summary>
        [Phone]
        [MaxLength(20)]
        public string Phone { get; set; }
        
        /// <summary>
        /// The therapist's professional title
        /// </summary>
        [MaxLength(100)]
        public string Title { get; set; }
        
        /// <summary>
        /// Biographical information about the therapist
        /// </summary>
        public string Bio { get; set; }
        
        /// <summary>
        /// URL to the therapist's profile image
        /// </summary>
        public string ImageUrl { get; set; }
        
        /// <summary>
        /// Whether the therapist is active
        /// </summary>
        public bool? IsActive { get; set; }
    }
} 
using System;

namespace MassageBooking.API.Models
{
    /// <summary>
    /// Represents an available time slot for booking
    /// </summary>
    public class AvailableSlot
    {
        /// <summary>
        /// The start time of the available slot
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// The end time of the available slot
        /// </summary>
        public DateTime EndTime { get; set; }
        
        /// <summary>
        /// The ID of the therapist available for this slot
        /// </summary>
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// The name of the therapist available for this slot
        /// </summary>
        public string TherapistName { get; set; }
    }
} 
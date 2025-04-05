using System;

namespace MassageBooking.API.Models
{
    /// <summary>
    /// Represents an available time slot for booking appointments
    /// </summary>
    public class TimeSlot
    {
        /// <summary>
        /// The start time of the slot
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// The end time of the slot
        /// </summary>
        public DateTime EndTime { get; set; }
        
        /// <summary>
        /// The therapist ID associated with this slot
        /// </summary>
        public int TherapistId { get; set; }
        
        /// <summary>
        /// The therapist's name
        /// </summary>
        public string TherapistName { get; set; }
        
        /// <summary>
        /// The service ID that can be booked during this slot
        /// </summary>
        public int ServiceId { get; set; }
        
        /// <summary>
        /// The service name
        /// </summary>
        public string ServiceName { get; set; }
        
        /// <summary>
        /// The duration of the service in minutes
        /// </summary>
        public int ServiceDuration { get; set; }
    }
} 
using System;

namespace MassageBooking.API.Models
{
    public class TherapistService
    {
        public Guid TherapistId { get; set; }
        public Therapist Therapist { get; set; }
        
        public Guid ServiceId { get; set; }
        public Service Service { get; set; }
    }
} 
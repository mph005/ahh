using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MassageBooking.API.DTOs
{
    /// <summary>
    /// DTO for daily schedule information
    /// </summary>
    public class DailyScheduleDTO
    {
        /// <summary>
        /// The date of the schedule
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Optional therapist ID if filtering by a specific therapist
        /// </summary>
        public Guid? TherapistId { get; set; }
        
        /// <summary>
        /// Therapist name if filtering by a specific therapist
        /// </summary>
        public string TherapistName { get; set; }
        
        /// <summary>
        /// List of time slots in the schedule
        /// </summary>
        public List<ScheduleTimeSlotDTO> TimeSlots { get; set; }
    }

    /// <summary>
    /// DTO for a time slot in the schedule
    /// </summary>
    public class ScheduleTimeSlotDTO
    {
        /// <summary>
        /// The appointment ID for booked slots, null for available slots
        /// </summary>
        public Guid? AppointmentId { get; set; }
        
        /// <summary>
        /// Client name for booked slots
        /// </summary>
        public string ClientName { get; set; }
        
        /// <summary>
        /// Service name for booked slots
        /// </summary>
        public string ServiceName { get; set; }
        
        /// <summary>
        /// Therapist name
        /// </summary>
        public string TherapistName { get; set; }
        
        /// <summary>
        /// Start time of the slot
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// End time of the slot
        /// </summary>
        public DateTime EndTime { get; set; }
        
        /// <summary>
        /// Status of the appointment (Scheduled, Completed, Cancelled, etc.)
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// DTO for therapist availability information
    /// </summary>
    public class TherapistAvailabilityDTO
    {
        /// <summary>
        /// The therapist ID
        /// </summary>
        public Guid TherapistId { get; set; }
        
        /// <summary>
        /// The therapist's full name
        /// </summary>
        public string TherapistName { get; set; }
        
        /// <summary>
        /// The date of the availability
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// The day of week of the availability
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }
        
        /// <summary>
        /// Whether the therapist is available on this day
        /// </summary>
        public bool IsAvailable { get; set; }
        
        /// <summary>
        /// Start time of availability (if available)
        /// </summary>
        public TimeSpan? StartTime { get; set; }
        
        /// <summary>
        /// End time of availability (if available)
        /// </summary>
        public TimeSpan? EndTime { get; set; }
        
        /// <summary>
        /// Break start time (if any)
        /// </summary>
        public TimeSpan? BreakStartTime { get; set; }
        
        /// <summary>
        /// Break end time (if any)
        /// </summary>
        public TimeSpan? BreakEndTime { get; set; }
        
        /// <summary>
        /// Notes about the availability
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating therapist availability
    /// </summary>
    public class UpdateAvailabilityRequestDTO
    {
        /// <summary>
        /// The specific date to update (null if updating a recurring day of week)
        /// </summary>
        public DateTime? Date { get; set; }
        
        /// <summary>
        /// The day of week to update (ignored if Date is provided)
        /// </summary>
        public DayOfWeek? DayOfWeek { get; set; }
        
        /// <summary>
        /// Whether the therapist is available
        /// </summary>
        [Required]
        public bool IsAvailable { get; set; }
        
        /// <summary>
        /// Start time of availability
        /// </summary>
        public TimeSpan? StartTime { get; set; }
        
        /// <summary>
        /// End time of availability
        /// </summary>
        public TimeSpan? EndTime { get; set; }
        
        /// <summary>
        /// Break start time (if any)
        /// </summary>
        public TimeSpan? BreakStartTime { get; set; }
        
        /// <summary>
        /// Break end time (if any)
        /// </summary>
        public TimeSpan? BreakEndTime { get; set; }
        
        /// <summary>
        /// Notes about the availability
        /// </summary>
        public string Notes { get; set; }
        
        /// <summary>
        /// Whether this is a recurring change (only applicable when Date is provided)
        /// </summary>
        public bool IsRecurring { get; set; }
    }

    /// <summary>
    /// DTO for blocking time on a therapist's schedule
    /// </summary>
    public class BlockTimeRequestDTO
    {
        /// <summary>
        /// The start date and time of the blocked period
        /// </summary>
        [Required]
        public DateTime StartDateTime { get; set; }
        
        /// <summary>
        /// The end date and time of the blocked period
        /// </summary>
        [Required]
        public DateTime EndDateTime { get; set; }
        
        /// <summary>
        /// The reason for blocking the time
        /// </summary>
        public string Reason { get; set; }
        
        /// <summary>
        /// Whether to allow overriding existing appointments
        /// </summary>
        public bool OverrideExistingAppointments { get; set; }
    }

    /// <summary>
    /// DTO for operation results
    /// </summary>
    public class OperationResultDTO
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Error message if the operation failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }
} 
using System.Threading.Tasks;
using MassageBooking.API.Models;

namespace MassageBooking.API.Services
{
    /// <summary>
    /// Interface for email notification services
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an appointment confirmation email to the client
        /// </summary>
        /// <param name="appointment">Appointment information</param>
        /// <param name="client">Client information</param>
        /// <param name="therapist">Therapist information</param>
        /// <param name="service">Service information</param>
        /// <returns>True if the email was sent successfully, false otherwise</returns>
        Task<bool> SendAppointmentConfirmationAsync(Appointment appointment, Client client, Therapist therapist, Service service);
        
        /// <summary>
        /// Sends an appointment reminder email to the client
        /// </summary>
        /// <param name="appointment">Appointment information</param>
        /// <param name="client">Client information</param>
        /// <param name="therapist">Therapist information</param>
        /// <param name="service">Service information</param>
        /// <returns>True if the email was sent successfully, false otherwise</returns>
        Task<bool> SendAppointmentReminderAsync(Appointment appointment, Client client, Therapist therapist, Service service);
        
        /// <summary>
        /// Sends an appointment cancellation email to the client
        /// </summary>
        /// <param name="appointment">Appointment information</param>
        /// <param name="client">Client information</param>
        /// <param name="therapist">Therapist information</param>
        /// <param name="service">Service information</param>
        /// <returns>True if the email was sent successfully, false otherwise</returns>
        Task<bool> SendAppointmentCancellationAsync(Appointment appointment, Client client, Therapist therapist, Service service);
        
        /// <summary>
        /// Sends a notification to the therapist about a new appointment
        /// </summary>
        /// <param name="appointment">Appointment information</param>
        /// <param name="client">Client information</param>
        /// <param name="therapist">Therapist information</param>
        /// <param name="service">Service information</param>
        /// <returns>True if the email was sent successfully, false otherwise</returns>
        Task<bool> SendTherapistAppointmentNotificationAsync(Appointment appointment, Client client, Therapist therapist, Service service);
        
        /// <summary>
        /// Sends a password reset email
        /// </summary>
        /// <param name="email">Recipient email</param>
        /// <param name="resetToken">Password reset token</param>
        /// <returns>True if the email was sent successfully, false otherwise</returns>
        Task<bool> SendPasswordResetAsync(string email, string resetToken);
        
        /// <summary>
        /// Sends a welcome email to a new client
        /// </summary>
        /// <param name="client">Client information</param>
        /// <returns>True if the email was sent successfully, false otherwise</returns>
        Task<bool> SendWelcomeEmailAsync(Client client);
    }
} 
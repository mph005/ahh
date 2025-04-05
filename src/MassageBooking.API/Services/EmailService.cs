using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MassageBooking.API.Models;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MassageBooking.API.Services
{
    /// <summary>
    /// Service for sending email notifications
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<bool> SendAppointmentConfirmationAsync(Appointment appointment, Client client, Therapist therapist, Service service)
        {
            // In a real implementation, this would send an email using SMTP
            _logger.LogInformation(
                "SIMULATED EMAIL: Appointment confirmation sent to {ClientEmail}. " +
                "Appointment with {TherapistName} for {ServiceName} on {AppointmentDate} at {AppointmentTime}",
                client.Email,
                $"{therapist.FirstName} {therapist.LastName}",
                service.Name,
                appointment.StartTime.ToShortDateString(),
                appointment.StartTime.ToShortTimeString());
            
            await Task.CompletedTask; // Simulate async operation
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> SendAppointmentReminderAsync(Appointment appointment, Client client, Therapist therapist, Service service)
        {
            // In a real implementation, this would send an email using SMTP
            _logger.LogInformation(
                "SIMULATED EMAIL: Appointment reminder sent to {ClientEmail}. " +
                "Appointment with {TherapistName} for {ServiceName} on {AppointmentDate} at {AppointmentTime}",
                client.Email,
                $"{therapist.FirstName} {therapist.LastName}",
                service.Name,
                appointment.StartTime.ToShortDateString(),
                appointment.StartTime.ToShortTimeString());
            
            await Task.CompletedTask; // Simulate async operation
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> SendAppointmentCancellationAsync(Appointment appointment, Client client, Therapist therapist, Service service)
        {
            // In a real implementation, this would send an email using SMTP
            _logger.LogInformation(
                "SIMULATED EMAIL: Appointment cancellation notice sent to {ClientEmail}. " +
                "Cancelled appointment with {TherapistName} for {ServiceName} on {AppointmentDate} at {AppointmentTime}",
                client.Email,
                $"{therapist.FirstName} {therapist.LastName}",
                service.Name,
                appointment.StartTime.ToShortDateString(),
                appointment.StartTime.ToShortTimeString());
            
            await Task.CompletedTask; // Simulate async operation
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> SendTherapistAppointmentNotificationAsync(Appointment appointment, Client client, Therapist therapist, Service service)
        {
            try
            {
                var subject = "New Appointment Scheduled";
                
                var body = new StringBuilder();
                body.AppendLine($"<h2>New Appointment Notification</h2>");
                body.AppendLine($"<p>Dear {therapist.FirstName},</p>");
                body.AppendLine($"<p>A new appointment has been scheduled with you:</p>");
                body.AppendLine($"<ul>");
                body.AppendLine($"<li><strong>Client:</strong> {client.FirstName} {client.LastName}</li>");
                body.AppendLine($"<li><strong>Service:</strong> {service.Name}</li>");
                body.AppendLine($"<li><strong>Date:</strong> {appointment.StartTime:dddd, MMMM d, yyyy}</li>");
                body.AppendLine($"<li><strong>Time:</strong> {appointment.StartTime:h:mm tt} - {appointment.EndTime:h:mm tt}</li>");
                body.AppendLine($"<li><strong>Client Phone:</strong> {client.Phone}</li>");
                body.AppendLine($"<li><strong>Client Email:</strong> {client.Email}</li>");
                body.AppendLine($"<li><strong>Notes:</strong> {appointment.Notes ?? "None"}</li>");
                body.AppendLine($"</ul>");
                body.AppendLine($"<p>Please log in to the system to view full details.</p>");
                body.AppendLine($"<p>Thank you,<br>Massage Therapy Booking System</p>");
                
                return await SendEmailAsync(therapist.Email, subject, body.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending appointment notification email to therapist {TherapistId}", therapist.TherapistId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendPasswordResetAsync(string email, string resetToken)
        {
            // In a real implementation, this would send an email using SMTP
            _logger.LogInformation(
                "SIMULATED EMAIL: Password reset email sent to {Email}. " +
                "Token: {ResetToken}",
                email,
                resetToken);
            
            await Task.CompletedTask; // Simulate async operation
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> SendWelcomeEmailAsync(Client client)
        {
            // In a real implementation, this would send an email using SMTP
            _logger.LogInformation(
                "SIMULATED EMAIL: Welcome email sent to {ClientEmail}. " +
                "Welcome to our massage booking system, {ClientName}!",
                client.Email,
                $"{client.FirstName} {client.LastName}");
            
            await Task.CompletedTask; // Simulate async operation
            return true;
        }

        /// <inheritdoc />
        public async Task SendAppointmentRescheduleAsync(Appointment appointment, Client client, Therapist therapist, Service service)
        {
            // In a real implementation, this would send an email using SMTP
            _logger.LogInformation(
                "SIMULATED EMAIL: Appointment reschedule notice sent to {ClientEmail}. " +
                "Rescheduled appointment with {TherapistName} for {ServiceName} to {AppointmentDate} at {AppointmentTime}",
                client.Email,
                $"{therapist.FirstName} {therapist.LastName}",
                service.Name,
                appointment.StartTime.ToShortDateString(),
                appointment.StartTime.ToShortTimeString());
            
            await Task.CompletedTask; // Simulate async operation
        }

        /// <inheritdoc />
        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            // This method is essentially a duplicate of SendPasswordResetAsync, but with a different return type
            // In a real implementation, this would send an email using SMTP
            _logger.LogInformation(
                "SIMULATED EMAIL: Password reset email sent to {Email}. " +
                "Token: {ResetToken}",
                email,
                resetToken);
            
            await Task.CompletedTask; // Simulate async operation
        }

        #region Private Methods

        private async Task<bool> SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                if (string.IsNullOrEmpty(to))
                {
                    _logger.LogWarning("Attempted to send email with empty recipient");
                    return false;
                }

                // Create message
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                
                message.To.Add(new MailAddress(to));

                // Configure SMTP client
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
                };

                // Send email
                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {Recipient} with subject: {Subject}", to, subject);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Recipient} with subject: {Subject}", to, subject);
                return false;
            }
        }

        #endregion
    }
} 
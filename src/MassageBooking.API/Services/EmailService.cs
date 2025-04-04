using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MassageBooking.API.Models;
using MassageBooking.API.Configuration;
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

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings?.Value ?? throw new ArgumentNullException(nameof(emailSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> SendAppointmentConfirmationAsync(Appointment appointment, Client client, Therapist therapist, Service service)
        {
            try
            {
                var subject = "Your Massage Therapy Appointment Confirmation";
                
                var body = new StringBuilder();
                body.AppendLine($"<h2>Appointment Confirmation</h2>");
                body.AppendLine($"<p>Dear {client.FirstName},</p>");
                body.AppendLine($"<p>Your appointment has been confirmed:</p>");
                body.AppendLine($"<ul>");
                body.AppendLine($"<li><strong>Service:</strong> {service.Name}</li>");
                body.AppendLine($"<li><strong>Therapist:</strong> {therapist.FirstName} {therapist.LastName}</li>");
                body.AppendLine($"<li><strong>Date:</strong> {appointment.StartTime:dddd, MMMM d, yyyy}</li>");
                body.AppendLine($"<li><strong>Time:</strong> {appointment.StartTime:h:mm tt} - {appointment.EndTime:h:mm tt}</li>");
                body.AppendLine($"<li><strong>Duration:</strong> {service.Duration} minutes</li>");
                body.AppendLine($"<li><strong>Price:</strong> ${service.Price}</li>");
                body.AppendLine($"</ul>");
                body.AppendLine($"<p>Location: 123 Relaxation Street, Wellness City</p>");
                body.AppendLine($"<p>Please arrive 10 minutes early to complete any necessary paperwork.</p>");
                body.AppendLine($"<h3>Cancellation Policy</h3>");
                body.AppendLine($"<p>If you need to cancel or reschedule, please do so at least 24 hours in advance to avoid a cancellation fee.</p>");
                body.AppendLine($"<p>Thank you for choosing our services!</p>");
                body.AppendLine($"<p>Warm regards,<br>Massage Therapy Booking Team</p>");
                
                return await SendEmailAsync(client.Email, subject, body.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending appointment confirmation email to client {ClientId}", client.ClientId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendAppointmentReminderAsync(Appointment appointment, Client client, Therapist therapist, Service service)
        {
            try
            {
                var subject = "Reminder: Your Upcoming Massage Therapy Appointment";
                
                var body = new StringBuilder();
                body.AppendLine($"<h2>Appointment Reminder</h2>");
                body.AppendLine($"<p>Dear {client.FirstName},</p>");
                body.AppendLine($"<p>This is a friendly reminder of your upcoming appointment:</p>");
                body.AppendLine($"<ul>");
                body.AppendLine($"<li><strong>Service:</strong> {service.Name}</li>");
                body.AppendLine($"<li><strong>Therapist:</strong> {therapist.FirstName} {therapist.LastName}</li>");
                body.AppendLine($"<li><strong>Date:</strong> {appointment.StartTime:dddd, MMMM d, yyyy}</li>");
                body.AppendLine($"<li><strong>Time:</strong> {appointment.StartTime:h:mm tt} - {appointment.EndTime:h:mm tt}</li>");
                body.AppendLine($"</ul>");
                body.AppendLine($"<p>Location: 123 Relaxation Street, Wellness City</p>");
                body.AppendLine($"<p>Please arrive 10 minutes early. If you need to cancel or reschedule, please do so at least 24 hours in advance.</p>");
                body.AppendLine($"<p>We look forward to seeing you!</p>");
                body.AppendLine($"<p>Warm regards,<br>Massage Therapy Booking Team</p>");
                
                return await SendEmailAsync(client.Email, subject, body.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending appointment reminder email to client {ClientId}", client.ClientId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendAppointmentCancellationAsync(Appointment appointment, Client client, Therapist therapist, Service service)
        {
            try
            {
                var subject = "Your Massage Therapy Appointment Has Been Cancelled";
                
                var body = new StringBuilder();
                body.AppendLine($"<h2>Appointment Cancellation</h2>");
                body.AppendLine($"<p>Dear {client.FirstName},</p>");
                body.AppendLine($"<p>Your appointment has been cancelled:</p>");
                body.AppendLine($"<ul>");
                body.AppendLine($"<li><strong>Service:</strong> {service.Name}</li>");
                body.AppendLine($"<li><strong>Therapist:</strong> {therapist.FirstName} {therapist.LastName}</li>");
                body.AppendLine($"<li><strong>Date:</strong> {appointment.StartTime:dddd, MMMM d, yyyy}</li>");
                body.AppendLine($"<li><strong>Time:</strong> {appointment.StartTime:h:mm tt} - {appointment.EndTime:h:mm tt}</li>");
                body.AppendLine($"</ul>");
                body.AppendLine($"<p>If you did not request this cancellation, please contact us immediately.</p>");
                body.AppendLine($"<p>To book a new appointment, please visit our website or call us.</p>");
                body.AppendLine($"<p>Thank you for your understanding.</p>");
                body.AppendLine($"<p>Warm regards,<br>Massage Therapy Booking Team</p>");
                
                return await SendEmailAsync(client.Email, subject, body.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending appointment cancellation email to client {ClientId}", client.ClientId);
                return false;
            }
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
            try
            {
                var subject = "Password Reset Request";
                
                var resetLink = $"{_emailSettings.WebsiteBaseUrl}/reset-password?token={resetToken}&email={WebUtility.UrlEncode(email)}";
                
                var body = new StringBuilder();
                body.AppendLine($"<h2>Password Reset Request</h2>");
                body.AppendLine($"<p>We received a request to reset your password.</p>");
                body.AppendLine($"<p>Click the link below to reset your password:</p>");
                body.AppendLine($"<p><a href=\"{resetLink}\">Reset Password</a></p>");
                body.AppendLine($"<p>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>");
                body.AppendLine($"<p>This link will expire in 1 hour.</p>");
                body.AppendLine($"<p>Thank you,<br>Massage Therapy Booking Team</p>");
                
                return await SendEmailAsync(email, subject, body.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendWelcomeEmailAsync(Client client)
        {
            try
            {
                var subject = "Welcome to Massage Therapy Booking";
                
                var body = new StringBuilder();
                body.AppendLine($"<h2>Welcome to Massage Therapy Booking!</h2>");
                body.AppendLine($"<p>Dear {client.FirstName},</p>");
                body.AppendLine($"<p>Thank you for creating an account with us. We're excited to help you on your wellness journey!</p>");
                body.AppendLine($"<p>With your new account, you can:</p>");
                body.AppendLine($"<ul>");
                body.AppendLine($"<li>Book appointments online</li>");
                body.AppendLine($"<li>View your appointment history</li>");
                body.AppendLine($"<li>Manage your profile information</li>");
                body.AppendLine($"<li>Receive appointment reminders</li>");
                body.AppendLine($"</ul>");
                body.AppendLine($"<p>If you have any questions or need assistance, please don't hesitate to contact us.</p>");
                body.AppendLine($"<p>We look forward to seeing you soon!</p>");
                body.AppendLine($"<p>Warm regards,<br>Massage Therapy Booking Team</p>");
                
                return await SendEmailAsync(client.Email, subject, body.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to client {ClientId}", client.ClientId);
                return false;
            }
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
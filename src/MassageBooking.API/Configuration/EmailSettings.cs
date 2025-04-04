namespace MassageBooking.API.Configuration
{
    /// <summary>
    /// Settings for email configuration
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// SMTP server address
        /// </summary>
        public string SmtpServer { get; set; }
        
        /// <summary>
        /// SMTP port
        /// </summary>
        public int SmtpPort { get; set; }
        
        /// <summary>
        /// SMTP username
        /// </summary>
        public string SmtpUsername { get; set; }
        
        /// <summary>
        /// SMTP password
        /// </summary>
        public string SmtpPassword { get; set; }
        
        /// <summary>
        /// Whether to use SSL for SMTP connection
        /// </summary>
        public bool EnableSsl { get; set; }
        
        /// <summary>
        /// Sender email address
        /// </summary>
        public string SenderEmail { get; set; }
        
        /// <summary>
        /// Sender display name
        /// </summary>
        public string SenderName { get; set; }
        
        /// <summary>
        /// Base URL of the website (used for links in emails)
        /// </summary>
        public string WebsiteBaseUrl { get; set; }
    }
} 
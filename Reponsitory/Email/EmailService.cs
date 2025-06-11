namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailConfirmationAsync(string email, string token)
        {
            var subject = "Confirm your email";
            var message = $"Please confirm your account by clicking this link: <a href='#'>Confirm Email</a>";
            await SendEmailAsync(email, subject, message);
        }

        public async Task SendPasswordResetAsync(string email, string token)
        {
            var subject = "Reset your password";
            var message = $"Please reset your password by clicking this link: <a href='#'>Reset Password</a>";
            await SendEmailAsync(email, subject, message);
        }

        public async Task SendNotificationAsync(string email, string subject, string message)
        {
            await SendEmailAsync(email, subject, message);
        }

        private async Task SendEmailAsync(string email, string subject, string message)
        {
            // Implementation depends on your email provider (SendGrid, SMTP, etc.)
            _logger.LogInformation($"Sending email to {email} with subject: {subject}");
            await Task.CompletedTask;
        }
    }
}

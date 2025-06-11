namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Email
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string token);
        Task SendPasswordResetAsync(string email, string token);
        Task SendNotificationAsync(string email, string subject, string message);
    }
}

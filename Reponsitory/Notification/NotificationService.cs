using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Hubs;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Communication;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Notificationtype
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string userId, string title, string message, NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification via SignalR
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }

        public async Task SendBulkNotificationAsync(List<string> userIds, string title, string message, NotificationType type)
        {
            var notifications = userIds.Select(userId => new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // Send real-time notifications
            foreach (var userId in userIds)
            {
                var notification = notifications.First(n => n.UserId == userId);
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
            }
        }

        public async Task<List<NotificationType>> GetUserNotificationsAsync(string userId, int count = 10)
        {
            return await _context.Notifications
              .Where(n => n.UserId == userId)
              .OrderByDescending(n => n.CreatedAt)
              .Select(n => n.Type)
              .Take(count)
              .ToListAsync();
        }
    }

}

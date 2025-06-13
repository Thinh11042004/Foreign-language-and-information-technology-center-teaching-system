using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly ILogger<BaseController> _logger;

        public BaseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<BaseController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        protected async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        protected async Task LogActivityAsync(string action, string entityType, string entityId = null, string oldValues = null, string newValues = null)
        {
            var user = await GetCurrentUserAsync();
            var auditLog = new AuditLog
            {
                UserId = user?.Id,
                Action = action,
                EntityType = entityType,
                EntityId = entityId ?? string.Empty,
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                OldValues = oldValues ?? string.Empty,
                NewValues = newValues ?? string.Empty
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }

}

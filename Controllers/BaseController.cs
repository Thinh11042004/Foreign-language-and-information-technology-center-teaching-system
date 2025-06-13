using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

<<<<<<< HEAD
        protected async Task LogActivityAsync(string action, string entityType, string entityId = null, string oldValues = null, string newValues = null)
=======
        protected async Task LogActivityAsync(string action, string entityType, string entityId = null, object newData = null, object oldData = null)
>>>>>>> c6beacc47ef9deee19b89d4bf7da190a4be3fb75
        {
            var user = await GetCurrentUserAsync();
            var auditLog = new AuditLog
            {
                UserId = user?.Id,
                Action = action,
                EntityType = entityType,
                EntityId = entityId ?? string.Empty,
                Timestamp = DateTime.UtcNow,
<<<<<<< HEAD
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                OldValues = oldValues ?? string.Empty,
                NewValues = newValues ?? string.Empty
=======
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),

                NewValues = newData != null ? JsonConvert.SerializeObject(newData) : "{}",
                OldValues = oldData != null ? JsonConvert.SerializeObject(oldData) : "{}"
>>>>>>> c6beacc47ef9deee19b89d4bf7da190a4be3fb75
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }


    }

}

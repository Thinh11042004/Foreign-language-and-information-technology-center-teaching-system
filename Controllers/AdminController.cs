using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Class;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Dashboard;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Notificationtype;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Report;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Financial;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Financial;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly IReportService _reportService;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                              INotificationService notificationService, IReportService reportService,
                              ILogger<AdminController> logger)
            : base(context, userManager, logger)
        {
            _notificationService = notificationService;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalTeachers = await _context.Teachers.CountAsync(),
                ActiveClasses = await _context.Classes.Where(c => c.Status == ClassStatus.Active).CountAsync(),
                PendingPayments = await _context.Payments.Where(p => p.Status == PaymentStatus.Pending).CountAsync(),
                RecentEnrollments = await _context.Enrollments
                    .Include(e => e.Student.User)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Take(10)
                    .ToListAsync(),
                MonthlyRevenue = await _reportService.GetMonthlyRevenueAsync(),
                ClassSchedule = await GetTodayClassScheduleAsync()
            };

            return View(model);
        }

        private async Task<List<ClassScheduleViewModel>> GetTodayClassScheduleAsync()
        {
            var today = DateTime.Today;
            var dayOfWeek = today.DayOfWeek;

            return await _context.Classes
                .Where(c => c.Status == ClassStatus.Active)
                .Select(c => new ClassScheduleViewModel
                {
                    ClassName = c.Name,
                    CourseTitle = c.Course.Name,
                    RoomName = c.Room.Name,
                    TeacherName = c.ClassTeachers.First(ct => ct.IsPrimary).Teacher.User.FullName,
                    StudentCount = c.CurrentStudents,
                    Schedule = c.Schedule
                })
                .ToListAsync();
        }
    }
}

using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ReportsController : BaseController
    {
        private readonly IReportService _reportService;

        public ReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                IReportService reportService, ILogger<ReportsController> logger)
            : base(context, userManager, logger)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Revenue(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-12);
            endDate ??= DateTime.Now;

            var model = await _reportService.GenerateRevenueReportAsync(startDate.Value, endDate.Value);
            return View(model);
        }

        public async Task<IActionResult> Enrollment(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-6);
            endDate ??= DateTime.Now;

            var model = await _reportService.GenerateEnrollmentReportAsync(startDate.Value, endDate.Value);
            return View(model);
        }

        public async Task<IActionResult> TeacherPerformance()
        {
            var model = await _reportService.GenerateTeacherPerformanceReportAsync();
            return View(model);
        }

        public async Task<IActionResult> StudentProgress(int? courseId = null)
        {
            var model = await _reportService.GenerateStudentProgressReportAsync(courseId);
            return View(model);
        }
    }
}

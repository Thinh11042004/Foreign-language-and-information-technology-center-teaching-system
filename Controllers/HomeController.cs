using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Home;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Shared;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Traffic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [AllowAnonymous]
    public class HomeController : BaseController
    {
        private readonly ITrafficService _trafficService;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                             ILogger<HomeController> logger, ITrafficService trafficService)
            : base(context, userManager, logger)
        {
            _trafficService = trafficService;
        }

        public async Task<IActionResult> Index()
        {
            await _trafficService.LogVisitAsync(HttpContext);

            var model = new HomeViewModel
            {
                FeaturedCourses = await _context.Courses
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(6)
                    .ToListAsync(),
                Statistics = new SiteStatistics
                {
                    TotalStudents = await _context.Students.CountAsync(),
                    TotalTeachers = await _context.Teachers.CountAsync(),
                    TotalCourses = await _context.Courses.Where(c => c.IsActive).CountAsync(),
                    ActiveClasses = await _context.Classes.Where(c => c.Status == ClassStatus.Active).CountAsync()
                }
            };

            return View(model);
        }

        public IActionResult About() => View();
        public IActionResult Contact() => View();
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }


}

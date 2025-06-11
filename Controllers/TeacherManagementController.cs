using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class TeacherManagementController : BaseController
    {
        public TeacherManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                          ILogger<TeacherManagementController> logger)
            : base(context, userManager, logger)
        {
        }

        public async Task<IActionResult> Index()
        {
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .OrderBy(t => t.User.FullName)
                .ToListAsync();

            return View(teachers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                        .ThenInclude(c => c.Course)
                .Include(t => t.Schedules)
                .Include(t => t.Ratings)
                    .ThenInclude(r => r.Student.User)
                .FirstOrDefaultAsync(t => t.id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            var model = new TeacherDetailsViewModel
            {
                Teacher = teacher,
                CurrentClasses = teacher.ClassTeachers
                    .Where(ct => ct.Class.Status == ClassStatus.Active)
                    .Select(ct => ct.Class)
                    .ToList(),
                TotalStudents = await GetTeacherTotalStudentsAsync(id),
                AverageRating = teacher.Rating,
                MonthlyTeachingHours = await GetMonthlyTeachingHoursAsync(id)
            };

            return View(model);
        }

        private async Task<int> GetTeacherTotalStudentsAsync(int teacherId)
        {
            return await _context.ClassTeachers
                .Where(ct => ct.TeacherId == teacherId)
                .SelectMany(ct => ct.Class.Enrollments)
                .Where(e => e.Status == EnrollmentStatus.Active)
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();
        }

        private async Task<int> GetMonthlyTeachingHoursAsync(int teacherId)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            return await _context.Lessons
                .Where(l => l.Class.ClassTeachers.Any(ct => ct.TeacherId == teacherId && ct.IsPrimary) &&
                           l.ScheduledDate.Month == currentMonth &&
                           l.ScheduledDate.Year == currentYear &&
                           l.Status == LessonStatus.Completed)
                .SumAsync(l => (int)l.Duration.TotalHours);
        }
    }
}

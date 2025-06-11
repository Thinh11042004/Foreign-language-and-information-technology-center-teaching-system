using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Course;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.CourseViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class CourseManagementController : BaseController
    {
        public CourseManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                         ILogger<CourseManagementController> logger)
            : base(context, userManager, logger)
        {
        }

        // Courses List
        public async Task<IActionResult> Index(int page = 1, string search = "", CourseType? type = null)
        {
            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search) || c.Code.Contains(search));
            }

            if (type.HasValue)
            {
                query = query.Where(c => c.Type == type.Value);
            }

            var courses = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();

            var model = new CourseListViewModel
            {
                Courses = courses,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(await query.CountAsync() / 20.0),
                SearchTerm = search,
                SelectedType = type
            };

            return View(model);
        }

        // Create Course
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCourseViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = new Course
                {
                    Name = model.Name,
                    Code = model.Code,
                    Description = model.Description,
                    Type = model.Type,
                    Category = model.Category,
                    Level = model.Level,
                    Fee = model.Fee,
                    DurationHours = model.DurationHours,
                    MaxStudents = model.MaxStudents,
                    Prerequisites = model.Prerequisites,
                    LearningOutcomes = model.LearningOutcomes,
                    Materials = model.Materials,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity.Name
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                await LogActivityAsync("CreateCourse", "Course", course.id.ToString());

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Course Details
        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Modules)
                .Include(c => c.Classes)
                    .ThenInclude(cl => cl.ClassTeachers)
                        .ThenInclude(ct => ct.Teacher.User)
                .FirstOrDefaultAsync(c => c.id == id);

            if (course == null)
            {
                return NotFound();
            }

            var model = new CourseDetailsViewModel
            {
                Course = course,
                TotalEnrollments = await _context.Enrollments.Where(e => e.CourseId == id).CountAsync(),
                ActiveClasses = course.Classes.Where(c => c.Status == ClassStatus.Active).Count(),
                AverageRating = await GetCourseAverageRatingAsync(id)
            };

            return View(model);
        }

        private async Task<double> GetCourseAverageRatingAsync(int courseId)
        {
            var classIds = await _context.Classes
                .Where(c => c.CourseId == courseId)
                .Select(c => c.id)
                .ToListAsync();

            if (!classIds.Any()) return 0;

            var ratings = await _context.TeacherRatings
                .Where(r => classIds.Contains(r.ClassId))
                .Select(r => r.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
        }
    }
}

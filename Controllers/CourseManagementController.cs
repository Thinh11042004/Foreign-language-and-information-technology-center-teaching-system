using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Course;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.CourseViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
                TempData["SuccessMessage"] = "Khóa học đã được tạo thành công!";
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

        // Edit Course
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var model = new CreateCourseViewModel
            {
                Name = course.Name,
                Code = course.Code,
                Description = course.Description,
                Type = course.Type,
                Category = course.Category,
                Level = course.Level,
                Fee = course.Fee,
                DurationHours = course.DurationHours,
                MaxStudents = course.MaxStudents,
                Prerequisites = course.Prerequisites,
                LearningOutcomes = course.LearningOutcomes,
                Materials = course.Materials
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCourseViewModel model)
        {
            if (id != model.id) // Assuming CreateCourseViewModel also has an 'id' property for editing. If not, this check needs adjustment.
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var courseToUpdate = await _context.Courses.FindAsync(id);
                    if (courseToUpdate == null)
                    {
                        return NotFound();
                    }

                    courseToUpdate.Name = model.Name;
                    courseToUpdate.Code = model.Code;
                    courseToUpdate.Description = model.Description;
                    courseToUpdate.Type = model.Type;
                    courseToUpdate.Category = model.Category;
                    courseToUpdate.Level = model.Level;
                    courseToUpdate.Fee = model.Fee;
                    courseToUpdate.DurationHours = model.DurationHours;
                    courseToUpdate.MaxStudents = model.MaxStudents;
                    courseToUpdate.Prerequisites = model.Prerequisites;
                    courseToUpdate.LearningOutcomes = model.LearningOutcomes;
                    courseToUpdate.Materials = model.Materials;
                    courseToUpdate.IsActive = true; // Assuming this should remain true or be handled elsewhere
                    // Keep CreatedAt and CreatedBy as they are original
                    // courseToUpdate.UpdatedAt = DateTime.UtcNow; // Add an UpdateAt property in your model if needed

                    _context.Update(courseToUpdate);
                    await _context.SaveChangesAsync();
                    await LogActivityAsync("UpdateCourse", "Course", courseToUpdate.id.ToString());
                    TempData["SuccessMessage"] = "Khóa học đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(model.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Delete Course
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Capture the old values before deletion for audit log
            var oldValues = JsonSerializer.Serialize(course);

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            // Log the activity with old values and empty new values for deletion
            await LogActivityAsync("DeleteCourse", "Course", course.id.ToString(), oldValues, "");
            TempData["SuccessMessage"] = "Khóa học đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.id == id);
        }
    }
}

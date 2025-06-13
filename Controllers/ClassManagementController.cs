using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.ClassroomViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ClassManagementController : BaseController
    {
        public ClassManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                        ILogger<ClassManagementController> logger)
            : base(context, userManager, logger)
        {
        }

        public async Task<IActionResult> Index()
        {
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Room)
                .Include(c => c.ClassTeachers)
                    .ThenInclude(ct => ct.Teacher.User)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            return View(classes);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateClassViewModel
            {
                Courses = await _context.Courses.ToListAsync(),
                Rooms = await _context.Rooms.ToListAsync(),
                Teachers = await _context.Teachers
                    .Include(t => t.User)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = await _context.Courses.FindAsync(model.CourseId);

                var newClass = new Classroom
                {
                    Name = model.Name,
                    Code = GenerateClassCode(),
                    CourseId = model.CourseId,
                    RoomId = model.RoomId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Schedule = model.Schedule,
                    MaxStudents = model.MaxStudents,
                    CurrentStudents = 0,
                    Status = ClassStatus.Planned,
                    ClassFee = course.Fee,
                    Notes = model.Notes
                };

                _context.Classes.Add(newClass);
                await _context.SaveChangesAsync();

                // Assign teachers
                if (model.TeacherIds != null && model.TeacherIds.Any())
                {
                    for (int i = 0; i < model.TeacherIds.Count; i++)
                    {
                        var classTeacher = new ClassTeacher
                        {
                            ClassId = newClass.id,
                            TeacherId = model.TeacherIds[i],
                            IsPrimary = i == 0, // First teacher is primary
                            AssignedDate = DateTime.UtcNow,
                            Role = i == 0 ? "Main Teacher" : "Assistant"
                        };
                        _context.ClassTeachers.Add(classTeacher);
                    }
                    await _context.SaveChangesAsync();
                }

                await LogActivityAsync("CreateClass", "Class", newClass.id.ToString());
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            model.Courses = await _context.Courses.ToListAsync();
            model.Rooms = await _context.Rooms.ToListAsync();
            model.Teachers = await _context.Teachers.Include(t => t.User).ToListAsync();

            return View(model);
        }

        // Class Details
        public async Task<IActionResult> Details(int id)
        {
            var classroom = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Room)
                .Include(c => c.ClassTeachers)
                    .ThenInclude(ct => ct.Teacher.User)
                .Include(c => c.Enrollments)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.id == id);

            if (classroom == null)
            {
                return NotFound();
            }

            var model = new ClassDetailsViewModel
            {
                Class = classroom,
                TotalEnrollments = classroom.Enrollments.Count(),
                TotalLessons = classroom.Lessons.Count(),
                TotalTeachers = classroom.ClassTeachers.Count()
            };

            return View(model);
        }

        // Edit Class
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.Classes
                .Include(c => c.ClassTeachers)
                .FirstOrDefaultAsync(c => c.id == id);

            if (classroom == null)
            {
                return NotFound();
            }

            var model = new CreateClassViewModel
            {
                id = classroom.id,
                Name = classroom.Name,
                CourseId = classroom.CourseId,
                RoomId = classroom.RoomId,
                StartDate = classroom.StartDate,
                EndDate = classroom.EndDate,
                Schedule = classroom.Schedule,
                MaxStudents = classroom.MaxStudents,
                Notes = classroom.Notes,
                TeacherIds = classroom.ClassTeachers.Select(ct => ct.TeacherId).ToList(),
                Courses = await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                Rooms = await _context.Rooms.Where(r => r.IsActive).ToListAsync(),
                Teachers = await _context.Teachers.Include(t => t.User).Where(t => t.IsAvailable).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateClassViewModel model)
        {
            if (id != model.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var classToUpdate = await _context.Classes
                        .Include(c => c.ClassTeachers)
                        .FirstOrDefaultAsync(c => c.id == id);

                    if (classToUpdate == null)
                    {
                        return NotFound();
                    }

                    var oldValues = System.Text.Json.JsonSerializer.Serialize(classToUpdate);

                    classToUpdate.Name = model.Name;
                    classToUpdate.CourseId = model.CourseId;
                    classToUpdate.RoomId = model.RoomId;
                    classToUpdate.StartDate = model.StartDate;
                    classToUpdate.EndDate = model.EndDate;
                    classToUpdate.Schedule = model.Schedule;
                    classToUpdate.MaxStudents = model.MaxStudents;
                    classToUpdate.Notes = model.Notes;

                    // Update teachers
                    _context.ClassTeachers.RemoveRange(classToUpdate.ClassTeachers);
                    if (model.TeacherIds != null && model.TeacherIds.Any())
                    {
                        for (int i = 0; i < model.TeacherIds.Count; i++)
                        {
                            var classTeacher = new ClassTeacher
                            {
                                ClassId = classToUpdate.id,
                                TeacherId = model.TeacherIds[i],
                                IsPrimary = i == 0,
                                AssignedDate = DateTime.UtcNow,
                                Role = i == 0 ? "Main Teacher" : "Assistant"
                            };
                            _context.ClassTeachers.Add(classTeacher);
                        }
                    }

                    _context.Update(classToUpdate);
                    await _context.SaveChangesAsync();

                    var newValues = System.Text.Json.JsonSerializer.Serialize(classToUpdate);
                    await LogActivityAsync("UpdateClass", "Class", classToUpdate.id.ToString(), oldValues, newValues);
                    TempData["SuccessMessage"] = "Lớp học đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(model.id))
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
            // Reload dropdowns if validation fails
            model.Courses = await _context.Courses.Where(c => c.IsActive).ToListAsync();
            model.Rooms = await _context.Rooms.Where(r => r.IsActive).ToListAsync();
            model.Teachers = await _context.Teachers.Include(t => t.User).Where(t => t.IsAvailable).ToListAsync();
            return View(model);
        }

        // Delete Class
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Room)
                .FirstOrDefaultAsync(m => m.id == id);
            if (classroom == null)
            {
                return NotFound();
            }

            return View(classroom);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classroom = await _context.Classes.FindAsync(id);
            if (classroom == null)
            {
                return NotFound();
            }

            // Capture the old values before deletion for audit log
            var oldValues = System.Text.Json.JsonSerializer.Serialize(classroom);

            _context.Classes.Remove(classroom);
            await _context.SaveChangesAsync();

            // Log the activity with old values and empty new values for deletion
            await LogActivityAsync("DeleteClass", "Class", classroom.id.ToString(), oldValues, "");
            TempData["SuccessMessage"] = "Lớp học đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.id == id);
        }

        private string GenerateClassCode()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var sequence = _context.Classes.Count() + 1;
            return $"CL{year}{sequence:D4}";
        }
    }
}

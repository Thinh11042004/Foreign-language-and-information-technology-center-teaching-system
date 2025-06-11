using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Class;
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
                Courses = await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                Rooms = await _context.Rooms.Where(r => r.IsActive).ToListAsync(),
                Teachers = await _context.Teachers
                    .Include(t => t.User)
                    .Where(t => t.IsAvailable)
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
            model.Courses = await _context.Courses.Where(c => c.IsActive).ToListAsync();
            model.Rooms = await _context.Rooms.Where(r => r.IsActive).ToListAsync();
            model.Teachers = await _context.Teachers.Include(t => t.User).Where(t => t.IsAvailable).ToListAsync();

            return View(model);
        }

        private string GenerateClassCode()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var sequence = _context.Classes.Count() + 1;
            return $"CL{year}{sequence:D4}";
        }
    }
}

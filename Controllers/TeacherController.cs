using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Class;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Dashboard;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : BaseController
    {
        public TeacherController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                ILogger<TeacherController> logger)
            : base(context, userManager, logger)
        {
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                        .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var model = new TeacherDashboardViewModel
            {
                Teacher = teacher,
                TodayClasses = await GetTodayClassesAsync(teacher.id),
                ActiveClasses = teacher.ClassTeachers
                    .Where(ct => ct.Class.Status == ClassStatus.Active)
                    .Select(ct => ct.Class)
                    .ToList(),
                PendingAssignments = await GetPendingAssignmentsAsync(teacher.id),
                RecentRatings = await _context.TeacherRatings
                    .Include(r => r.Student.User)
                    .Include(r => r.Class.Course)
                    .Where(r => r.TeacherId == teacher.id)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> MyClasses()
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            var classes = await _context.ClassTeachers
                .Include(ct => ct.Class)
                    .ThenInclude(c => c.Course)
                .Include(ct => ct.Class.Room)
                .Where(ct => ct.TeacherId == teacher.id)
                .Select(ct => ct.Class)
                .ToListAsync();

            return View(classes);
        }

        public async Task<IActionResult> ClassDetails(int id)
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            var classTeacher = await _context.ClassTeachers
                .Include(ct => ct.Class)
                    .ThenInclude(c => c.Course)
                .Include(ct => ct.Class.Room)
                .Include(ct => ct.Class.Enrollments)
                    .ThenInclude(e => e.Student.User)
                .FirstOrDefaultAsync(ct => ct.ClassId == id && ct.TeacherId == teacher.id);

            if (classTeacher == null)
            {
                return NotFound();
            }

            var model = new TeacherClassDetailsViewModel
            {
                Class = classTeacher.Class,
                Students = classTeacher.Class.Enrollments
                    .Where(e => e.Status == EnrollmentStatus.Active)
                    .Select(e => e.Student)
                    .ToList(),
                Lessons = await _context.Lessons
                    .Where(l => l.ClassId == id)
                    .OrderByDescending(l => l.ScheduledDate)
                    .ToListAsync(),
                UpcomingAssignments = await _context.Assignments
                    .Where(a => a.Lesson.ClassId == id && a.DueDate > DateTime.Now)
                    .OrderBy(a => a.DueDate)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAttendance(int lessonId, List<AttendanceRecordViewModel> attendanceRecords)
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            foreach (var record in attendanceRecords)
            {
                var attendance = new AttendanceRecord
                {
                    StudentId = record.StudentId,
                    LessonId = lessonId,
                    ClassId = record.ClassId,
                    Date = DateTime.Today,
                    Status = record.Status,
                    Notes = record.Notes,
                    MarkedBy = user.Id,
                    MarkedAt = DateTime.UtcNow
                };

                _context.AttendanceRecords.Add(attendance);
            }

            await _context.SaveChangesAsync();
            await LogActivityAsync("MarkAttendance", "Lesson", lessonId.ToString());

            return RedirectToAction(nameof(ClassDetails), new { id = attendanceRecords.First().ClassId });
        }

        private async Task<List<Classroom>> GetTodayClassesAsync(int teacherId)
        {
            var today = DateTime.Today;
            var dayOfWeek = today.DayOfWeek;

            return await _context.ClassTeachers
                .Where(ct => ct.TeacherId == teacherId && ct.Class.Status == ClassStatus.Active)
                .Select(ct => ct.Class)
                .Where(c => c.Schedule.Contains(dayOfWeek.ToString()))
                .ToListAsync();
        }

        private async Task<List<StudentAssignment>> GetPendingAssignmentsAsync(int teacherId)
        {
            return await _context.StudentAssignments
                .Include(sa => sa.Assignment)
                .Include(sa => sa.Student.User)
                .Where(sa => sa.Assignment.Lesson.Class.ClassTeachers.Any(ct => ct.TeacherId == teacherId) &&
                            sa.Status == AssignmentStatus.Submitted &&
                            sa.Score == null)
                .OrderBy(sa => sa.SubmittedAt)
                .Take(10)
                .ToListAsync();
        }
    }

}

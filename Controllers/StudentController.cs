using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Course;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : BaseController
    {
        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                ILogger<StudentController> logger)
            : base(context, userManager, logger)
        {
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Class)
                        .ThenInclude(c => c.ClassTeachers)
                            .ThenInclude(ct => ct.Teacher.User)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var model = new StudentDashboardViewModel
            {
                Student = student,
                ActiveEnrollments = student.Enrollments.Where(e => e.Status == EnrollmentStatus.Active).ToList(),
                TodayClasses = await GetStudentTodayClassesAsync(student.id),
                PendingAssignments = await GetStudentPendingAssignmentsAsync(student.id),
                RecentGrades = await GetStudentRecentGradesAsync(student.id),
                OverallProgress = await GetStudentOverallProgressAsync(student.id)
            };

            return View(model);
        }

        public async Task<IActionResult> MyCourses()
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassTeachers)
                        .ThenInclude(ct => ct.Teacher.User)
                .Where(e => e.StudentId == student.id)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();

            return View(enrollments);
        }

        public async Task<IActionResult> CourseDetails(int enrollmentId)
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Modules)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassTeachers)
                        .ThenInclude(ct => ct.Teacher.User)
                .FirstOrDefaultAsync(e => e.id == enrollmentId && e.StudentId == student.id);

            if (enrollment == null)
            {
                return NotFound();
            }

            var model = new StudentCourseDetailsViewModel
            {
                Enrollment = enrollment,
                Progress = await _context.StudentProgresses
                    .Where(p => p.StudentId == student.id && p.CourseId == enrollment.CourseId)
                    .ToListAsync(),
                Assignments = await GetCourseAssignmentsAsync(enrollment.ClassId.Value, student.id),
                AttendanceRecords = await _context.AttendanceRecords
                    .Where(a => a.StudentId == student.id && a.ClassId == enrollment.ClassId)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Assignments()
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

            var assignments = await _context.StudentAssignments
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Lesson.Class.Course)
                .Where(sa => sa.StudentId == student.id)
                .OrderByDescending(sa => sa.Assignment.DueDate)
                .ToListAsync();

            return View(assignments);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAssignment(int assignmentId, string content, IFormFile[] files)
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null)
            {
                return NotFound();
            }

            var studentAssignment = await _context.StudentAssignments
                .FirstOrDefaultAsync(sa => sa.AssignmentId == assignmentId && sa.StudentId == student.id);

            if (studentAssignment == null)
            {
                studentAssignment = new StudentAssignment
                {
                    AssignmentId = assignmentId,
                    StudentId = student.id,
                    Status = AssignmentStatus.Submitted
                };
                _context.StudentAssignments.Add(studentAssignment);
            }

            studentAssignment.SubmissionContent = content;
            studentAssignment.SubmittedAt = DateTime.UtcNow;
            studentAssignment.Status = AssignmentStatus.Submitted;

            // Handle file uploads
            if (files != null && files.Length > 0)
            {
                var filePaths = new List<string>();
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine("uploads", "assignments", fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        filePaths.Add(filePath);
                    }
                }
                studentAssignment.AttachedFiles = string.Join(";", filePaths);
            }

            await _context.SaveChangesAsync();
            await LogActivityAsync("SubmitAssignment", "Assignment", assignmentId.ToString());

            return RedirectToAction(nameof(Assignments));
        }

        private async Task<List<Classroom>> GetStudentTodayClassesAsync(int studentId)
        {
            var today = DateTime.Today;
            var dayOfWeek = today.DayOfWeek;

            return await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.Class)
                .Where(c => c.Status == ClassStatus.Active && c.Schedule.Contains(dayOfWeek.ToString()))
                .ToListAsync();
        }

        private async Task<List<StudentAssignment>> GetStudentPendingAssignmentsAsync(int studentId)
        {
            return await _context.StudentAssignments
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Lesson.Class.Course)
                .Where(sa => sa.StudentId == studentId &&
                            sa.Status == AssignmentStatus.Pending &&
                            sa.Assignment.DueDate > DateTime.Now)
                .OrderBy(sa => sa.Assignment.DueDate)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<StudentAssignment>> GetStudentRecentGradesAsync(int studentId)
        {
            return await _context.StudentAssignments
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Lesson.Class.Course)
                .Where(sa => sa.StudentId == studentId && sa.Score.HasValue)
                .OrderByDescending(sa => sa.GradedAt)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<StudentProgress>> GetStudentOverallProgressAsync(int studentId)
        {
            return await _context.StudentProgresses
                .Include(p => p.Course)
                .Where(p => p.StudentId == studentId)
                .ToListAsync();
        }

        private async Task<List<StudentAssignment>> GetCourseAssignmentsAsync(int classId, int studentId)
        {
            return await _context.StudentAssignments
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Lesson)
                .Where(sa => sa.StudentId == studentId && sa.Assignment.Lesson.ClassId == classId)
                .OrderByDescending(sa => sa.Assignment.DueDate)
                .ToListAsync();
        }
    }
}

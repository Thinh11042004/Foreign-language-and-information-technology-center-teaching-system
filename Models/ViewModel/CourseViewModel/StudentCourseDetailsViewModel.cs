using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Course
{
    public class StudentCourseDetailsViewModel
    {
        public Enrollment Enrollment { get; set; }
        public List<StudentProgress> Progress { get; set; }
        public List<StudentAssignment> Assignments { get; set; }
        public List<AttendanceRecord> AttendanceRecords { get; set; }

        // Thêm nếu cần thêm thông tin khác
        public Students Student { get; set; }
    }
}

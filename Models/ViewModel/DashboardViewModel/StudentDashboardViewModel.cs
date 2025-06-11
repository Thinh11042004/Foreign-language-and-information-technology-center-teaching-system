using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;


namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Dashboard
{
    public class StudentDashboardViewModel
    {
        public Students Student { get; set; }
        public List<Enrollment> ActiveEnrollments { get; set; }
        public List<Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses.Classroom> TodayClasses { get; set; }
        public List<StudentAssignment> PendingAssignments { get; set; }
        public List<StudentAssignment> RecentGrades { get; set; }
        public List<StudentProgress> OverallProgress { get; set; }
    }
}

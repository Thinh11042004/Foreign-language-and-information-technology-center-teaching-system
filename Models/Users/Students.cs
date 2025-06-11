using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Finance;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users
{
    public class Students
    {
        public int id { get; set; }
        public string UserId { get; set; }
        public string StudentCode { get; set; }
        public string ParentName { get; set; }
        public string ParentPhone { get; set; }
        public string EmergencyContact { get; set; }
        public string LearningGoals { get; set; }
        public string PreviousExperience { get; set; }
        public StudentStatus Status { get; set; } = StudentStatus.Active;
        public DateTime EnrollmentDate { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<StudentProgress> Progresses { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; }
    }
}

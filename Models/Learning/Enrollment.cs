using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Finance;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning
{
    public class Enrollment
    {
        public int id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int? ClassId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Studying;
        public decimal PaidAmount { get; set; }
        public decimal TotalFee { get; set; }
        public string PaymentPlan { get; set; } 
        public DateTime? CompletionDate { get; set; }
        public decimal? FinalGrade { get; set; }
        public string Certificate { get; set; }
        public string Notes { get; set; }

        public virtual Students Student { get; set; }
        public virtual Courses.Course Course { get; set; }
        public virtual Classroom Class { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }

    }
}

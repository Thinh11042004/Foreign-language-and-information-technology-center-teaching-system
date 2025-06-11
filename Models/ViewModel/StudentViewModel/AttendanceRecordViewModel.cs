using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Student
{
    public class AttendanceRecordViewModel
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public AttendanceStatus Status { get; set; }
        public string Notes { get; set; }
    }
}

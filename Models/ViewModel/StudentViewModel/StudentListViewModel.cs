using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Student
{
    public class StudentListViewModel
    {
        public List<Students> Students { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public StudentStatus? SelectedStatus { get; set; }
        public Dictionary<StudentStatus, string> StatusLabels =>
            new()
            {
                { StudentStatus.Active, "Đang học" },
                { StudentStatus.Graduated, "Đã tốt nghiệp" },
                { StudentStatus.Inactive, "Tạm dừng" }
            };
    }
}

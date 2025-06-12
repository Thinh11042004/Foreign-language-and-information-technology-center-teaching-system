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
        public UserStatus? SelectedStatus { get; set; }
        public Dictionary<UserStatus, string> StatusLabels =>
            new()
            {
                { UserStatus.Active, "Đang học" },
                { UserStatus.Graduated, "Đã tốt nghiệp" },
                { UserStatus.Inactive, "Tạm dừng" }
            };
    }
}

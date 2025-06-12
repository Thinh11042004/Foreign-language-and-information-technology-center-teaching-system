using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.User
{
    public class UserListViewModel
    {
        public List<ApplicationUser> Users { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public string SelectedRole { get; set; }
        public string SearchString { get; set; }
        public List<ApplicationRole> Roles { get; set; }
    }
}

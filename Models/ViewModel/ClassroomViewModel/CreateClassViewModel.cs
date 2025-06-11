using CourseModel = Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses.Course;

using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Facilities;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Class
{
    public class CreateClassViewModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public string Schedule { get; set; }

        [Required]
        [Range(1, 100)]
        public int MaxStudents { get; set; }

        public string Notes { get; set; }

        public List<int> TeacherIds { get; set; }

        // For dropdown lists
        public List<CourseModel> Courses { get; set; }
        public List<Room> Rooms { get; set; }
        public List<Teachers> Teachers { get; set; }
    }
}

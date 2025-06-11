using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Facilities
{
    public class Room
    {

        public int id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Capacity { get; set; }
        public RoomType Type { get; set; }
        public string Equipment { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; } = true;
        public string Description { get; set; }

        public virtual ICollection<Classroom> Classes { get; set; }
        public virtual ICollection<RoomBooking> Bookings { get; set; }
    }
}

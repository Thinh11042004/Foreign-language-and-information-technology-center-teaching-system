﻿using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Feedback;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Dashboard
{
    public class TeacherDashboardViewModel
    {
        public Teachers Teacher { get; set; }
        public List<Classroom> TodayClasses { get; set; }
        public List<Classroom> ActiveClasses { get; set; }
        public List<StudentAssignment> PendingAssignments { get; set; }
        public List<TeacherRating> RecentRatings { get; set; }
    }
}

using NabdSchool.Core.Entities;

namespace NabdSchool.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int DeletedStudents { get; set; }
        public int TotalGrades { get; set; }
        public int TotalClasses { get; set; }
        public List<GradeStatistic> StudentsByGrade { get; set; }
        public List<AuditLog> RecentActivities { get; set; }
    }
}

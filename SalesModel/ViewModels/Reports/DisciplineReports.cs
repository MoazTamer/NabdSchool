using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels.Reports
{
    // ViewModel لتقرير الطلاب الأكثر انضباطاً
    public class MostDisciplinedStudentsReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? ClassId { get; set; }
        public int? ClassRoomId { get; set; }
        public string ClassName { get; set; }
        public string ClassRoomName { get; set; }
        public string PeriodText { get; set; }
        public int TopCount { get; set; } = 10;
        public List<MostDisciplinedStudentViewModel> Students { get; set; }
        public int TotalStudents { get; set; }
        public int TotalPresentDays { get; set; }
    }

    public class MostDisciplinedStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string ClassName { get; set; }
        public string ClassRoomName { get; set; }
        public int PresentDays { get; set; }
        public int LateDays { get; set; }
        public int AbsentDays { get; set; }
        public int TotalDays { get; set; }
        public double DisciplinePercentage { get; set; }
        public double AbsencePercentage { get; set; }
        public double LatePercentage { get; set; }
    }

    // ViewModel لتقرير الفصول الأكثر انضباطاً
    public class MostDisciplinedClassesReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? ClassId { get; set; }
        public string PeriodText { get; set; }
        public List<MostDisciplinedClassViewModel> Classes { get; set; }
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
    }

    public class MostDisciplinedClassViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int ClassRoomId { get; set; }
        public string ClassRoomName { get; set; }
        public int TotalStudents { get; set; }
        public int TotalPresentDays { get; set; }
        public int TotalLateDays { get; set; }
        public int TotalAbsentDays { get; set; }
        public double DisciplinePercentage { get; set; }
        public double AbsencePercentage { get; set; }
        public double LatePercentage { get; set; }
        public int Rank { get; set; }
    }
}

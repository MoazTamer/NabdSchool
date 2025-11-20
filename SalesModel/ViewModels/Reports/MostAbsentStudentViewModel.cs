using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels.Reports
{
    public class MostAbsentStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string ClassName { get; set; }
        public string ClassRoomName { get; set; }
        public int AbsentDays { get; set; }
        public int TotalDays { get; set; }
        public double AbsentPercentage => TotalDays > 0 ? Math.Round((AbsentDays * 100.0) / TotalDays, 1) : 0;
    }

    public class MostAbsentStudentsReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? ClassId { get; set; }
        public int? ClassRoomId { get; set; }
        public int TopCount { get; set; }
        public List<MostAbsentStudentViewModel> Students { get; set; }
        public int TotalStudents { get; set; }
        public int TotalAbsentDays { get; set; }
        public string ReportType { get; set; } = "غياب"; // القيمة الافتراضية

        // خصائص للعرض
        public string ClassName { get; set; }
        public string ClassRoomName { get; set; }
        public string PeriodText => $"من {FromDate:yyyy/MM/dd} إلى {ToDate:yyyy/MM/dd}";
        public string ReportTitle => ReportType == "تأخر" ? "تقرير أكثر الطلاب تأخر" : "تقرير أكثر الطلاب غياب";
    }
}

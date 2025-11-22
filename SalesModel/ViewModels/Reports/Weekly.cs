using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels.Reports
{
    public class WeeklyPatternReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? ClassId { get; set; }
        public int? ClassRoomId { get; set; }
        public string ClassName { get; set; }
        public string ClassRoomName { get; set; }

        public List<StudentWeeklyPatternViewModel> Students { get; set; }
        public WeeklySummaryViewModel Summary { get; set; }
        public string ReportType { get; set; }
    }

    public class StudentWeeklyPatternViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string ClassName { get; set; }
        public string ClassRoomName { get; set; }

        public List<DayPatternViewModel> DayPatterns { get; set; }
        public int TotalLate { get; set; }
        public int TotalAbsent { get; set; }
        public string MostFrequentDay { get; set; }
        public string MostFrequentType { get; set; }
        public int PatternStrength { get; set; } // قوة النمط (نسبة التكرار)
        public int TotalStudents { get; set; }
    }

    public class DayPatternViewModel
    {
        public string DayName { get; set; }
        public string DayNameEnglish { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int TotalOccurrences { get; set; }
        public double Percentage { get; set; }
        public string PatternType { get; set; } // "late", "absent", "mixed", "none"
    }

    public class WeeklySummaryViewModel
    {
        public int TotalStudents { get; set; }
        public int StudentsWithPatterns { get; set; }
        public Dictionary<string, int> DayPatternsCount { get; set; }
        public Dictionary<string, int> MostFrequentDays { get; set; }
        public List<StrongPatternStudentViewModel> StrongPatternStudents { get; set; }
    }

    public class StrongPatternStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string PatternDay { get; set; }
        public string PatternType { get; set; }
        public int OccurrenceCount { get; set; }
        public double Percentage { get; set; }
    }
}

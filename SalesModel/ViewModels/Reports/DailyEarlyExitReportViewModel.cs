using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels.Reports
{
    public class DailyEarlyExitReportViewModel
    {
        public DateTime ReportDate { get; set; }
        public List<ClassEarlyExitViewModel> ClassesEarlyExit { get; set; }
        public int TotalEarlyExitStudents { get; set; }
        public int TotalClasses { get; set; }
        public List<ClassEarlyExitViewModel> ClassesReport { get; set; }
    }

    public class ClassEarlyExitViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int ClassRoomId { get; set; }
        public string ClassRoomName { get; set; }
        public int TotalStudents { get; set; }
        public int EarlyExitStudents { get; set; }
        public double EarlyExitPercentage { get; set; }
        public List<EarlyExitStudentViewModel> EarlyExitStudentsList { get; set; }
    }

    public class EarlyExitStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string StudentPhone { get; set; }
        public string ExitTime { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
        public int ConsecutiveEarlyExitDays { get; set; }
    }
}

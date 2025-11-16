using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels.Reports
{
    public class StudentAttendanceReportViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public DateTime ReportDate { get; set; }

        public List<StudentDayStatusViewModel> Days { get; set; }

        public int TotalPresent { get; set; }
        public int TotalLate { get; set; }
        public int TotalAbsent { get; set; }

        public int ConsecutiveLate { get; set; }
        public int ConsecutiveAbsent { get; set; }
    }

}

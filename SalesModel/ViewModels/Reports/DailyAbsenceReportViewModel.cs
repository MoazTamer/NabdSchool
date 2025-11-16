using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels.Reports
{
    public class DailyAbsenceReportViewModel
    {
        public DateTime ReportDate { get; set; }
        public List<ClassAbsenceViewModel> ClassesAbsence { get; set; }
        public int TotalAbsentStudents { get; set; }
        public int TotalClasses { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels.Reports
{
    public class StudentAbsenceNoticeViewModel
    {
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string StudentGuardianType { get; set; }
        public List<AbsenceDateInfo> AbsenceDates { get; set; }
        public string NoticeText { get; set; }
        public string StudentSignature { get; set; } 
        public string DateText { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }

    }

    public class AbsenceDateInfo
    {
        public int RowNumber { get; set; }
        public string Date { get; set; }
        public string DayName { get; set; }
    }
}

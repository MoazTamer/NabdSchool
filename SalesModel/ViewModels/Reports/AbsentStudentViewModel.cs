using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels.Reports
{
    public class AbsentStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string StudentPhone { get; set; }
        public int ConsecutiveAbsenceDays { get; set; }
        public string Notes { get; set; }
    }
}
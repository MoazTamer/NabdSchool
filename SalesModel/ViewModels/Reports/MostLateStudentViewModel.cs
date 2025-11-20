using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels.Reports
{
    public class MostLateStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string ClassName { get; set; }
        public string ClassRoomName { get; set; }
        public int LateDays { get; set; }
        public int TotalDays { get; set; }
        public double LatePercentage => TotalDays > 0 ? Math.Round((LateDays * 100.0) / TotalDays, 1) : 0;
    }
}

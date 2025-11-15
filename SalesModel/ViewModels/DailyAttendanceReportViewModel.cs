using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels
{
    public class DailyAttendanceReportViewModel
    {
        public DateTime Date { get; set; }

        public int SchoolId { get; set; }
        public string SchoolName { get; set; }

        public int ClassId { get; set; }
        public string ClassName { get; set; }

        public int ClassRoomId { get; set; }
        public string ClassRoomName { get; set; }

        public List<StudentAttendanceViewModel> StudentAttendances { get; set; }
    }

    public class StudentAttendanceViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string AttendanceStatus { get; set; } // حضور/متأخر/غياب
        public TimeSpan? AttendanceTime { get; set; }
        public int LateMinutes { get; set; }
    }

}

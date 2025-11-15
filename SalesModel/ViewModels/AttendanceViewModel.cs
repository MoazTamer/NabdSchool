using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels
{
    public class AttendanceViewModel
    {
        public int TotalStudents { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
    }

}

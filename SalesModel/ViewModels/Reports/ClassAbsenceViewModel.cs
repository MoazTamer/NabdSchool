using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels.Reports
{
    public class ClassAbsenceViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int ClassRoomId { get; set; }
        public string ClassRoomName { get; set; }
        public int TotalStudents { get; set; }
        public int AbsentStudents { get; set; }
        public decimal AbsencePercentage { get; set; }
        public List<AbsentStudentViewModel> AbsentStudentsList { get; set; }
    }
}

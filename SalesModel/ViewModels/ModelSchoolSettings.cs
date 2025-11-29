using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels
{
    public class ModelSchoolSettings
    {
        public int Setting_ID { get; set; }

        public string Setting_Key { get; set; }

        [Required(ErrorMessage = "موعد الحضور مطلوب")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "الرجاء إدخال الوقت بصيغة صحيحة (مثال: 07:30)")]
        public string AttendanceTime { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال العام الدراسي")]
        [Display(Name = "العام الدراسي")]
        public string AcademicYear { get; set; }

        [Required(ErrorMessage = "الرجاء اختيار الفصل الدراسي")]
        [Display(Name = "الفصل الدراسي")]
        public string Semester { get; set; } 


        public string Setting_Description { get; set; }
    }
}

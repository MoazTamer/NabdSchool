using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels
{
    public class ModelStudent
    {
        public int Student_ID { get; set; }

        [Required(ErrorMessage = "من فضلك اختر الفصل")]
        public int ClassRoom_ID { get; set; }

        [Required(ErrorMessage = "من فضلك ادخل اسم الطالب")]
        [StringLength(200, ErrorMessage = "اسم الطالب يجب أن لا يزيد عن 200 حرف")]
        public string Student_Name { get; set; }

        [StringLength(50, ErrorMessage = "رقم الطالب يجب أن لا يزيد عن 50 حرف")]
        public string? Student_Code { get; set; }

        [StringLength(20, ErrorMessage = "رقم الجوال يجب أن لا يزيد عن 20 حرف")]
        [Phone(ErrorMessage = "من فضلك ادخل رقم جوال صحيح")]
        public string? Student_Phone { get; set; }

        [StringLength(500, ErrorMessage = "العنوان يجب أن لا يزيد عن 500 حرف")]
        public string? Student_Address { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Student_BirthDate { get; set; }

        [StringLength(10, ErrorMessage = "الجنس يجب أن لا يزيد عن 10 حرف")]
        public string? Student_Gender { get; set; }

        [StringLength(1000, ErrorMessage = "الملاحظات يجب أن لا تزيد عن 1000 حرف")]
        public string? Student_Notes { get; set; }

        // For display purposes
        public string? ClassName { get; set; }
        public string? ClassRoomName { get; set; }
    }
}

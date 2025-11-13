using System.ComponentModel.DataAnnotations;

namespace SalesModel.ViewModels

{
    public class StudentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "رقم الطالب مطلوب")]
        [Display(Name = "رقم الطالب")]
        [StringLength(50)]
        public string StudentNumber { get; set; }

        [Required(ErrorMessage = "اسم الطالب مطلوب")]
        [Display(Name = "اسم الطالب")]
        [StringLength(200)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        [Display(Name = "رقم الجوال")]
        [Phone(ErrorMessage = "رقم الجوال غير صحيح")]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "الصف مطلوب")]
        [Display(Name = "الصف")]
        public int GradeId { get; set; }

        [Required(ErrorMessage = "الفصل مطلوب")]
        [Display(Name = "الفصل")]
        public int ClassId { get; set; }

        // For Display
        public string GradeName { get; set; }
        public string ClassName { get; set; }
    }
}

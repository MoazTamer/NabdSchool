using System.ComponentModel.DataAnnotations;

namespace NabdSchool.Web.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        public string Username { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        [StringLength(200)]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "رقم الجوال غير صحيح")]
        [Display(Name = "رقم الجوال")]
        public string PhoneNumber { get; set; }
    }


}

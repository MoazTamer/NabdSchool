using System.ComponentModel.DataAnnotations;

namespace NabdSchool.Web.ViewModels
{
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string Roles { get; set; }
    }
}

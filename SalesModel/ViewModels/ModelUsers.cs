using Microsoft.AspNetCore.Mvc.Rendering;
using SalesModel.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesModel.ViewModels
{
    public class ModelUsers
	{
		public string Id { get; set; }
		public int BranchID { get; set; }
        public int CashBalance_ID { get; set; }
        public int CashBalanceBank_ID { get; set; }
		public string Branch_Name { get; set; }
		public string BranchName { get; set; }
		public string UserName { get; set; }
        public string UserType { get; set; }
		public string Password { get; set; }
		public string Visible { get; set; }

		public IEnumerable<SelectListItem> BranchList { get; set; }
        public IEnumerable<SelectListItem> UserTypeList { get; set; }
    }

    public class ModelClass
    {
        public int Class_ID { get; set; }
        public string Class_Name { get; set; }
        public List<ModelClassRoom>? ClassRooms { get; set; }
    }

    public class ModelClassRoom
    {
        public int ClassRoom_ID { get; set; }
        public int Class_ID { get; set; }
        public string ClassRoom_Name { get; set; }
        public string? Class_Name { get; set; }
        public int? StudentsCount { get; set; } // عدد الطلاب في الفصل
    }

    public class ModelStudent2
    {
        public int Student_ID { get; set; }
        public int ClassRoom_ID { get; set; }
        public string Student_Name { get; set; }
        public string? Student_Code { get; set; }
        public string? Student_Phone { get; set; }
        public string? Student_Address { get; set; }
        public DateTime? Student_BirthDate { get; set; }
        public string? Student_Gender { get; set; }
        public string? Student_Notes { get; set; }
        public string? ClassRoom_Name { get; set; }
        public string? Class_Name { get; set; }
    }

}

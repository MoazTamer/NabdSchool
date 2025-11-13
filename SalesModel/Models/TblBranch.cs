using System.ComponentModel.DataAnnotations;

namespace SalesModel.Models
{
	public class TblBranch
	{
		[Key]
		public int Branch_ID { get; set; }
		public string Branch_Name { get; set; }
		public string? Branch_Visible { get; set; }
		public string? Branch_AddUserID { get; set; }
		public DateTime? Branch_AddDate { get; set; }
		public string? Branch_EditUserID { get; set; }
		public DateTime? Branch_EditDate { get; set; }
		public string? Branch_DeleteUserID { get; set; }
		public DateTime? Branch_DeleteDate { get; set; }
	}
}

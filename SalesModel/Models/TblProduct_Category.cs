using System.ComponentModel.DataAnnotations;

namespace SalesModel.Models
{
	public class TblProduct_Category
	{
		[Key]
		public int ProductCategory_ID { get; set; }
        public string ProductCategory_Name { get; set; }
		public string? ProductCategory_Visible { get; set; }
		public string? ProductCategory_AddUserID { get; set; }
		public DateTime? ProductCategory_AddDate { get; set; }
		public string? ProductCategory_EditUserID { get; set; }
		public DateTime? ProductCategory_EditDate { get; set; }
		public string? ProductCategory_DeleteUserID { get; set; }
		public DateTime? ProductCategory_DeleteDate { get; set; }
	}
}

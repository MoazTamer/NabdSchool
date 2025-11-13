using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesModel.Models
{
    public class ApplicationUser : IdentityUser
	{
		public int Branch_ID { get; set; }
		[ForeignKey("Branch_ID")]
		public virtual TblBranch TblBranch { get; set; }
        public int CashBalance_ID { get; set; }
        public int CashBalanceBank_ID { get; set; }
        public string Category { get; set; }
		public string UserType { get; set; }
		public string Password { get; set; }
		public string Visible { get; set; }
	}
}

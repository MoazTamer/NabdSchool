using System.ComponentModel.DataAnnotations;

namespace SalesModel.Models
{
	public class TblVat
	{
		[Key]
		public int VatID { get; set; }
		public double VatValue { get; set; }
		public double VatPercent { get; set; }
	}
}

using Microsoft.AspNetCore.Mvc.Rendering;

namespace SalesModel.ViewModels
{
	public class ModelBranch
    {
        public int Branch_ID { get; set; }
        public string? Branch_Name { get; set; }

        public double ProductBarcode_TotalSum { get; set; }
        public double ProductBarcode_TotalSumVat { get; set; }
        public double ProductBarcode_TotalPayPriceSum { get; set; }
        public double ProductBarcode_TotalPayPriceSumVat { get; set; }
    }
}

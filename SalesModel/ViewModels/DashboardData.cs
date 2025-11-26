using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.ViewModels
{
    public class DashboardData
    {
        public int Present { get; set; }
        public int Late { get; set; }
        public int Absent { get; set; }
        public int Disciplined { get; set; }
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<int> ChartData { get; set; } = new List<int>();
    }

}

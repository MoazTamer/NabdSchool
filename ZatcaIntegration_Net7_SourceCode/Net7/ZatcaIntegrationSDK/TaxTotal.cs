using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public class TaxTotal
    {
        /// <summary>
        /// مبلغ الضريبة
        /// 
        /// </summary>
        public decimal TaxAmount { set; get; }
        /// <summary>
        /// المبلغ شامل الضريبة
        /// 
        /// </summary>
        public decimal RoundingAmount { set; get; }
        /// <summary>
        /// تفاصيل الضريبة
        /// 
        /// </summary>

        public TaxSubtotal TaxSubtotal = new TaxSubtotal();
        public void AddSubtotal(TaxSubtotal _taxsubtotal)
        {
            this.TaxSubtotal = _taxsubtotal;
        }
        public TaxSubtotalCollection TaxSubtotalList = new TaxSubtotalCollection();
    }
}


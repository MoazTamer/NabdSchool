using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace  ZatcaIntegrationSDK
{

    public class Result
    {
        public string Operation { get; set; }

        public bool IsValid { get; set; }

        public string ErrorMessage { get; set; }
        public string WarningMessage { get; set; }

        public string ResultedValue { get; set; }

        public ResultCollection lstSteps { get; set; }

        public string SingedXML { get; set; }
        public string SingedXMLFileName { get; set; }
        public string SingedXMLFileNameFullPath { get; set; }
        public string InvoiceHash { get; set; }
        public string UUID { get; set; }
        public string EncodedInvoice { get; set; }
        public string PIH { get; set; }
        public string QRCode { get; set; }
        public string LineExtensionAmount { get; set; }
        public string TaxExclusiveAmount { get; set; }
        public string TaxInclusiveAmount { get; set; }
        public string AllowanceTotalAmount { get; set; }
        public string ChargeTotalAmount { get; set; }
        public string PayableAmount { get; set; }
        public string PrepaidAmount { get; set; }
        public string TaxAmount { get; set; }
        public string PayableRoundingAmount { set; get; }
        public string SingedXMLFileNameShortPath { get; set; }
        public string NormalXMLFileNameFullPath { get; set; }
        public string NormalXMLFileNameShortPath { get; set; }

    }
}
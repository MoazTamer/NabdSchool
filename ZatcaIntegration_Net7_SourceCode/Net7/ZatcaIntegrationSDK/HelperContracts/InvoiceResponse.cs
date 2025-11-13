namespace ZatcaIntegrationSDK.HelperContracts
{
    public class InvoiceResponse
    {
        public ValidationResults validationResults { get; set; }
        /// <summary>
        /// Õ«·… «·„‘«—ﬂ… »«·‰”»… ··›« Ê—… «·„»”ÿ… 
        /// 
        /// REPORTED  „ ﬁ»Ê· «·›« Ê—…
        /// 
        /// NOT_REPORTED ·„ Ì „ ﬁ»Ê· «·›« Ê—…
        /// Õ«·… «·«⁄ „«œ »«·‰”»… ··›« Ê—… «·÷—Ì»Ì… 
        /// 
        /// CLEARED  „ ﬁ»Ê· «·›« Ê—…
        /// 
        /// NOT_CLEARED ·„ Ì „ ﬁ»Ê· «·›« Ê—…
        /// </summary>
        public string SendingStatus { get; set; }
        /// <summary>
        /// Cleared XML base64 format
        /// </summary>
        public string ClearedInvoice { get; set; }
        /// <summary>
        /// ﬂÌÊ «— ﬂÊœ »⁄œ «·«⁄ „«œ
        /// </summary>
        public string QRCode { get; set; }
        /// <summary>
        /// Cleared XML PlainText
        /// </summary>
        public string SingedXML { get; set; }

        /// <summary>
        /// —”«·… «·Œÿ√ 
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// —”«·… «· Õ–Ì— 
        /// </summary>
        public string WarningMessage { get; set; }
        //public string WarningMessage
        //{
        //    get
        //    {
        //        if (this.validationResults != null)
        //        {
        //            if (this.validationResults.WarningMessages == null)
        //            { return string.Empty; }
        //            else
        //                return validationResults.WarningMessages.ToWarnings();
        //        }
        //        return string.Empty;
        //    }
        //}

        /// <summary>
        /// true ›Ï Õ«·… «—”«· «·›« Ê—… »‰Ã«Õ ··“ﬂ«… Ê«·œŒ· 
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 200 XML Accepted
        /// -------
        /// 202 Accepted with warnings
        /// ------
        /// 400 Bad request - must solve the error and create new xml
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// «”„ „·› xml «·„⁄ „œ 
        /// </summary>
        public string ClearedXMLFileName { get; set; }
        /// <summary>
        /// „”«— „·› xml »⁄œ «·«⁄ „«œ 
        /// </summary>
        public string ClearedXMLFileNameFullPath { get; set; }
        /// <summary>
        /// „”«— ﬁ’Ì— „·› xml »⁄œ «·«⁄ „«œ 
        /// </summary>
        public string ClearedXMLFileNameShortPath { get; set; }
    }
}

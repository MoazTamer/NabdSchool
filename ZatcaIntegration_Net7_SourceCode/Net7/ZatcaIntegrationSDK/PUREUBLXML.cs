using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace ZatcaIntegrationSDK
{
    public class PUREUBLXML
    {
        public PUREUBLXML(string _lang = "EN")
        {
            this.lang = _lang.ToUpper();
        }
        //main function
        bool IsDisposing = false;
        bool SaveXML = false;
        bool ValidateXML = true;
        public string lang = "";
        /// <summary>
        /// Generate Invoice As UBL XML with Zatca Standared Rules .
        /// 
        /// and return all needed xml data to save into database and send to Zatca
        /// </summary>
        /// <param name="inv">All needed Invoice Data from Sales Invoice Solution .</param>
        /// <param name="Directorypath">physical Directory in server to save xml .</param>
        /// <param name="savexml">true when need to save xml as a file .</param>
        /// <returns>
        /// Result object with Result.IsValid=true if the invoice xml created successfully .
        /// 
        /// if IsValid=false Read Result.ErrorMessage
        /// </returns>
        public Result GenerateInvoiceXML(Invoice inv, string Directorypath, bool savexml = true)
        {
            this.SaveXML = savexml;
            Result result = new Result();
            result.IsValid = false;
            if (savexml)
            {
                Utility.CreateInvoicesFolder(Directorypath);
            }
            //if (!CheckTrail(inv))
            //{
            //    result.ErrorMessage = "لقت انتهت الفترة التجريبية على هذة المكتبة \n لشراء النسخة المدفوعة برجاء التواصل معنا على واتس آب +201090838734";

            //    return result;
            //}
            if (!(inv.invoiceTypeCode.id == 381 || inv.invoiceTypeCode.id == 383 || inv.invoiceTypeCode.id == 388 || inv.invoiceTypeCode.id == 386))
            {
                result.ErrorMessage = "Invalid Invoice Type Code ! ";
                return result;
            }
            if (inv.invoiceTypeCode.Name.Length != 7)
            {
                result.ErrorMessage = "Invalid Invoice Type Code Name Example (0100000) For Simplified Invoice!";
                return result;
            }
            string invoicetype = inv.invoiceTypeCode.Name.Substring(0, 2);
            if (inv.invoiceTypeCode.id == 381) //credit note
            {
                if (invoicetype == "01")// standard
                {
                    result = GenerateStandardCreditXML(inv, Directorypath);
                    return result;
                }
                else if (invoicetype == "02")// simplified
                {
                    result = GenerateSimplifiedCreditXML(inv, Directorypath);
                    return result;

                }
                else
                {
                    result.ErrorMessage = "Invalid Invoice Type Code !";
                    return result;
                }
            }
            else if (inv.invoiceTypeCode.id == 383) // debit note
            {
                if (invoicetype == "01")// standard
                {
                    result = GenerateStandardDebitXML(inv, Directorypath);
                    return result;
                }
                else if (invoicetype == "02")// simplified
                {
                    result = GenerateSimplifiedDebitXML(inv, Directorypath);
                    return result;
                }
                else
                {
                    result.ErrorMessage = "Invalid Invoice Type Code !";
                    return result;
                }
            }
            else if (inv.invoiceTypeCode.id == 388) // invoice
            {
                if (invoicetype == "01") // standard
                {
                    result = GenerateStandardInvoiceXML(inv, Directorypath);
                    return result;
                }
                else if (invoicetype == "02") // simplified
                {
                    result = GenerateSimplifiedInvoiceXML(inv, Directorypath);
                    //result.ErrorMessage = "Test test test";
                    return result;
                }
                else
                {
                    result.ErrorMessage = "Invalid Invoice Type Code !";
                    return result;
                }
            }
            else if (inv.invoiceTypeCode.id == 386) // Prepaid invoice
            {
                if (invoicetype == "01") // standard
                {
                    result = GenerateStandardInvoiceXML(inv, Directorypath);
                    return result;
                }
                else if (invoicetype == "02") // simplified
                {
                    result = GenerateSimplifiedInvoiceXML(inv, Directorypath);
                    //result.ErrorMessage = "Test test test";
                    return result;
                }
                else
                {
                    result.ErrorMessage = "Invalid Invoice Type Code !";
                    return result;
                }
            }
            else
            {
                result.ErrorMessage = "Invalid Invoice Type Code !";
                return result;
            }

        }
        private Result GenerateStandardInvoiceXML(Invoice inv, string Directorypath)
        {
            Result result = new Result();
            StringBuilder sb = new StringBuilder();
            string error = "";
            GetCommonInvoiceTagElements(inv, sb);
            GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
            GetPIHElement(Directorypath, inv, sb, ref error);
            GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
            GetAccountingCustomerPartyElement(inv, sb, ref error);
            GetDeliveryElement(inv, sb, ref error);
            GetPaymentMeansElement(inv, sb, ref error);
            GetallowanceChargeElement(inv, sb);
            GetDocumentTaxTotal(inv, sb, ref error);
            GetLegalMonetaryTotal(inv, sb, ref error);
            GetInvoiceLineElement(inv, sb);
            sb.Append("</Invoice>");
            if (!string.IsNullOrEmpty(error))
            {
                result.ErrorMessage = error;
                result.IsValid = false;
                return result;
            }
            string returnnormalxmldir = SettingsParams.NormalStandardInvoicePath;
            string returnsignedxmldir = SettingsParams.StandardInvoicePath;
            SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);
            return result;
        }
        private Result GenerateSimplifiedInvoiceXML(Invoice inv, string Directorypath)
        {
            Result result = new Result();
            StringBuilder sb = new StringBuilder();
            string error = "";

            GetCommonInvoiceTagElements(inv, sb);
            GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
            GetPIHElement(Directorypath, inv, sb, ref error);
            GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
            GetAccountingCustomerPartyElement(inv, sb, ref error);
            GetDeliveryElement(inv, sb, ref error);
            GetPaymentMeansElement(inv, sb, ref error);
            GetallowanceChargeElement(inv, sb);
            GetDocumentTaxTotal(inv, sb, ref error);
            GetLegalMonetaryTotal(inv, sb, ref error);
            GetInvoiceLineElement(inv, sb);
            sb.Append("</Invoice>");

            if (!string.IsNullOrEmpty(error))
            {
                result.ErrorMessage = error;
                result.IsValid = false;
                return result;
            }

            string returnnormalxmldir = SettingsParams.NormalSimplifiedInvoicePath;
            string returnsignedxmldir = SettingsParams.SimplifiedInvoicePath;
            SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);

            return result;
        }
        private Result GenerateStandardCreditXML(Invoice inv, string Directorypath)
        {
            Result result = new Result();
            StringBuilder sb = new StringBuilder();
            string error = "";
            GetCommonInvoiceTagElements(inv, sb);
            GetInvoiceDocumentReferenceElement(inv.billingReference.invoiceDocumentReferences, sb);
            GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
            GetPIHElement(Directorypath, inv, sb, ref error);
            GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
            GetAccountingCustomerPartyElement(inv, sb, ref error);
            GetDeliveryElement(inv, sb, ref error);
            GetPaymentMeansElement(inv, sb, ref error);
            GetallowanceChargeElement(inv, sb);
            GetDocumentTaxTotal(inv, sb, ref error);
            GetLegalMonetaryTotal(inv, sb, ref error);
            GetInvoiceLineElement(inv, sb);
            sb.Append("</Invoice>");

            if (!string.IsNullOrEmpty(error))
            {
                result.ErrorMessage = error;
                result.IsValid = false;
                return result;
            }

            string returnnormalxmldir = SettingsParams.NormalStandardCreditPath;
            string returnsignedxmldir = SettingsParams.StandardCreditPath;
            SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);
            return result;
        }
        private Result GenerateSimplifiedCreditXML(Invoice inv, string Directorypath)
        {
            Result result = new Result();
            StringBuilder sb = new StringBuilder();
            string error = "";
            GetCommonInvoiceTagElements(inv, sb);
            GetInvoiceDocumentReferenceElement(inv.billingReference.invoiceDocumentReferences, sb);
            GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
            GetPIHElement(Directorypath, inv, sb, ref error);
            GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
            GetAccountingCustomerPartyElement(inv, sb, ref error);
            GetDeliveryElement(inv, sb, ref error);
            GetPaymentMeansElement(inv, sb, ref error);
            GetallowanceChargeElement(inv, sb);
            GetDocumentTaxTotal(inv, sb, ref error);
            GetLegalMonetaryTotal(inv, sb, ref error);
            GetInvoiceLineElement(inv, sb);
            sb.Append("</Invoice>");
            if (!string.IsNullOrEmpty(error))
            {
                result.ErrorMessage = error;
                result.IsValid = false;
                return result;
            }

            string returnnormalxmldir = SettingsParams.NormalSimplifiedCreditPath;
            string returnsignedxmldir = SettingsParams.SimplifiedCreditPath;
            SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);
            return result;
        }
        private Result GenerateStandardDebitXML(Invoice inv, string Directorypath)
        {
            Result result = new Result();
            StringBuilder sb = new StringBuilder();
            string error = "";
            GetCommonInvoiceTagElements(inv, sb);
            GetInvoiceDocumentReferenceElement(inv.billingReference.invoiceDocumentReferences, sb);
            GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
            GetPIHElement(Directorypath, inv, sb, ref error);
            GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
            GetAccountingCustomerPartyElement(inv, sb, ref error);
            GetDeliveryElement(inv, sb, ref error);
            GetPaymentMeansElement(inv, sb, ref error);
            GetallowanceChargeElement(inv, sb);
            GetDocumentTaxTotal(inv, sb, ref error);
            GetLegalMonetaryTotal(inv, sb, ref error);
            GetInvoiceLineElement(inv, sb);
            sb.Append("</Invoice>");

            if (!string.IsNullOrEmpty(error))
            {
                result.ErrorMessage = error;
                result.IsValid = false;
                return result;
            }

            string returnnormalxmldir = SettingsParams.NormalStandardDebitPath;
            string returnsignedxmldir = SettingsParams.StandardDebitPath;
            SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);
            return result;
        }
        private Result GenerateSimplifiedDebitXML(Invoice inv, string Directorypath)
        {
            Result result = new Result();
            StringBuilder sb = new StringBuilder();
            string error = "";
            GetCommonInvoiceTagElements(inv, sb);
            GetInvoiceDocumentReferenceElement(inv.billingReference.invoiceDocumentReferences, sb);
            GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
            GetPIHElement(Directorypath, inv, sb, ref error);
            GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
            GetAccountingCustomerPartyElement(inv, sb, ref error);
            GetDeliveryElement(inv, sb, ref error);
            GetPaymentMeansElement(inv, sb, ref error);
            GetallowanceChargeElement(inv, sb);
            GetDocumentTaxTotal(inv, sb, ref error);
            GetLegalMonetaryTotal(inv, sb, ref error);
            GetInvoiceLineElement(inv, sb);
            sb.Append("</Invoice>");

            if (!string.IsNullOrEmpty(error))
            {
                result.ErrorMessage = error;
                result.IsValid = false;
                return result;
            }

            string returnnormalxmldir = SettingsParams.NormalSimplifiedDebitPath;
            string returnsignedxmldir = SettingsParams.SimplifiedDebitPath;
            SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);

            return result;
        }
        public bool GenerateSampleXML(Invoice inv, string invoicetypename, int invoicetypeid, string Directorypath, ref string xmldoc, ref string ErrorMessage)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                //string InvoiceTypeString = "";
                inv.UUID = Guid.NewGuid().ToString();
                inv.invoiceTypeCode.id = invoicetypeid;
                inv.invoiceTypeCode.Name = invoicetypename;
                inv.legalMonetaryTotal.PrepaidAmount = 0;
                inv.legalMonetaryTotal.PayableAmount = 0;
                string invoicetype = inv.invoiceTypeCode.Name.Substring(0, 2);
                GetCommonInvoiceTagElements(inv, sb);
                if (inv.invoiceTypeCode.id == 383 || inv.invoiceTypeCode.id == 381)
                {
                    GetInvoiceDocumentReferenceElement(inv.billingReference.invoiceDocumentReferences, sb);
                }
                GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref ErrorMessage);
                GetPIHElement(Directorypath, inv, sb, ref ErrorMessage);
                GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
                GetAccountingCustomerPartyElement(inv, sb, ref ErrorMessage);
                GetDeliveryElement(inv, sb, ref ErrorMessage);
                GetPaymentMeansElement(inv, sb, ref ErrorMessage);
                GetallowanceChargeElement(inv, sb);
                GetDocumentTaxTotal(inv, sb, ref ErrorMessage);
                GetLegalMonetaryTotal(inv, sb, ref ErrorMessage);
                GetInvoiceLineElement(inv, sb);
                sb.Append("</Invoice>");
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    return false;
                }
                xmldoc = sb.ToString();

                return true;

            }
            catch (Exception ex)
            {
                ErrorMessage = ex.InnerException.ToString();
                return false;
            }

        }
        private void SignDocument(string Directorypath, string xmldocument, string returnsignedxmldir, string returnnormalxmldir, Invoice inv, ref Result result)
        {
            try
            {

                XDocument d = XDocument.Parse(xmldocument, LoadOptions.PreserveWhitespace);
                XmlDocument doc = new XmlDocument();
                //doc.LoadXml(xmldocument);
                //XmlDocument xmlDocument = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.LoadXml("<?xml version='1.0' encoding='UTF-8'?>\r\n" + d.ToString());
                string issuedate = "";
                string issuetime = "";
                if (!CheckIssueDateFormat(inv.IssueDate, inv.IssueTime))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Wrong issuedate or issuetime format";
                    return;
                }
                if (!string.IsNullOrEmpty(inv.IssueDate))
                    issuedate = inv.IssueDate.Replace("-", "");
                if (!string.IsNullOrEmpty(inv.IssueTime))
                    issuetime = inv.IssueTime.Replace(":", "");
                //Save the document to a file.
                string xmlfile = inv.SupplierParty.partyTaxScheme.CompanyID + "_" + issuedate + "T" + issuetime + "_" + RemoveNonAlphanumeric(inv.ID) + ".xml";
                string xmlfilename = Directorypath + returnnormalxmldir + xmlfile;
                string signedxmlfilename = Directorypath + returnsignedxmldir + xmlfile;
                string signedxmlshortpath = returnsignedxmldir + xmlfile;
                string normalxmlfilename = Directorypath + returnnormalxmldir + xmlfile;
                string normalxmlshortpath = returnnormalxmldir + xmlfile;
                if (this.SaveXML)
                {
                    //FileMode.Create will overwrite the file.No seek and truncate is needed.
                    using (var fs = new FileStream(xmlfilename, FileMode.Create))
                    {
                        doc.Save(fs);
                    }
                }

                //doc.Save(xmlfilename);
                var doc1 = new XmlDocument();
                string cert = "";
                string privatekey = "";
                if (string.IsNullOrEmpty(inv.cSIDInfo.CertPem))
                {
                    try
                    {
                        cert = string.Join("", File.ReadAllLines(Directorypath + "\\cert\\cert.pem"));
                    }
                    catch
                    {
                        result.IsValid = false;
                        result.ErrorMessage = "Certificate Pem Doesn't exist";
                        return;
                    }

                }
                else
                {
                    cert = inv.cSIDInfo.CertPem;
                }
                if (string.IsNullOrEmpty(inv.cSIDInfo.PrivateKey))
                {
                    try
                    {
                        privatekey = string.Join("", File.ReadAllLines(Directorypath + "\\cert\\key.pem"));
                    }
                    catch
                    {
                        result.IsValid = false;
                        result.ErrorMessage = "Private Key Doesn't exist";
                        return;
                    }

                }
                else
                {
                    privatekey = inv.cSIDInfo.PrivateKey;
                }
                using (BLL.EInvoiceSigningLogic logic = new BLL.EInvoiceSigningLogic())
                {
                    result = logic.SignDocument(doc, cert, privatekey);
                }

                if (result.IsValid)
                {
                    doc1.PreserveWhitespace = true;
                    doc1.LoadXml("<?xml version='1.0' encoding='UTF-8'?>" + result.ResultedValue);
                    if (this.SaveXML)
                    {
                        using (var fs = new FileStream(signedxmlfilename, FileMode.Create))
                        {
                            doc1.Save(fs);
                        }
                    }

                    //doc1.Save(signedxmlfilename);
                    result.SingedXML = result.ResultedValue;
                    result.EncodedInvoice = Utility.ToBase64Encode(result.ResultedValue);
                    result.UUID = Utility.GetNodeInnerText(doc1, SettingsParams.UUID_XPATH);
                    result.InvoiceHash = Utility.GetNodeInnerText(doc1, SettingsParams.Hash_XPATH);
                    result.QRCode = Utility.GetNodeInnerText(doc1, SettingsParams.QR_CODE_XPATH);
                    result.PIH = Utility.GetNodeInnerText(doc1, SettingsParams.PIH_XPATH);
                    result.LineExtensionAmount = Utility.GetNodeInnerText(doc1, SettingsParams.LineExtensionAmount);
                    result.TaxExclusiveAmount = Utility.GetNodeInnerText(doc1, SettingsParams.TaxExclusiveAmount);
                    result.TaxInclusiveAmount = Utility.GetNodeInnerText(doc1, SettingsParams.TaxInclusiveAmount);
                    result.AllowanceTotalAmount = Utility.GetNodeInnerText(doc1, SettingsParams.AllowanceTotalAmount);
                    result.ChargeTotalAmount = Utility.GetNodeInnerText(doc1, SettingsParams.ChargeTotalAmount);
                    result.PayableAmount = Utility.GetNodeInnerText(doc1, SettingsParams.PayableAmount);
                    result.PrepaidAmount = Utility.GetNodeInnerText(doc1, SettingsParams.PrepaidAmount);
                    result.PayableRoundingAmount = Utility.GetNodeInnerText(doc1, SettingsParams.PayableRoundingAmount);
                    result.TaxAmount = Utility.GetNodeInnerText(doc1, SettingsParams.VAT_TOTAL_XPATH);
                    result.SingedXMLFileName = xmlfile;
                    result.SingedXMLFileNameFullPath = signedxmlfilename;
                    result.SingedXMLFileNameShortPath = signedxmlshortpath;
                    result.NormalXMLFileNameFullPath = normalxmlfilename;
                    result.NormalXMLFileNameShortPath = normalxmlshortpath;
                    if (this.ValidateXML)
                    {
                        //set the language of error message the default is arabic with code AR and English with code EN

                        Result validationresult = new Result();
                        result.WarningMessage = "";
                        result.ErrorMessage = "";
                        using (BLL.EInvoiceValidator vali = new BLL.EInvoiceValidator(this.lang))
                        {
                            validationresult = vali.ValidateEInvoice(doc1, cert, result.PIH);

                        }
                        if (!validationresult.IsValid)
                        {
                            if (!string.IsNullOrEmpty(validationresult.ErrorMessage))
                            {
                                result.ErrorMessage += validationresult.ErrorMessage + "\n";
                            }
                            foreach (Result r in validationresult.lstSteps)
                            {
                                if (r.IsValid == false)
                                {
                                    if (!string.IsNullOrEmpty(r.ErrorMessage))
                                    {
                                        result.ErrorMessage += r.ErrorMessage + "\n";
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(r.WarningMessage))
                                    {
                                        if (!result.WarningMessage.Contains(r.WarningMessage))
                                            result.WarningMessage += r.WarningMessage + "\n";
                                    }

                                }
                            }
                            result.IsValid = false;
                        }
                        else
                        {
                            foreach (Result r in validationresult.lstSteps)
                            {
                                if (r.IsValid == true && !string.IsNullOrEmpty(r.WarningMessage))
                                {
                                    if (!result.WarningMessage.Contains(r.WarningMessage))
                                        result.WarningMessage += r.WarningMessage + "\n";
                                }
                            }
                            result.IsValid = true;
                            try
                            {
                                File.WriteAllText(Directorypath + "\\PIH\\pih.txt", string.Empty);
                                File.WriteAllText(Directorypath + "\\PIH\\pih.txt", result.InvoiceHash);
                            }
                            catch
                            {

                            }
                        }
                    }
                    else
                    {
                        result.IsValid = true;
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        result.ErrorMessage += result.ErrorMessage + "\n";
                    }
                    foreach (Result r in result.lstSteps)
                    {
                        if (r.IsValid == false)
                        {
                            result.ErrorMessage += r.ErrorMessage + "\n";
                        }
                    }
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException;
            }
        }


        public Result ReSendXMLFile(string FilePath)
        {
            Result result = new Result();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(FilePath);
                result.ResultedValue = doc.OuterXml;
                result.SingedXML = doc.OuterXml;
                result.EncodedInvoice = Utility.ToBase64Encode(result.ResultedValue);
                result.UUID = Utility.GetNodeInnerText(doc, SettingsParams.UUID_XPATH);
                result.InvoiceHash = Utility.GetNodeInnerText(doc, SettingsParams.Hash_XPATH);
                result.QRCode = Utility.GetNodeInnerText(doc, SettingsParams.QR_CODE_XPATH);
                result.PIH = Utility.GetNodeInnerText(doc, SettingsParams.PIH_XPATH);
                if (string.IsNullOrEmpty(result.UUID) || string.IsNullOrEmpty(result.InvoiceHash) || string.IsNullOrEmpty(result.EncodedInvoice))
                {
                    result.ErrorMessage = "ملف XML غير قابل للإرسال";
                    result.IsValid = false;
                }
                else
                {
                    result.IsValid = true;
                }

            }
            catch
            {
                result.ErrorMessage = "حدث خطأ أثناء تحميل ملف XML";
                result.IsValid = false;
            }
            return result;
        }
        public Result ReSendXMLText(string xmlstring)
        {
            Result result = new Result();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.LoadXml(xmlstring);
                result.ResultedValue = doc.OuterXml;
                result.SingedXML = doc.OuterXml;
                result.EncodedInvoice = Utility.ToBase64Encode(result.ResultedValue);
                result.UUID = Utility.GetNodeInnerText(doc, SettingsParams.UUID_XPATH);
                result.InvoiceHash = Utility.GetNodeInnerText(doc, SettingsParams.Hash_XPATH);
                result.QRCode = Utility.GetNodeInnerText(doc, SettingsParams.QR_CODE_XPATH);
                result.PIH = Utility.GetNodeInnerText(doc, SettingsParams.PIH_XPATH);
                if (string.IsNullOrEmpty(result.UUID) || string.IsNullOrEmpty(result.InvoiceHash) || string.IsNullOrEmpty(result.EncodedInvoice))
                {
                    result.ErrorMessage = "ãáÝ xml ÛíÑ ÕÇáÍ";
                    result.IsValid = false;
                }
                else
                {
                    result.ErrorMessage = "ÍÏË ÎØÃ ÇËäÇÁ ÊÍãíá ãáÝ xml";
                    result.IsValid = true;
                }
            }
            catch
            {
                result.IsValid = false;
            }
            return result;
        }

        #region Common Methods
        //private bool CheckTrail(Invoice inv)
        //{
        //    try
        //    {
        //        DateTime invoicedate = default(DateTime);

        //        DateTime.TryParseExact(inv.IssueDate, SettingsParams.allDatesFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out invoicedate);
        //        DateTime trailenddate = default(DateTime);

        //        DateTime.TryParseExact("2025-04-01", SettingsParams.allDatesFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out trailenddate);

        //        if (invoicedate > trailenddate)
        //            return false;
        //        else
        //            return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //}
        private void GetCommonInvoiceTagElements(Invoice inv, StringBuilder sb)
        {
            inv.ProfileID = "reporting:1.0";
            if (string.IsNullOrEmpty(inv.UUID))
            { inv.UUID = Guid.NewGuid().ToString(); }
            //if (string.IsNullOrEmpty(inv.TaxCurrencyCode))
            inv.TaxCurrencyCode = "SAR";
            if (string.IsNullOrEmpty(inv.DocumentCurrencyCode))
            {
                inv.DocumentCurrencyCode = "SAR";
            }
            else
            {
                inv.DocumentCurrencyCode = inv.DocumentCurrencyCode.ToUpper();
            }
            sb.Append("<?xml version='1.0' encoding='UTF-8'?>");
            sb.Append("<Invoice xmlns='urn:oasis:names:specification:ubl:schema:xsd:Invoice-2' xmlns:cac='urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2' xmlns:cbc='urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2' xmlns:ext='urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2'>");
            sb.Append("<cbc:ProfileID>" + inv.ProfileID + "</cbc:ProfileID>");
            sb.Append("<cbc:ID>" + inv.ID + "</cbc:ID>");
            sb.Append("<cbc:UUID>" + inv.UUID + "</cbc:UUID>");
            sb.Append("<cbc:IssueDate>" + inv.IssueDate + "</cbc:IssueDate>");
            //if (!string.IsNullOrEmpty(inv.IssueTime))
            //    inv.IssueTime = inv.IssueTime.Replace("Z", "") + "Z";

            sb.Append("<cbc:IssueTime>" + inv.IssueTime + "</cbc:IssueTime>");
            sb.Append("<cbc:InvoiceTypeCode name='" + inv.invoiceTypeCode.Name + "'>" + inv.invoiceTypeCode.id + "</cbc:InvoiceTypeCode>");
            sb.Append("<cbc:DocumentCurrencyCode>" + inv.DocumentCurrencyCode + "</cbc:DocumentCurrencyCode>");

            sb.Append("<cbc:TaxCurrencyCode>" + inv.TaxCurrencyCode + "</cbc:TaxCurrencyCode>");
            sb.Append("<cbc:LineCountNumeric>" + inv.InvoiceLines.Count + "</cbc:LineCountNumeric>");
        }
        private void GetPIHElement(string Directorypath, Invoice inv, StringBuilder sb, ref string error)
        {
            try
            {
                string pih = "";
                if (!string.IsNullOrEmpty(inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject))
                {
                    pih = inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject.Trim();
                }
                else
                {
                    pih = File.ReadAllText(Directorypath + "\\PIH\\pih.txt").Trim();
                }
                sb.Append("<cac:AdditionalDocumentReference>" +
                    "<cbc:ID>PIH</cbc:ID>" +
                    "<cac:Attachment>" +
                    "<cbc:EmbeddedDocumentBinaryObject mimeCode='text/plain'>" + pih +
                    "</cbc:EmbeddedDocumentBinaryObject>" +
                    "</cac:Attachment>" +
                    "</cac:AdditionalDocumentReference>");
            }
            catch
            {
                //error += ex.InnerException.ToString() + "\n";
                error += "PIH doesn't exist.";
            }
        }
        private void GetICVElement(long icv, StringBuilder sb, ref string error)
        {

            try
            {
                if (icv != 0)
                {
                    sb.Append("<cac:AdditionalDocumentReference>" +
                        "<cbc:ID>ICV</cbc:ID>" +
                        "<cbc:UUID>" + icv + "</cbc:UUID>" +
                        "</cac:AdditionalDocumentReference>");

                }
                else
                {
                    error += "\n";
                    error += "Error in Invoice Counter Value.";
                }
            }
            catch
            {
                error += "\n";
                error += "Error in Invoice Counter Value.";
            }


        }
        private void GetInvoiceDocumentReferenceElement(InvoiceDocumentReferenceCollection InvoiceDocumentReferences, StringBuilder sb)
        {
            //this is for debit and credit notes 
            if (InvoiceDocumentReferences != null)
            {
                foreach (InvoiceDocumentReference doc in InvoiceDocumentReferences)
                {
                    if (!string.IsNullOrEmpty(doc.ID))
                    {
                        sb.Append("<cac:BillingReference>" +
                            "<cac:InvoiceDocumentReference>" +
                            "<cbc:ID>" + doc.ID.Trim() + "</cbc:ID>" +
                            "</cac:InvoiceDocumentReference>" +
                            "</cac:BillingReference>");

                    }
                }
            }

        }

        private string RemoveNonAlphanumeric(string str)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            str = rgx.Replace(str, "");
            return str;
        }
        private void GetAccountingSupplierPartyElement(AccountingSupplierParty supplier, StringBuilder sb)
        {

            sb.Append("<cac:AccountingSupplierParty>" +
                "<cac:Party>");
            if (!string.IsNullOrEmpty(supplier.partyIdentification.ID))
            {
                if (!string.IsNullOrEmpty(supplier.partyIdentification.schemeID))
                {
                    supplier.partyIdentification.schemeID = supplier.partyIdentification.schemeID.Trim().ToUpper();
                }
                else
                {
                    supplier.partyIdentification.schemeID = "CRN";
                }
                sb.Append("<cac:PartyIdentification>" +
                "<cbc:ID schemeID='" + supplier.partyIdentification.schemeID + "'>" + supplier.partyIdentification.ID.Trim() + "</cbc:ID>" +
                "</cac:PartyIdentification>");
            }
            sb.Append("<cac:PostalAddress>");
            if (!string.IsNullOrEmpty(supplier.postalAddress.StreetName))
            {
                sb.Append("<cbc:StreetName>" + Utility.ReplaceXMLSpecialCharacters(supplier.postalAddress.StreetName.Trim()) + "</cbc:StreetName>");
            }
            if (!string.IsNullOrEmpty(supplier.postalAddress.AdditionalStreetName))
            {
                sb.Append("<cbc:AdditionalStreetName>" + Utility.ReplaceXMLSpecialCharacters(supplier.postalAddress.AdditionalStreetName.Trim()) + "</cbc:AdditionalStreetName>");
            }
            if (!string.IsNullOrEmpty(supplier.postalAddress.BuildingNumber))
            {
                sb.Append("<cbc:BuildingNumber>" + supplier.postalAddress.BuildingNumber.Trim() + "</cbc:BuildingNumber>");
            }

            if (!string.IsNullOrEmpty(supplier.postalAddress.PlotIdentification))
            {
                sb.Append("<cbc:PlotIdentification>" + supplier.postalAddress.PlotIdentification.Trim() + "</cbc:PlotIdentification>");
            }
            if (!string.IsNullOrEmpty(supplier.postalAddress.CitySubdivisionName))
            {
                sb.Append("<cbc:CitySubdivisionName>" + Utility.ReplaceXMLSpecialCharacters(supplier.postalAddress.CitySubdivisionName.Trim()) + "</cbc:CitySubdivisionName>");
            }
            if (!string.IsNullOrEmpty(supplier.postalAddress.CityName))
            {
                sb.Append("<cbc:CityName>" + Utility.ReplaceXMLSpecialCharacters(supplier.postalAddress.CityName.Trim()) + "</cbc:CityName>");
            }
            if (!string.IsNullOrEmpty(supplier.postalAddress.PostalZone))
            {
                sb.Append("<cbc:PostalZone>" + supplier.postalAddress.PostalZone.Trim() + "</cbc:PostalZone>");
            }
            if (!string.IsNullOrEmpty(supplier.postalAddress.CountrySubentity))
            {
                sb.Append("<cbc:CountrySubentity>" + Utility.ReplaceXMLSpecialCharacters(supplier.postalAddress.CountrySubentity.Trim()) + "</cbc:CountrySubentity>");
            }
            if (string.IsNullOrEmpty(supplier.postalAddress.country.IdentificationCode))
            {
                supplier.postalAddress.country.IdentificationCode = "SA";
            }
            sb.Append("<cac:Country>" +
                "<cbc:IdentificationCode>" + supplier.postalAddress.country.IdentificationCode.Trim().ToUpper() + "</cbc:IdentificationCode>" +
                "</cac:Country>");
            sb.Append("</cac:PostalAddress>");
            if (!string.IsNullOrEmpty(supplier.partyTaxScheme.CompanyID))
            {
                sb.Append("<cac:PartyTaxScheme>" +
                    "<cbc:CompanyID>" + supplier.partyTaxScheme.CompanyID.Trim() + "</cbc:CompanyID>" +
                    "<cac:TaxScheme>" +
                "<cbc:ID>VAT</cbc:ID>" +
                "</cac:TaxScheme>" +
                "</cac:PartyTaxScheme>");
            }
            if (!string.IsNullOrEmpty(supplier.partyLegalEntity.RegistrationName))
            {
                sb.Append("<cac:PartyLegalEntity>" +
                "<cbc:RegistrationName>" + Utility.ReplaceXMLSpecialCharacters(supplier.partyLegalEntity.RegistrationName.Trim()) + "</cbc:RegistrationName>" +
                "</cac:PartyLegalEntity>");
            }
            sb.Append("</cac:Party>" +
            "</cac:AccountingSupplierParty>");
        }
        private void GetAccountingCustomerPartyElement(Invoice inv, StringBuilder sb, ref string error)
        {
            AccountingCustomerParty customer = inv.CustomerParty;
            bool issimplified = inv.invoiceTypeCode.Name.Substring(0, 2) == "02";
            bool issummary = inv.invoiceTypeCode.Name.Substring(5, 1) == "1";
            bool exportinvoice = inv.invoiceTypeCode.Name.Substring(4, 1) == "1";
            //error = "";
            if (issimplified && issummary && string.IsNullOrEmpty(customer.partyLegalEntity.RegistrationName))
            {
                error += "\n";
                error += "You must Enter Customer registration name";
            }
            if (string.IsNullOrEmpty(customer.partyIdentification.ID)
                && string.IsNullOrEmpty(customer.postalAddress.StreetName)
                && string.IsNullOrEmpty(customer.postalAddress.AdditionalStreetName)
                && string.IsNullOrEmpty(customer.postalAddress.BuildingNumber)
                && string.IsNullOrEmpty(customer.postalAddress.PlotIdentification)
                && string.IsNullOrEmpty(customer.postalAddress.CitySubdivisionName)
                && string.IsNullOrEmpty(customer.postalAddress.CityName)
                && string.IsNullOrEmpty(customer.postalAddress.PostalZone)
                && string.IsNullOrEmpty(customer.partyTaxScheme.CompanyID)
                && string.IsNullOrEmpty(customer.partyLegalEntity.RegistrationName)
                )
            {
                sb.Append("<cac:AccountingCustomerParty>" + "</cac:AccountingCustomerParty>");
                return;
            }
            sb.Append("<cac:AccountingCustomerParty>" +
                "<cac:Party>");
            if (!string.IsNullOrEmpty(customer.partyIdentification.ID))
            {
                if (!string.IsNullOrEmpty(customer.partyIdentification.schemeID))
                {
                    customer.partyIdentification.schemeID = customer.partyIdentification.schemeID.Trim().ToUpper();
                }
                else
                {
                    if (exportinvoice)
                    {
                        customer.partyIdentification.schemeID = "OTH";
                    }
                    else
                    {
                        customer.partyIdentification.schemeID = "CRN";
                    }

                }
                sb.Append("<cac:PartyIdentification>" +
                "<cbc:ID schemeID='" + customer.partyIdentification.schemeID + "'>" + customer.partyIdentification.ID + "</cbc:ID>" +
                "</cac:PartyIdentification>");
            }
            sb.Append("<cac:PostalAddress>");
            if (!string.IsNullOrEmpty(customer.postalAddress.StreetName))
            {
                sb.Append("<cbc:StreetName>" + Utility.ReplaceXMLSpecialCharacters(customer.postalAddress.StreetName.Trim()) + "</cbc:StreetName>");
            }
            if (!string.IsNullOrEmpty(customer.postalAddress.AdditionalStreetName))
            {
                sb.Append("<cbc:AdditionalStreetName>" + Utility.ReplaceXMLSpecialCharacters(customer.postalAddress.AdditionalStreetName.Trim()) + "</cbc:AdditionalStreetName>");
            }
            if (!string.IsNullOrEmpty(customer.postalAddress.BuildingNumber))
            {
                sb.Append("<cbc:BuildingNumber>" + customer.postalAddress.BuildingNumber.Trim() + "</cbc:BuildingNumber>");
            }
            if (!string.IsNullOrEmpty(customer.postalAddress.PlotIdentification))
            {
                sb.Append("<cbc:PlotIdentification>" + customer.postalAddress.PlotIdentification.Trim() + "</cbc:PlotIdentification>");
            }
            if (!string.IsNullOrEmpty(customer.postalAddress.CitySubdivisionName))
            {
                sb.Append("<cbc:CitySubdivisionName>" + Utility.ReplaceXMLSpecialCharacters(customer.postalAddress.CitySubdivisionName.Trim()) + "</cbc:CitySubdivisionName>");
            }
            if (!string.IsNullOrEmpty(customer.postalAddress.CityName))
            {
                sb.Append("<cbc:CityName>" + Utility.ReplaceXMLSpecialCharacters(customer.postalAddress.CityName.Trim()) + "</cbc:CityName>");
            }
            if (!string.IsNullOrEmpty(customer.postalAddress.PostalZone))
            {
                sb.Append("<cbc:PostalZone>" + customer.postalAddress.PostalZone.Trim() + "</cbc:PostalZone>");
            }
            if (!string.IsNullOrEmpty(customer.postalAddress.CountrySubentity))
            {
                sb.Append("<cbc:CountrySubentity>" + Utility.ReplaceXMLSpecialCharacters(customer.postalAddress.CountrySubentity.Trim()) + "</cbc:CountrySubentity>");
            }
            if (string.IsNullOrEmpty(customer.postalAddress.country.IdentificationCode))
            {
                customer.postalAddress.country.IdentificationCode = "SA";
            }
            sb.Append("<cac:Country>" +
                "<cbc:IdentificationCode>" + customer.postalAddress.country.IdentificationCode.Trim().ToUpper() + "</cbc:IdentificationCode>" +
                "</cac:Country>");
            sb.Append("</cac:PostalAddress>");
            if (!string.IsNullOrEmpty(customer.partyTaxScheme.CompanyID) && !exportinvoice)
            {
                sb.Append("<cac:PartyTaxScheme>" +
                    "<cbc:CompanyID>" + customer.partyTaxScheme.CompanyID.Trim() + "</cbc:CompanyID>" +
                    "<cac:TaxScheme>" +
                "<cbc:ID>VAT</cbc:ID>" +
                "</cac:TaxScheme>" +
                "</cac:PartyTaxScheme>");
            }
            if (!string.IsNullOrEmpty(customer.partyLegalEntity.RegistrationName))
            {
                sb.Append("<cac:PartyLegalEntity>" +
                "<cbc:RegistrationName>" + Utility.ReplaceXMLSpecialCharacters(customer.partyLegalEntity.RegistrationName.Trim()) + "</cbc:RegistrationName>" +
                "</cac:PartyLegalEntity>");
            }
            if (!string.IsNullOrEmpty(customer.contact.Name)
                || !string.IsNullOrEmpty(customer.contact.Telephone)
                || !string.IsNullOrEmpty(customer.contact.ElectronicMail)
                || !string.IsNullOrEmpty(customer.contact.Note)
                )
            {
                sb.Append("<cac:Contact>");
                if (!string.IsNullOrEmpty(customer.contact.Name))
                {

                    sb.Append("<cbc:Name>" + Utility.ReplaceXMLSpecialCharacters(customer.contact.Name.Trim()) + "</cbc:Name>");

                }
                if (!string.IsNullOrEmpty(customer.contact.Telephone))
                {

                    sb.Append("<cbc:Telephone>" + Utility.ReplaceXMLSpecialCharacters(customer.contact.Telephone.Trim()) + "</cbc:Telephone>");

                }
                if (!string.IsNullOrEmpty(customer.contact.ElectronicMail))
                {

                    sb.Append("<cbc:ElectronicMail>" + Utility.ReplaceXMLSpecialCharacters(customer.contact.ElectronicMail.Trim()) + "</cbc:ElectronicMail>");

                }
                if (!string.IsNullOrEmpty(customer.contact.Note))
                {

                    sb.Append("<cbc:Note>" + Utility.ReplaceXMLSpecialCharacters(customer.contact.Note.Trim()) + "</cbc:Note>");

                }
                sb.Append("</cac:Contact>");
            }
            sb.Append("</cac:Party>" +
                "</cac:AccountingCustomerParty>");

        }

        private void GetDeliveryElement(Invoice inv, StringBuilder sb, ref string error)
        {
            Delivery delivery = inv.delivery;

            bool issimplified = inv.invoiceTypeCode.Name.Substring(0, 2) == "02";
            bool issummary = inv.invoiceTypeCode.Name.Substring(5, 1) == "1";
            //error = "";
            if (issimplified && issummary && string.IsNullOrEmpty(delivery.ActualDeliveryDate))
            {
                error += "\n";
                error += "You must Enter start and end Delivery Date";
            }

            if (string.IsNullOrEmpty(delivery.ActualDeliveryDate) && string.IsNullOrEmpty(delivery.LatestDeliveryDate))
            {

            }
            else
            {
                if (!issimplified || (issimplified && issummary))
                {
                    sb.Append("<cac:Delivery>");
                    if (!string.IsNullOrEmpty(delivery.ActualDeliveryDate))
                        sb.Append("<cbc:ActualDeliveryDate>" + delivery.ActualDeliveryDate + "</cbc:ActualDeliveryDate>");
                    if (!string.IsNullOrEmpty(delivery.LatestDeliveryDate))
                        sb.Append("<cbc:LatestDeliveryDate>" + delivery.LatestDeliveryDate + "</cbc:LatestDeliveryDate>");
                    sb.Append("</cac:Delivery>");

                }

            }

        }
        private void GetallowanceChargeElement(Invoice inv, StringBuilder sb)
        {
            foreach (AllowanceCharge allowance in inv.allowanceCharges)
            {

                decimal allowanceamount = 0;
                bool addbaseamounttag = false;
                if (allowance.Amount > 0)
                {
                    allowanceamount = allowance.Amount;

                }
                else if (allowance.MultiplierFactorNumeric > 0 && allowance.BaseAmount > 0)
                {
                    allowanceamount = (allowance.MultiplierFactorNumeric / 100) * allowance.BaseAmount;
                    allowance.Amount = allowanceamount;
                    addbaseamounttag = true;
                }
                if (allowanceamount > 0)
                {
                    StringBuilder taxcat = new StringBuilder();
                    sb.Append("<cac:AllowanceCharge>");
                    sb.Append("<cbc:ChargeIndicator>" + allowance.ChargeIndicator.ToString().ToLower() + "</cbc:ChargeIndicator>");
                    if (!string.IsNullOrEmpty(allowance.AllowanceChargeReasonCode))
                    {
                        sb.Append("<cbc:AllowanceChargeReasonCode>" + allowance.AllowanceChargeReasonCode.Trim() + "</cbc:AllowanceChargeReasonCode>");
                    }
                    if (!string.IsNullOrEmpty(allowance.AllowanceChargeReason))
                    {
                        sb.Append("<cbc:AllowanceChargeReason>" + Utility.ReplaceXMLSpecialCharacters(allowance.AllowanceChargeReason.Trim()) + "</cbc:AllowanceChargeReason>");

                    }
                    if (!string.IsNullOrEmpty(allowance.taxCategory.ID))
                        allowance.taxCategory.ID = allowance.taxCategory.ID.Trim().ToUpper();
                    GetTaxCategoryElement(allowance.taxCategory.Percent, allowance.taxCategory.ID, taxcat);
                    if (addbaseamounttag)
                    {
                        sb.Append("<cbc:MultiplierFactorNumeric>" + allowance.MultiplierFactorNumeric.ToString("0.00") + "</cbc:MultiplierFactorNumeric>");

                    }
                    sb.Append("<cbc:Amount currencyID='" + inv.DocumentCurrencyCode + "'>" + allowanceamount.ToString("0.00") + "</cbc:Amount>");
                    if (addbaseamounttag)
                    {
                        sb.Append("<cbc:BaseAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + allowance.BaseAmount.ToString("0.00") + "</cbc:BaseAmount>");
                    }
                    sb.Append(taxcat);
                    sb.Append("</cac:AllowanceCharge>");
                }

            }

        }
        private void GetIvoiceLineallowanceChargeElement(InvoiceLine invline, string DocumentCurrencyCode, StringBuilder sb)
        {
            foreach (var allowancecharge in invline.allowanceCharges)
            {
                decimal allowanceamount = 0;
                bool addbaseamounttag = false;
                if (allowancecharge.Amount > 0)
                {
                    allowanceamount = allowancecharge.Amount;

                }
                else if (allowancecharge.MultiplierFactorNumeric > 0 && allowancecharge.BaseAmount > 0)
                {
                    allowanceamount = (allowancecharge.MultiplierFactorNumeric / 100) * allowancecharge.BaseAmount;
                    allowancecharge.Amount = allowanceamount;
                    addbaseamounttag = true;
                }
                if (allowanceamount > 0)
                {
                    sb.Append("<cac:AllowanceCharge>");
                    sb.Append("<cbc:ChargeIndicator>" + allowancecharge.ChargeIndicator.ToString().ToLower() + "</cbc:ChargeIndicator>");
                    if (!string.IsNullOrEmpty(allowancecharge.AllowanceChargeReasonCode))
                    {
                        sb.Append("<cbc:AllowanceChargeReasonCode>" + allowancecharge.AllowanceChargeReasonCode.Trim() + "</cbc:AllowanceChargeReasonCode>");

                    }
                    if (!string.IsNullOrEmpty(allowancecharge.AllowanceChargeReason))
                    {
                        sb.Append("<cbc:AllowanceChargeReason>" + Utility.ReplaceXMLSpecialCharacters(allowancecharge.AllowanceChargeReason.Trim()) + "</cbc:AllowanceChargeReason>");
                    }
                    if (addbaseamounttag)
                    {
                        sb.Append("<cbc:MultiplierFactorNumeric>" + allowancecharge.MultiplierFactorNumeric.ToString("0.00") + "</cbc:MultiplierFactorNumeric>");

                    }
                    sb.Append("<cbc:Amount currencyID='" + DocumentCurrencyCode + "'>" + allowanceamount.ToString("0.00") + "</cbc:Amount>");
                    if (addbaseamounttag)
                    {
                        sb.Append("<cbc:BaseAmount currencyID='" + DocumentCurrencyCode + "'>" + allowancecharge.BaseAmount.ToString("0.00") + "</cbc:BaseAmount>");
                    }
                    sb.Append("</cac:AllowanceCharge>");
                }
            }
        }
        private void GetTaxCategoryElement(decimal percent, string code, StringBuilder taxcat)
        {

            taxcat.Append("<cac:TaxCategory>" +
                "<cbc:ID>" + code + "</cbc:ID>" +
                "<cbc:Percent>" + percent.ToString("0.00") + "</cbc:Percent>" +
                "<cac:TaxScheme>" +
                "<cbc:ID>VAT</cbc:ID>" +
                "</cac:TaxScheme>" +
                "</cac:TaxCategory>");
        }
        private void GetLegalMonetaryTotal(Invoice inv, StringBuilder sb, ref string error)
        {
            try
            {
                sb.Append("<cac:LegalMonetaryTotal>");
                sb.Append("<cbc:LineExtensionAmount currencyID='SAR'>" + Math.Round(inv.legalMonetaryTotal.LineExtensionAmount, 2).ToString("0.00") + "</cbc:LineExtensionAmount>");
                sb.Append("<cbc:TaxExclusiveAmount currencyID='SAR'>" + Math.Round(inv.legalMonetaryTotal.TaxExclusiveAmount, 2).ToString("0.00") + "</cbc:TaxExclusiveAmount>");
                sb.Append("<cbc:TaxInclusiveAmount currencyID='SAR'>" + Math.Round(inv.legalMonetaryTotal.TaxInclusiveAmount, 2).ToString("0.00") + "</cbc:TaxInclusiveAmount>");

                if (inv.legalMonetaryTotal.AllowanceTotalAmount > 0)
                {
                    sb.Append("<cbc:AllowanceTotalAmount currencyID='SAR'>" + Math.Round(inv.legalMonetaryTotal.AllowanceTotalAmount, 2).ToString("0.00") + "</cbc:AllowanceTotalAmount>");
                }
                if (inv.legalMonetaryTotal.ChargeTotalAmount > 0)
                {
                    sb.Append("<cbc:ChargeTotalAmount currencyID='SAR'>" + Math.Round(inv.legalMonetaryTotal.ChargeTotalAmount, 2).ToString("0.00") + "</cbc:ChargeTotalAmount>");
                }
                if (inv.legalMonetaryTotal.PrepaidAmount > 0)
                {
                    sb.Append("<cbc:PrepaidAmount currencyID='SAR'>" + Math.Round(inv.legalMonetaryTotal.PrepaidAmount, 2).ToString("0.00") + "</cbc:PrepaidAmount>");
                }
                if (inv.legalMonetaryTotal.PayableRoundingAmount != 0)
                {
                    sb.Append("<cbc:PayableRoundingAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + Math.Round((decimal)inv.legalMonetaryTotal.PayableRoundingAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:PayableRoundingAmount>");

                }
                if (inv.legalMonetaryTotal.PayableAmount > 0)
                {
                    sb.Append("<cbc:PayableAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + Math.Round(inv.legalMonetaryTotal.PayableAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:PayableAmount>");
                }
                else
                {
                    sb.Append("<cbc:PayableAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + Math.Round(Math.Round(inv.legalMonetaryTotal.TaxInclusiveAmount, 2) - inv.legalMonetaryTotal.PrepaidAmount + inv.legalMonetaryTotal.PayableRoundingAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:PayableAmount>");
                }

                sb.Append("</cac:LegalMonetaryTotal>");

            }
            catch
            {
                error += "\n";
                error += "Error in LegalMonetaryTotal.";
            }

        }
        private void GetDocumentTaxTotal(Invoice inv, StringBuilder sb, ref string error)
        {
            StringBuilder subtotaltext = new StringBuilder();
            foreach (TaxSubtotal taxsubtotal in inv.TaxTotal.TaxSubtotalList)
            {
                subtotaltext.Append("<cac:TaxSubtotal>" +
                    "<cbc:TaxableAmount currencyID='SAR'>" + taxsubtotal.TaxableAmount.ToString("0.00") + "</cbc:TaxableAmount>" +
                    "<cbc:TaxAmount currencyID='SAR'>" + taxsubtotal.TaxAmount.ToString("0.00") + "</cbc:TaxAmount>");
                subtotaltext.Append("<cac:TaxCategory>" +
                "<cbc:ID>" + taxsubtotal.taxCategory.ID + "</cbc:ID>" +
                "<cbc:Percent>" + taxsubtotal.taxCategory.Percent.ToString("0.00") + "</cbc:Percent>");
                if (taxsubtotal.taxCategory.ID == "O" || taxsubtotal.taxCategory.ID == "E" || taxsubtotal.taxCategory.ID == "Z")
                {
                    if (string.IsNullOrEmpty(taxsubtotal.taxCategory.TaxExemptionReasonCode))
                    {
                        error += "\n";
                        error += "items with VAT Code ( Z - E - O ) must include TaxExemptionReasonCode";
                    }
                    else
                    {
                        subtotaltext.Append("<cbc:TaxExemptionReasonCode>" + Utility.ReplaceXMLSpecialCharacters(taxsubtotal.taxCategory.TaxExemptionReasonCode.Trim()).ToUpper() + "</cbc:TaxExemptionReasonCode>");

                    }
                    if (!string.IsNullOrEmpty(taxsubtotal.taxCategory.TaxExemptionReason))
                    {
                        subtotaltext.Append("<cbc:TaxExemptionReason>" + Utility.ReplaceXMLSpecialCharacters(taxsubtotal.taxCategory.TaxExemptionReason.Trim()) + "</cbc:TaxExemptionReason>");

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(taxsubtotal.taxCategory.TaxExemptionReasonCode))
                        {
                            // get Tax Exemption Reason
                            string taxexemptionreason = GetTaxExemptionReason(Utility.ReplaceXMLSpecialCharacters(taxsubtotal.taxCategory.TaxExemptionReasonCode.Trim()).ToUpper());
                            if (!string.IsNullOrEmpty(taxexemptionreason))
                            {
                                subtotaltext.Append("<cbc:TaxExemptionReason>" + taxexemptionreason + "</cbc:TaxExemptionReason>");
                            }
                        }
                        else
                        {
                            error += "\n";
                            error += "Tax must include TaxExemptionReason";
                        }

                    }
                }
                subtotaltext.Append("<cac:TaxScheme>" +
                        "<cbc:ID>VAT</cbc:ID>" +
                        "</cac:TaxScheme>" +
                        "</cac:TaxCategory>");
                subtotaltext.Append("</cac:TaxSubtotal>");


            }
            sb.Append("<cac:TaxTotal>" +
            "<cbc:TaxAmount currencyID='" + inv.TaxCurrencyCode + "'>" + Math.Round(inv.TaxTotal.TaxAmount, 2).ToString("0.00") + "</cbc:TaxAmount>" +
            "</cac:TaxTotal>");
            sb.Append("<cac:TaxTotal>");
            sb.Append("<cbc:TaxAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + Math.Round(inv.TaxTotal.TaxAmount, 2).ToString("0.00") + "</cbc:TaxAmount>");
            sb.Append(subtotaltext);

            sb.Append("</cac:TaxTotal>");


        }
        private void GetInvoiceLineElement(Invoice inv, StringBuilder sb)
        {

            int count = 0;
            foreach (InvoiceLine invline in inv.InvoiceLines)
            {
                count = count + 1;
                bool AdditionalInvoiceLine = false;
                if (inv.legalMonetaryTotal.PrepaidAmount > 0 && invline.documentReferences.Count > 0)
                {
                    AdditionalInvoiceLine = true;
                    invline.LineExtensionAmount = 0;
                    invline.taxTotal.TaxAmount = 0;
                    invline.taxTotal.RoundingAmount = 0;
                    invline.price.PriceAmount = 0;
                }

                decimal ItemPriceAmount = invline.price.PriceAmount;
                decimal lineextensionamount = 0;
                if (invline.LineExtensionAmount > 0)
                {
                    lineextensionamount = invline.LineExtensionAmount;
                }
                else
                {
                    decimal invoicelineallowanceCharges = 0;
                    if (invline.allowanceCharges != null && invline.allowanceCharges.Count > 0)
                    {
                        invoicelineallowanceCharges = invline.allowanceCharges.Sum(x => x.Amount);
                    }

                    lineextensionamount = (invline.InvoiceQuantity * ItemPriceAmount) - invoicelineallowanceCharges;
                }
                sb.Append("<cac:InvoiceLine>");
                if (!string.IsNullOrEmpty(invline.ID))
                {
                    sb.Append("<cbc:ID>" + Utility.ReplaceXMLSpecialCharacters(invline.ID.Trim()) + "</cbc:ID>");
                }
                else
                {
                    invline.ID = count.ToString();
                    sb.Append("<cbc:ID>" + invline.ID + "</cbc:ID>");

                }
                sb.Append("<cbc:InvoicedQuantity>" + invline.InvoiceQuantity + "</cbc:InvoicedQuantity>");
                sb.Append("<cbc:LineExtensionAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + lineextensionamount.ToString("0.00") + "</cbc:LineExtensionAmount>");
                if (AdditionalInvoiceLine)
                {
                    //prepare document referance tag
                    sb.Append(GetDocumentReferences(invline.documentReferences));
                }
                if (AdditionalInvoiceLine)
                {
                    sb.Append(GetInvoiceLineTaxTotalPrePaid(invline, inv.DocumentCurrencyCode));
                }
                else
                {
                    GetIvoiceLineallowanceChargeElement(invline, inv.DocumentCurrencyCode, sb);
                    sb.Append(GetInvoiceLineTaxTotal(invline));
                }
                sb.Append(GetInvoiceLineItemElement(invline));
                sb.Append(GetInvoiceLinePriceElement(invline, inv.DocumentCurrencyCode));
                sb.Append("</cac:InvoiceLine>");
            }

        }
        private StringBuilder GetDocumentReferences(DocumentReferenceCollection documentReferences)
        {
            StringBuilder referencestag = new StringBuilder();

            foreach (DocumentReference doc in documentReferences)
            {
                referencestag.Append("<cac:DocumentReference>");
                referencestag.Append("<cbc:ID>" + doc.ID + "</cbc:ID>");
                if (!string.IsNullOrEmpty(doc.UUID))
                {
                    referencestag.Append("<cbc:UUID>" + doc.UUID + "</cbc:UUID>");
                }
                referencestag.Append("<cbc:IssueDate>" + doc.IssueDate + "</cbc:IssueDate>");
                referencestag.Append("<cbc:IssueTime>" + doc.IssueTime + "</cbc:IssueTime>");
                if (doc.DocumentTypeCode == 0)
                {
                    doc.DocumentTypeCode = 386;
                }
                referencestag.Append("<cbc:DocumentTypeCode>" + doc.DocumentTypeCode + "</cbc:DocumentTypeCode>");
                referencestag.Append("</cac:DocumentReference>");
            }

            return referencestag;
        }
        private StringBuilder GetInvoiceLineItemElement(InvoiceLine invline)
        {
            StringBuilder item = new StringBuilder();
            item.Append("<cac:Item>");
            if (!string.IsNullOrEmpty(invline.item.Name))
            {
                invline.item.Name = Utility.RemoveControlCharacters(invline.item.Name);
                item.Append("<cbc:Name>" + Utility.ReplaceXMLSpecialCharacters(invline.item.Name.Trim()) + "</cbc:Name>");
            }
            if (!string.IsNullOrEmpty(invline.item.BuyersItemIdentificationID))
            {
                item.Append("<cac:BuyersItemIdentification>" +
                 "<cbc:ID>" + Utility.ReplaceXMLSpecialCharacters(invline.item.BuyersItemIdentificationID.Trim()) + "</cbc:ID>" +
                 "</cac:BuyersItemIdentification>");
            }
            if (!string.IsNullOrEmpty(invline.item.SellersItemIdentificationID))
            {
                item.Append("<cac:SellersItemIdentification>" +
                 "<cbc:ID>" + Utility.ReplaceXMLSpecialCharacters(invline.item.SellersItemIdentificationID.Trim()) + "</cbc:ID>" +
                 "</cac:SellersItemIdentification>");
            }
            if (!string.IsNullOrEmpty(invline.item.StandardItemIdentificationID))
            {
                item.Append("<cac:StandardItemIdentification>" +
                 "<cbc:ID>" + Utility.ReplaceXMLSpecialCharacters(invline.item.StandardItemIdentificationID.Trim()) + "</cbc:ID>" +
                 "</cac:StandardItemIdentification>");
            }
            item.Append("<cac:ClassifiedTaxCategory>" +
                "<cbc:ID>" + invline.item.classifiedTaxCategory.ID.Trim().ToUpper() + "</cbc:ID>" +
                "<cbc:Percent>" + invline.item.classifiedTaxCategory.Percent.ToString("0.00") + "</cbc:Percent>" +
                "<cac:TaxScheme>" +
                "<cbc:ID>VAT</cbc:ID>" +
                "</cac:TaxScheme>" +
                "</cac:ClassifiedTaxCategory>");
            item.Append("</cac:Item>");
            return item;
        }
        private StringBuilder GetInvoiceLinePriceElement(InvoiceLine invline, string DocumentCurrencyCode)
        {
            StringBuilder pricetxt = new StringBuilder();
            pricetxt.Append("<cac:Price>");
            pricetxt.Append("<cbc:PriceAmount currencyID='" + DocumentCurrencyCode + "'>" + invline.price.PriceAmount + "</cbc:PriceAmount>");
            if (invline.price.BaseQuantity > 0)
            {
                pricetxt.Append("<cbc:BaseQuantity>" + invline.price.BaseQuantity.ToString("0.00") + "</cbc:BaseQuantity>");
            }
            if (invline.price.allowanceCharge.Amount > 0)
            {
                pricetxt.Append("<cac:AllowanceCharge>" +
                        "<cbc:ChargeIndicator>false</cbc:ChargeIndicator>");
                if (!string.IsNullOrEmpty(invline.price.allowanceCharge.AllowanceChargeReason))
                {
                    pricetxt.Append("<cbc:AllowanceChargeReason>" + Utility.ReplaceXMLSpecialCharacters(invline.price.allowanceCharge.AllowanceChargeReason.Trim()) + "</cbc:AllowanceChargeReason>");
                }
                pricetxt.Append("<cbc:Amount currencyID='" + DocumentCurrencyCode + "'>" + invline.price.allowanceCharge.Amount.ToString("0.00") + "</cbc:Amount>" +
                        "</cac:AllowanceCharge>");
            }

            pricetxt.Append("</cac:Price>");
            return pricetxt;
        }
        private void GetPaymentMeansElement(Invoice inv, StringBuilder sb, ref string error)
        {
            // bool issimplified = inv.invoiceTypeCode.Name.Substring(0, 2) == "02";
            foreach (var paymentmean in inv.paymentmeans)
            {
                if (inv.invoiceTypeCode.id == 388 && string.IsNullOrEmpty(paymentmean.PaymentMeansCode) && string.IsNullOrEmpty(paymentmean.InstructionNote))
                {
                    continue;
                }
                if ((inv.invoiceTypeCode.id == 383 || inv.invoiceTypeCode.id == 381) && string.IsNullOrEmpty(paymentmean.InstructionNote))
                {
                    error += "\n";
                    error += "Credit or Debit Note must has PaymentMeans InstructionNote.";
                }
                else
                {
                    sb.Append("<cac:PaymentMeans>");
                    if (!string.IsNullOrEmpty(paymentmean.PaymentMeansCode))
                    {
                        sb.Append("<cbc:PaymentMeansCode>" + paymentmean.PaymentMeansCode + "</cbc:PaymentMeansCode>");
                    }
                    if (!string.IsNullOrEmpty(paymentmean.InstructionNote))
                    {
                        sb.Append("<cbc:InstructionNote>" + Utility.ReplaceXMLSpecialCharacters(paymentmean.InstructionNote.Trim()) + "</cbc:InstructionNote>");
                    }
                    if (!string.IsNullOrEmpty(paymentmean.payeefinancialaccount.ID) || !string.IsNullOrEmpty(paymentmean.payeefinancialaccount.paymentnote))
                    {
                        sb.Append("<cac:PayeeFinancialAccount>");
                        if (!string.IsNullOrEmpty(paymentmean.payeefinancialaccount.ID))
                            sb.Append("<cbc:ID>" + paymentmean.payeefinancialaccount.ID.Trim() + "</cbc:ID>");
                        if (!string.IsNullOrEmpty(paymentmean.payeefinancialaccount.paymentnote))
                            sb.Append("<cbc:PaymentNote>" + paymentmean.payeefinancialaccount.paymentnote.Trim() + "</cbc:PaymentNote>");
                        sb.Append("</cac:PayeeFinancialAccount>");
                    }
                    sb.Append("</cac:PaymentMeans>");

                }
            }




        }
        private StringBuilder GetInvoiceLineTaxTotal(InvoiceLine invline)
        {
            StringBuilder taxtotal = new StringBuilder();

            taxtotal.Append("<cac:TaxTotal>" +
                "<cbc:TaxAmount currencyID='SAR'>" + Math.Round(invline.taxTotal.TaxAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:TaxAmount>" +
                "<cbc:RoundingAmount currencyID='SAR'>" + Math.Round(invline.taxTotal.RoundingAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:RoundingAmount>" +
                "</cac:TaxTotal>");

            return taxtotal;
        }

        private StringBuilder GetInvoiceLineTaxTotalPrePaid(InvoiceLine invline, string DocumentCurrencyCode)
        {
            StringBuilder taxtotal = new StringBuilder();
            decimal taxamount = Math.Round(invline.taxTotal.TaxAmount, 2, MidpointRounding.AwayFromZero);
            decimal roundingamount = invline.LineExtensionAmount + taxamount;
            taxtotal.Append("<cac:TaxTotal>");
            taxtotal.Append("<cbc:TaxAmount currencyID='" + DocumentCurrencyCode + "'>" + taxamount.ToString("0.00") + "</cbc:TaxAmount>");
            taxtotal.Append("<cbc:RoundingAmount currencyID='" + DocumentCurrencyCode + "'>" + roundingamount.ToString("0.00") + "</cbc:RoundingAmount>");
            taxtotal.Append("<cac:TaxSubtotal>");
            taxtotal.Append("<cbc:TaxableAmount currencyID='" + DocumentCurrencyCode + "'>" + invline.taxTotal.TaxSubtotal.TaxableAmount.ToString("0.00") + "</cbc:TaxableAmount>");
            taxtotal.Append("<cbc:TaxAmount currencyID='" + DocumentCurrencyCode + "'>" + invline.taxTotal.TaxSubtotal.TaxAmount.ToString("0.00") + "</cbc:TaxAmount>");
            taxtotal.Append("<cac:TaxCategory>");
            taxtotal.Append("<cbc:ID>" + invline.taxTotal.TaxSubtotal.taxCategory.ID.Trim().ToUpper() + "</cbc:ID>");
            taxtotal.Append("<cbc:Percent>" + invline.taxTotal.TaxSubtotal.taxCategory.Percent.ToString("0.00") + "</cbc:Percent>");
            if (invline.taxTotal.TaxSubtotal.taxCategory.ID == "O" || invline.taxTotal.TaxSubtotal.taxCategory.ID == "E" || invline.taxTotal.TaxSubtotal.taxCategory.ID == "Z")
            {
                if (!string.IsNullOrEmpty(invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReasonCode))
                {
                    taxtotal.Append("<cbc:TaxExemptionReasonCode>" + invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReasonCode.Trim().ToUpper() + "</cbc:TaxExemptionReasonCode>");

                }
                if (!string.IsNullOrEmpty(invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReason))
                {
                    taxtotal.Append("<cbc:TaxExemptionReason>" + Utility.ReplaceXMLSpecialCharacters(invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReason.Trim()) + "</cbc:TaxExemptionReason>");
                }
            }
            taxtotal.Append("<cac:TaxScheme>");
            taxtotal.Append("<cbc:ID>VAT</cbc:ID>");
            taxtotal.Append("</cac:TaxScheme>");
            taxtotal.Append("</cac:TaxCategory>");
            taxtotal.Append("</cac:TaxSubtotal>");
            taxtotal.Append("</cac:TaxTotal>");

            return taxtotal;
        }
        private bool CheckIssueDateFormat(string issuedate, string issuetime)
        {
            if (!string.IsNullOrEmpty(issuedate) && !string.IsNullOrEmpty(issuetime))
            {
                string input = issuedate.Trim() + " " + issuetime.Trim();
                string format = "yyyy-MM-dd HH:mm:ss";
                if (DateTime.TryParseExact(input, format, null, System.Globalization.DateTimeStyles.None, out DateTime exactResult))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        private string GetTaxExemptionReason(string TaxExemptionReasonCode)
        {

            switch (TaxExemptionReasonCode)
            {
                case "VATEX-SA-29":
                    return "Financial services mentioned in Article 29 of the VAT Regulations | الخدمات المالية";
                case "VATEX-SA-29-7":
                    return "Life insurance services mentioned in Article 29 of the VAT Regulations | عقد تأمين على الحياة";
                case "VATEX-SA-30":
                    return "Real estate transactions mentioned in Article 30 of the VAT Regulations | التوريدات العقارية المعفاة من الضريبة";
                case "VATEX-SA-32":
                    return "Export of goods | صادرات السلع من المملكة";
                case "VATEX-SA-33":
                    return "Export of services | صادرات الخدمات من المملكة";
                case "VATEX-SA-34-1":
                    return "The international transport of Goods | النقل الدولي للسلع";
                case "VATEX-SA-34-2":
                    return "international transport of passengers | النقل الدولي للركاب";
                case "VATEX-SA-34-3":
                    return "services directly connected and incidental to a Supply of international passenger transport | الخدمات المرتبطة مباشرة أو عرضياً بتوريد النقل الدولي للركاب";
                case "VATEX-SA-34-4":
                    return "Supply of a qualifying means of transport | توريد وسائل النقل المؤهلة";
                case "VATEX-SA-34-5":
                    return "Any services relating to Goods or passenger transportation, as defined in article twenty five of these Regulations | الخدمات ذات الصلة بنقل السلع أو الركاب، وفقاً للتعريف الوارد بالمادة الخامسة والعشرين من اللائحة التنفيذية لنظام ضريبة القيمة المضافة";
                case "VATEX-SA-35":
                    return "Medicines and medical equipment | الأدوية والمعدات الطبية";
                case "VATEX-SA-36":
                    return "Qualifying metals | المعادن المؤهلة";
                case "VATEX-SA-EDU":
                    return "Private education to citizen | الخدمات التعليمية الخاصة للمواطنين";
                case "VATEX-SA-HEA":
                    return "Private healthcare to citizen | الخدمات الصحية الخاصة للمواطنين";
                case "VATEX-SA-MLTRY":
                    return "supply of qualified military goods | توريد السلع العسكرية المؤهلة";
                case "VATEX-SA-OOS":
                    return "Not subject to VAT";
                case "VATEX-SA-DUTYFREE":
                    return "Qualified Supply of Goods in Duty Free area | التوريد المؤهل للسلع في الأسواق الحرة";
                case "VATEX-SA-DIPLOMAT":
                    return "Qualified Supplies to Diplomatic Missions | التوريدات المؤهلة للبعثات الدبلوماسية";
                default:
                    return "";
            }

        }
        #endregion



    }

}

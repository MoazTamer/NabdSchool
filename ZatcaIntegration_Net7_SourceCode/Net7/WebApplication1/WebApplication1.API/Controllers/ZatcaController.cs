using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ZatcaIntegrationSDK;
using ZatcaIntegrationSDK.BLL;
using ZatcaIntegrationSDK.HelperContracts;
using ZatcaIntegrationSDK.APIHelper;
using System.Security.Cryptography.Pkcs;
namespace WebApplication1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZatcaController : ControllerBase
    {
        [HttpGet(Name = "CreateSimplifiedInvoice")]
        public async Task<ActionResult<InvoiceReportingResponse>> CreateSimplifiedInvoice()
        {
           
            string localZonetext = " StandardName: " + TimeZoneInfo.Local.DisplayName + "   DaylightName: " + TimeZoneInfo.Local.DaylightName;
            UBLXML ubl = new UBLXML();
            Invoice inv = new Invoice();
            Result res = new Result();
            InvoiceReportingResponse invoicereportingmodel = new InvoiceReportingResponse();
            inv.ID = "1230"; // ãËÇá SME00010
            inv.UUID = Guid.NewGuid().ToString(); //"540b4b4c-5af5-4e07-8d3e-78170793c480"; // Guid.NewGuid().ToString();
            inv.IssueDate = "2023-05-14";
            inv.IssueTime = "11:25:55";
            //388 ÝÇÊæÑÉ  
            //383 ÇÔÚÇÑ ãÏíä
            //381 ÇÔÚÇÑ ÏÇÆä
            inv.invoiceTypeCode.id = 388;
            //inv.invoiceTypeCode.Name based on format NNPNESB
            //NN 01 ÝÇÊæÑÉ ÚÇÏíÉ
            //NN 02 ÝÇÊæÑÉ ãÈÓØÉ
            //
            //
            //
            //
            //
            //P Ýì ÍÇáÉ ÝÇÊæÑÉ áØÑÝ ËÇáË äßÊÈ 1 æÝì ÇáÍÇáÉ ÇáÇÎÑì äßÊÈ 0
            //N Ýì ÍÇáÉ ÝÇÊæÑÉ ÇÓãíÉ äßÊÈ 1 æÝì ÇáÍÇáÉ ÇáÇÎÑì äßÊÈ 0
            //E Ýì ÍÇáÉ ÝÇÊæÑÉ ááÕÇÏÑÇÊ äßÊÈ 1 æÝì ÇáÍÇáÉ ÇáÇÎÑì äßÊÈ 0
            //S Ýì ÍÇáÉ ÝÇÊæÑÉ ãáÎÕÉ äßÊÈ 1 æÝì ÇáÍÇáÉ ÇáÇÎÑì äßÊÈ 0
            //B Ýì ÍÇáÉ ÝÇÊæÑÉ ÐÇÊíÉ äßÊÈ 1
            //B Ýì ÍÇáÉ Çä ÇáÝÇÊæÑÉ ÕÇÏÑÇÊ =1  áÇíãßä Çä Êßæä ÇáÝÇÊæÑÉ ÐÇÊíÉ 1
            //
            inv.invoiceTypeCode.Name = "0200000";
            inv.DocumentCurrencyCode = "SAR";//ÇáÚãáÉ
            inv.TaxCurrencyCode = "SAR"; ////Ýì ÍÇáÉ ÇáÏæáÇÑ áÇÈÏ Çä Êßæä ÚãáÉ ÇáÖÑíÈÉ ÈÇáÑíÇá ÇáÓÚæÏì
                                         //inv.CurrencyRate = decimal.Parse("3.75"); // ÞíãÉ ÇáÏæáÇÑ ãÞÇÈá ÇáÑíÇá
                                         // Ýì ÍÇáÉ Çä ÇÔÚÇÑ ÏÇÆä Çæ ãÏíä ÝÞØ åÇäßÊÈ ÑÞã ÇáÝÇÊæÑÉ Çááì ÇÕÏÑäÇ ÇáÇÔÚÇÑ áíåÇ
            if (inv.invoiceTypeCode.id == 383 || inv.invoiceTypeCode.id == 381)
            {
                // فى حالة ان اشعار دائن او مدين فقط هانكتب رقم الفاتورة اللى اصدرنا الاشعار ليها
                InvoiceDocumentReference invoiceDocumentReference = new InvoiceDocumentReference();
                invoiceDocumentReference.ID = "Invoice Number: 354; Invoice Issue Date: 2021-02-10"; // اجبارى
                inv.billingReference.invoiceDocumentReferences.Add(invoiceDocumentReference);
            }
            // åäÇ ããßä ÇÖíÝ Çá pih ãä ÞÇÚÏÉ ÇáÈíÇäÇÊ  
            inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
            // ÞíãÉ ÚÏÇÏ ÇáÝÇÊæÑÉ
            inv.AdditionalDocumentReferenceICV.UUID = 123456; // áÇÈÏ Çä íßæä ÇÑÞÇã ÝÞØ

            //Ýì ÍÇáÉ ÝÇÊæÑÉ ãÈÓØÉ æÝÇÊæÑÉ ãáÎÕÉ åÇäßÊÈ ÊÇÑíÎ ÇáÊÓáíã æÇÎÑ ÊÇÑíÎ ÇáÊÓáíã
            //
            //ÈíÇäÇÊ ÇáÏÝÚ 
            // ÇßæÇÏ ãÚíä
            // ÇÎÊíÇÑì ßæÏ ÇáÏÝÚ
            PaymentMeans paymentMeans = new PaymentMeans();
            paymentMeans.PaymentMeansCode = "10";//ÇÎÊíÇÑì
            //PaymentMeans paymentMeans1 = new PaymentMeans();
            //paymentMeans1.PaymentMeansCode = "42";//ÇÎÊíÇÑì
            //paymentMeans1.InstructionNote = "Payment Notes"; //ÇÌÈÇÑì Ýì ÇáÇÔÚÇÑÇÊ
            //inv.paymentmeans.payeefinancialaccount.ID = "";//ÇÎÊíÇÑì
            //inv.paymentmeans.payeefinancialaccount.paymentnote = "Payment by credit";//ÇÎÊíÇÑì
            inv.paymentmeans.Add(paymentMeans);
            //inv.paymentmeans.Add(paymentMeans1);
            //ÈíÇäÇÊ ÇáÈÇÆÚ 
            inv.SupplierParty.partyIdentification.ID = "2050012095"; // ÑÞã ÇáÓÌá ÇáÊÌÇÑì ÇáÎÇÖ ÈÇáÈÇÆÚ
            inv.SupplierParty.partyIdentification.schemeID = "CRN"; //ÑÞã ÇáÓÌá ÇáÊÌÇÑì
            inv.SupplierParty.postalAddress.StreetName = "streetnumber";// ÇÌÈÇÑì
            inv.SupplierParty.postalAddress.AdditionalStreetName = "ststtstst"; //ÇÎÊíÇÑì
            inv.SupplierParty.postalAddress.BuildingNumber = "3724"; // ÇÌÈÇÑì ÑÞã ÇáãÈäì
            inv.SupplierParty.postalAddress.PlotIdentification = "9833";//ÇÎÊíÇÑì ÑÞã ÇáÞØÚÉ
            inv.SupplierParty.postalAddress.CityName = "gaddah"; //ÇÓã ÇáãÏíäÉ
            inv.SupplierParty.postalAddress.PostalZone = "15385";//ÇáÑÞã ÇáÈÑíÏí
            inv.SupplierParty.postalAddress.CountrySubentity = "makka";//ÇÓã ÇáãÍÇÝÙÉ Çæ ÇáãÏíäÉ ãËÇá (ãßÉ) ÇÎÊíÇÑì
            inv.SupplierParty.postalAddress.CitySubdivisionName = "flassk";// ÇÓã ÇáãäØÞÉ Çæ ÇáÍì 
            inv.SupplierParty.postalAddress.country.IdentificationCode = "SA";
            inv.SupplierParty.partyLegalEntity.RegistrationName = "ãÄÓÓÉ ÎáíÌ ÌÇÒÇä ááÊÌÇÑÉ"; // ÇÓã ÇáÔÑßÉ ÇáãÓÌá Ýì ÇáåíÆÉ
            inv.SupplierParty.partyTaxScheme.CompanyID = "310901645200003";// ÑÞã ÇáÊÓÌíá ÇáÖÑíÈí

            // ÈíÇäÇÊ ÇáãÔÊÑì
            inv.CustomerParty.partyIdentification.ID = "123456";// ÑÞã ÇáÓÌá ÇáÊÌÇÑì ÇáÎÇÖ ÈÇáãÔÊÑì
            inv.CustomerParty.partyIdentification.schemeID = "NAT";//ÑÞã ÇáÓÌá ÇáÊÌÇÑì
            inv.CustomerParty.postalAddress.StreetName = "Kemarat Street,";// ÇÌÈÇÑì
            inv.CustomerParty.postalAddress.AdditionalStreetName = "";//ÇÎÊíÇÑì
            inv.CustomerParty.postalAddress.BuildingNumber = "3724";// ÇÌÈÇÑì ÑÞã ÇáãÈäì
            inv.CustomerParty.postalAddress.PlotIdentification = "9833";//ÇÎÊíÇÑì ÑÞã ÇáÞØÚÉ
            inv.CustomerParty.postalAddress.CityName = "Jeddah"; //ÇÓã ÇáãÏíäÉ
            inv.CustomerParty.postalAddress.PostalZone = "15385";//ÇáÑÞã ÇáÈÑíÏí
            inv.CustomerParty.postalAddress.CountrySubentity = "Makkah";//ÇÓã ÇáãÍÇÝÙÉ Çæ ÇáãÏíäÉ ãËÇá (ãßÉ) ÇÎÊíÇÑì
            inv.CustomerParty.postalAddress.CitySubdivisionName = "Alfalah";// ÇÓã ÇáãäØÞÉ Çæ ÇáÍì 
            inv.CustomerParty.postalAddress.country.IdentificationCode = "SA";
            inv.CustomerParty.partyLegalEntity.RegistrationName = "buyyername";// ÇÓã ÇáÔÑßÉ ÇáãÓÌá Ýì ÇáåíÆÉ
            inv.CustomerParty.partyTaxScheme.CompanyID = "301121971100003";// ÑÞã ÇáÊÓÌíá ÇáÖÑíÈí


            inv.legalMonetaryTotal.PayableAmount = 0;

            InvoiceLine invline = new InvoiceLine();
            //Product Quantity
            invline.InvoiceQuantity = 10;
            //Product Name
            invline.item.Name ="Item1";

          
                invline.item.classifiedTaxCategory.ID = "S"; // كود الضريبة
                                                             //item Tax code
                invline.taxTotal.TaxSubtotal.taxCategory.ID = "S"; // كود الضريبة
           
            //item Tax percentage
            invline.item.classifiedTaxCategory.Percent = 15; // نسبة الضريبة
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = 15; // نسبة الضريبة
            //EncludingVat = false this flag will be false in case you will give me Product Price not including vat
            //EncludingVat = true this flag will be true in case you will give me Product Price including vat
            invline.price.EncludingVat = false;
            //Product Price
            invline.price.PriceAmount = 10;

                // incase there is discount in invoice line level
                AllowanceCharge allowanceCharge = new AllowanceCharge();
                // فى حالة الرسوم incase of charges
                // allowanceCharge.ChargeIndicator = true;
                // فى حالة الخصم incase of discount
                allowanceCharge.ChargeIndicator = false;

                allowanceCharge.AllowanceChargeReason = "discount"; // سبب الخصم على مستوى المنتج
                                                                    // allowanceCharge.AllowanceChargeReasonCode = "90"; // سبب الخصم على مستوى المنتج
                allowanceCharge.Amount = 0; // قيم الخصم discount amount or charge amount

                allowanceCharge.MultiplierFactorNumeric = 0;
                allowanceCharge.BaseAmount = 0;
                invline.allowanceCharges.Add(allowanceCharge);
            
            inv.InvoiceLines.Add(invline);


            //string publickey = @"MIIFADCCBKWgAwIBAgITbQAAGw/UXgsmTms9LgABAAAbDzAKBggqhkjOPQQDAjBiMRUwEwYKCZImiZPyLGQBGRYFbG9jYWwxEzARBgoJkiaJk/IsZAEZFgNnb3YxFzAVBgoJkiaJk/IsZAEZFgdleHRnYXp0MRswGQYDVQQDExJQRVpFSU5WT0lDRVNDQTItQ0EwHhcNMjMwOTIxMDgxODAyWhcNMjUwOTIxMDgyODAyWjBcMQswCQYDVQQGEwJTQTEMMAoGA1UEChMDVFNUMRYwFAYDVQQLEw1SaXlhZGggQnJhbmNoMScwJQYDVQQDEx5UU1QtMjA1MDAxMjA5NS0zMDAwMDAxMzUyMjAwMDMwVjAQBgcqhkjOPQIBBgUrgQQACgNCAASbUK/x5nG7tMATY9Z/u60/eKzfGtdM2WbAFe654OPM1Fb1aBj/JEqgSp5dJQtuahldiKPfJ8aCH8I1tN0cbRxBo4IDQTCCAz0wJwYJKwYBBAGCNxUKBBowGDAKBggrBgEFBQcDAjAKBggrBgEFBQcDAzA8BgkrBgEEAYI3FQcELzAtBiUrBgEEAYI3FQiBhqgdhND7EobtnSSHzvsZ08BVZoGc2C2D5cVdAgFkAgETMIHNBggrBgEFBQcBAQSBwDCBvTCBugYIKwYBBQUHMAKGga1sZGFwOi8vL0NOPVBFWkVJTlZPSUNFU0NBMi1DQSxDTj1BSUEsQ049UHVibGljJTIwS2V5JTIwU2VydmljZXMsQ049U2VydmljZXMsQ049Q29uZmlndXJhdGlvbixEQz1leHRnYXp0LERDPWdvdixEQz1sb2NhbD9jQUNlcnRpZmljYXRlP2Jhc2U/b2JqZWN0Q2xhc3M9Y2VydGlmaWNhdGlvbkF1dGhvcml0eTAdBgNVHQ4EFgQU6PKLogVxfkECr0gYpM0CSaBn1m8wDgYDVR0PAQH/BAQDAgeAMIGtBgNVHREEgaUwgaKkgZ8wgZwxOzA5BgNVBAQMMjEtVFNUfDItVFNUfDMtOTVjNjRhZjgtYTI4NS00ZGFlLTg4MDMtYWYwNzNhZmU4ZjBkMR8wHQYKCZImiZPyLGQBAQwPMzAwMDAwMTM1MjIwMDAzMQ0wCwYDVQQMDAQxMTAwMQ4wDAYDVQQaDAVNYWtrYTEdMBsGA1UEDwwUTWVkaWNhbCBMYWJvcmF0b3JpZXMwgeQGA1UdHwSB3DCB2TCB1qCB06CB0IaBzWxkYXA6Ly8vQ049UEVaRUlOVk9JQ0VTQ0EyLUNBKDEpLENOPVBFWkVpbnZvaWNlc2NhMixDTj1DRFAsQ049UHVibGljJTIwS2V5JTIwU2VydmljZXMsQ049U2VydmljZXMsQ049Q29uZmlndXJhdGlvbixEQz1leHRnYXp0LERDPWdvdixEQz1sb2NhbD9jZXJ0aWZpY2F0ZVJldm9jYXRpb25MaXN0P2Jhc2U/b2JqZWN0Q2xhc3M9Y1JMRGlzdHJpYnV0aW9uUG9pbnQwHwYDVR0jBBgwFoAUgfKje3J7vVCjap/x6NON1nuccLUwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMDMAoGCCqGSM49BAMCA0kAMEYCIQD52GbWVIWpbdu7B4BnDe+fIKlrAxRUjnGtcc8HiKCEDAIhAJqHLuv0Krp5+HiNCB6w5VPXBPhTKbKidRkZHeb2VTJ+";
            //string privateKey = @"MHQCAQEEIFMxGrBBfmGxmv3rAmuAKgGrqnyNQYAfKqr7OVKDzgDYoAcGBSuBBAAKoUQDQgAEm1Cv8eZxu7TAE2PWf7utP3is3xrXTNlmwBXuueDjzNRW9WgY/yRKoEqeXSULbmoZXYij3yfGgh/CNbTdHG0cQQ==";
            //string secretkey = "lHntHtEGWi+ZJtssv167Dy+R64uxf/PTMXg3CEGYfvM=";
            string publickey = @"MIICIzCCAcqgAwIBAgIGAZGe3NBoMAoGCCqGSM49BAMCMBUxEzARBgNVBAMMCmVJbnZvaWNpbmcwHhcNMjQwODI5MTU1OTEyWhcNMjkwODI4MjEwMDAwWjBcMQswCQYDVQQGEwJTQTEWMBQGA1UECwwNUml5YWRoIEJyYW5jaDEMMAoGA1UECgwDVFNUMScwJQYDVQQDDB5UU1QtMjA1MDAxMjA5NS0zMDA1ODkyODQ5MDAwMDMwVjAQBgcqhkjOPQIBBgUrgQQACgNCAASUSsHO+x6hNHMtO6eG3B6VUOd2jfPJ+2v5tKxiuzFcadVQ8f7X6O2Bll3DtC+EXmvGCSwKUawCH2DmSPx7MHa3o4HBMIG+MAwGA1UdEwEB/wQCMAAwga0GA1UdEQSBpTCBoqSBnzCBnDE7MDkGA1UEBAwyMS1UU1R8Mi1UU1R8My05OGI3ZjE2YS1hYmYwLTQ2Y2UtYWM0Yi01OTYyZGJiMWEyM2UxHzAdBgoJkiaJk/IsZAEBDA8zMTA5MDE2NDUyMDAwMDMxDTALBgNVBAwMBDExMDAxDjAMBgNVBBoMBU1ha2thMR0wGwYDVQQPDBRNZWRpY2FsIExhYm9yYXRvcmllczAKBggqhkjOPQQDAgNHADBEAiA+UuQ02k0tOFbVQIp0fzjYpOG1wb1TOzpHQlB0EGtK5AIgKrauZcHYNozfNSxGOChZxWOwgY5W8T7/Lhc0iOk0Fv8=";
            string privateKey = @"MHQCAQEEIJMOc02tEA+HncIbHPxKHxNVx6mMIZSIJjJcCAp6ZOGIoAcGBSuBBAAKoUQDQgAElErBzvseoTRzLTunhtwelVDndo3zyftr+bSsYrsxXGnVUPH+1+jtgZZdw7QvhF5rxgksClGsAh9g5kj8ezB2tw==";
            string secretkey = "xZLpAHj8bg6VY4brdKJC7eC/EoCVHCu3MISF/MW2gLU=";
            inv.cSIDInfo.CertPem = publickey;

            inv.cSIDInfo.PrivateKey = privateKey;
            
            InvoiceTotal CalculateInvoiceTotal = ubl.CalculateInvoiceTotal(inv.InvoiceLines, inv.allowanceCharges);
            res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory());
            if (res.IsValid)
            {

            }
            else
            {
                invoicereportingmodel.ErrorMessage = res.ErrorMessage;
                return BadRequest(invoicereportingmodel);
                
            }

            Mode mode = Mode.developer;
            ApiRequestLogic apireqlogic = new ApiRequestLogic(mode);
            InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest();
            invrequestbody.invoice = res.EncodedInvoice;
            invrequestbody.invoiceHash = res.InvoiceHash;
            invrequestbody.uuid = res.UUID;
            if (mode == Mode.developer)
            {

                invoicereportingmodel = apireqlogic.CallComplianceInvoiceAPI(Utility.ToBase64Encode(publickey), secretkey, invrequestbody);
                
                    if(invoicereportingmodel.IsSuccess)
                    return Ok(invoicereportingmodel);
               else
                return BadRequest(invoicereportingmodel);

            }
            else
            {
                invoicereportingmodel = apireqlogic.CallReportingAPI(Utility.ToBase64Encode(publickey), secretkey, invrequestbody);
               if(invoicereportingmodel.IsSuccess)
                    return Ok(invoicereportingmodel);
               else
                return BadRequest(invoicereportingmodel);
            }

        }
        private InvoiceLine GetInvoiceLine(string itemname, decimal invoicedquantity, decimal basequantity, decimal itemprice, AllowanceChargeCollection allowanceCharges, string vatcategory, decimal vatpercentage, bool includingvat = false, string TaxExemptionReasonCode = "", string TaxExemptionReason = "")
        {
            InvoiceLine invline = new InvoiceLine();
            invline.item.Name = itemname;
            invline.InvoiceQuantity = invoicedquantity; // 
            invline.price.BaseQuantity = basequantity;
            invline.price.PriceAmount = itemprice;// ÓÚÑ ÇáãäÊÌ  

            invline.item.classifiedTaxCategory.ID = vatcategory;// ßæÏ ÇáÖÑíÈÉ
            invline.item.classifiedTaxCategory.Percent = vatpercentage;// äÓÈÉ ÇáÖÑíÈÉ
            invline.allowanceCharges = allowanceCharges;
            //if the price is including vat set EncludingVat=true;
            invline.price.EncludingVat = includingvat;



            invline.taxTotal.TaxSubtotal.taxCategory.ID = vatcategory;//ßæÏ ÇáÖÑíÈÉ
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = vatpercentage;//äÓÈÉ ÇáÖÑíÈÉ
            if (vatcategory != "S")
            {
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReason = TaxExemptionReason;
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReasonCode = TaxExemptionReasonCode;
            }

            return invline;
        }
       
    }
}

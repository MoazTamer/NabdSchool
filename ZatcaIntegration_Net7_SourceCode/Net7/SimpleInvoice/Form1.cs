using net.sf.saxon.functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using ZatcaIntegrationSDK;
using ZatcaIntegrationSDK.APIHelper;
using ZatcaIntegrationSDK.BLL;
using ZatcaIntegrationSDK.HelperContracts;
using ZXing;
using ZXing.Common;

namespace SimpleInvoice
{
    public partial class Form1 : Form
    {

        private Mode mode = Mode.developer;
        private class InvoiceItems
        {
            public string ProductName { get; set; }
            public decimal ProductPrice { get; set; }
            public decimal ProductQuantity { get; set; }
            public decimal TotalPrice { get; set; }
            public decimal DiscountValue { get; set; }
            public decimal TotalPriceAfterDiscount { get; set; }
            public decimal VatPercentage { get; set; }
            public decimal VatValue { get; set; }
            public decimal TotalWithVat { get; set; }
        }
        List<InvoiceItems> invlines;
        public static string GetNodeInnerText(XmlDocument doc, string xPath)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            XmlNode titleNode = doc.SelectSingleNode(xPath, nsmgr);
            if (titleNode != null)
            {
                return titleNode.InnerText;
            }
            return "";
        }
        public Form1()
        {
            InitializeComponent();
            FillSellerOtherIdentification();
            FillBuyerOtherIdentification();
            
    }
        public string GetInvoiceHash(string SignedXmlPath)
        {
            string hash = "";
            string Hash_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'UBLExtensions']/*[local-name() = 'UBLExtension']/*[local-name() = 'ExtensionContent']/*[local-name() = 'UBLDocumentSignatures']/*[local-name() = 'SignatureInformation']/*[local-name() = 'Signature']/*[local-name() = 'SignedInfo']/*[local-name() = 'Reference' and @Id='invoiceSignedData']/*[local-name() = 'DigestValue']";
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            try
            {
                doc.Load(SignedXmlPath);
                hash = GetNodeInnerText(doc, Hash_XPATH);
            }
            catch
            {
               
            }
            
            return hash;
        }
        private InvoiceLine GetAdditionalInvoiceLine(string itemname, AllowanceChargeCollection allowanceCharges, string vatcategory, decimal vatpercentage, string TaxExemptionReasonCode = "", string TaxExemptionReason = "")
        {
            InvoiceLine invline = new InvoiceLine();
            invline.item.Name = "Prepayment adjustment";
            invline.InvoiceQuantity = 1; // 
            invline.price.PriceAmount = 0;// سعر المنتج  

            invline.item.classifiedTaxCategory.ID = vatcategory;// كود الضريبة
            invline.item.classifiedTaxCategory.Percent = vatpercentage;// نسبة الضريبة
            invline.allowanceCharges = allowanceCharges;

            invline.taxTotal.TaxSubtotal.taxCategory.ID = vatcategory;//كود الضريبة
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = vatpercentage;//نسبة الضريبة
            if (vatcategory != "S")
            {
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReason = TaxExemptionReason;
                invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReasonCode = TaxExemptionReasonCode;
            }
            invline.taxTotal.TaxSubtotal.TaxableAmount = 1000m;
            invline.taxTotal.TaxSubtotal.TaxAmount = 150m;
            DocumentReference documentReference = new DocumentReference();
            documentReference.ID = "Inv-10000001";
            documentReference.IssueDate = "2023-05-10";
            documentReference.IssueTime = "11:25:55";
            documentReference.DocumentTypeCode = 386;

            DocumentReference documentReference1 = new DocumentReference();
            documentReference1.ID = "Inv-10000002";
            documentReference1.IssueDate = "2023-05-11";
            documentReference1.IssueTime = "11:25:55";
            documentReference1.DocumentTypeCode = 386;
            invline.documentReferences.Add(documentReference);
            invline.documentReferences.Add(documentReference1);
            return invline;
        }
        private InvoiceLine GetInvoiceLine(string itemname, decimal qty, decimal price, string vatcategory, decimal vatpercentage, string TaxExemptionReasonCode = "", string TaxExemptionReason = "")
        {
            InvoiceLine invline = new InvoiceLine();
            //Product Quantity
            invline.InvoiceQuantity = qty;
            //Product Name
            invline.item.Name = itemname;

            
            invline.item.classifiedTaxCategory.ID = vatcategory; // كود الضريبة
                                                         //item Tax code
            invline.taxTotal.TaxSubtotal.taxCategory.ID = vatcategory; // كود الضريبة
                                                               // }
                                                               //item Tax percentage
            invline.item.classifiedTaxCategory.Percent = vatpercentage; // نسبة الضريبة
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = vatpercentage; // نسبة الضريبة
                                                                                   //EncludingVat = false this flag will be false in case you will give me Product Price not including vat
                                                                                   //EncludingVat = true this flag will be true in case you will give me Product Price including vat
            invline.price.EncludingVat = false;
            //Product Price
            invline.price.PriceAmount = price;

            return invline;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //this method is not needed in integration (this is just for calculate Amounts in this screen)
            Calculate();

         
            UBLXML ubl = new UBLXML();
            Invoice inv = new Invoice();
            ZatcaIntegrationSDK.Result res = new ZatcaIntegrationSDK.Result();

            inv.ID = TextBox8.Text; // invoice number in your system example= SME00010
            //inv.UUID = Guid.NewGuid().ToString();
            inv.IssueDate = DateTimePicker1.Value.ToString("yyyy-MM-dd"); //invoice issue date with format yyyy-MM-dd example "2023-02-07"
            inv.IssueTime = DateTimePicker2.Value.ToString("HH:mm:ss"); // invoice issue date with format HH:mm:ss example "09:32:40"
            //all needed codes for invoiceTypeCode id
            // 388 sales invoice  
            // 383 debit note
            // 381 credit note
            // 386 دفع مسبق
            // get invoice type 
            inv.invoiceTypeCode.id = GetInvoiceType();
            // inv.invoiceTypeCode.Name based on format NNPNESB
            // NN 01 standard invoice 
            // NN 02 simplified invoice
            // P فى حالة فاتورة لطرف ثالث نكتب 1 فى الحالة الاخرى نكتب 0
            // N فى حالة فاتورة اسمية نكتب 1 وفى الحالة الاخرى نكتب 0
            // E فى حالة فاتورة للصادرات نكتب 1 وفى الحالة الاخرى نكتب 0
            // S فى حالة فاتورة ملخصة نكتب 1 وفى الحالة الاخرى نكتب 0
            // B فى حالة فاتورة ذاتية نكتب 1
            // B فى حالة ان الفاتورة صادرات=1 لايمكن ان تكون الفاتورة ذاتية =1
            // 
            inv.invoiceTypeCode.Name = GetInvoiceTypeName();
            inv.DocumentCurrencyCode = "SAR"; // Document Currency Code (invoice currency example SAR or USD) 
            inv.TaxCurrencyCode = "SAR"; // Tax Currency Code it must be with SAR
            inv.CurrencyRate = 3.75m; // incase of DocumentCurrencyCode equal any currency code not SAR we must mention CurrencyRate value
            if (inv.invoiceTypeCode.id == 383 || inv.invoiceTypeCode.id == 381)
            {
                // فى حالة ان اشعار دائن او مدين فقط هانكتب رقم الفاتورة اللى اصدرنا الاشعار ليها
                // in case of return sales invoice or debit notes we must mention the original sales invoice number
                InvoiceDocumentReference invoiceDocumentReference = new InvoiceDocumentReference();
                invoiceDocumentReference.ID = "from invoice number 500 to invoice number 505"; // mandatory in case of return sales invoice or debit notes
                inv.billingReference.invoiceDocumentReferences.Add(invoiceDocumentReference);
            }


            // هنا ممكن اضيف ال pih من قاعدة البيانات  
            //this is previous invoice hash (the invoice hash of last invoice ) res.InvoiceHash
            // for the first invoice and because there is no previous hash we must write this code "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ=="
            inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
            // قيمة عداد الفاتورة
            // Invoice counter (1,2,3,4) this counter must start from 1 for each CSID
            inv.AdditionalDocumentReferenceICV.UUID = Int32.Parse(TextBox7.Text); // لابد ان يكون ارقام فقط must be numbers only
            
            if (inv.invoiceTypeCode.Name.Substring(0, 2) == "01")
            {
                //supply date mandatory only for standard invoices
                // فى حالة فاتورة مبسطة وفاتورة ملخصة هانكتب تاريخ التسليم واخر تاريخ التسليم
                inv.delivery.ActualDeliveryDate = "2022-10-22";
                inv.delivery.LatestDeliveryDate = "2022-10-23";
            }
            // 
            // بيانات الدفع 
            // اكواد معين
            // اختيارى كود الدفع
            // payment methods mandatory for return invoice and debit notes and optional for invoices
            string paymentcode = GetPaymentMethod();
            if (!string.IsNullOrEmpty(paymentcode))
            {
                PaymentMeans paymentMeans = new PaymentMeans();
                paymentMeans.PaymentMeansCode = paymentcode; // optional for invoices - mandatory for return invoice - debit notes
                if (inv.invoiceTypeCode.id == 383 || inv.invoiceTypeCode.id == 381)
                {
                    paymentMeans.InstructionNote = "dameged items"; //the reason of return invoice - debit notes // manatory only for return invoice - debit notes 
                }
                paymentMeans.payeefinancialaccount.ID = "";
                paymentMeans.payeefinancialaccount.paymentnote = "";
                inv.paymentmeans.Add(paymentMeans);
            }

            // بيانات البائع 
            //seller date
            //other identifier for seller like commercial registration number
            inv.SupplierParty.partyIdentification.ID = textBox4.Text; // رقم السجل التجارى الخاض بالبائع
            //other identifier scheme id example CRN for commercial registration number
            inv.SupplierParty.partyIdentification.schemeID = cmb_seller_scheme.SelectedValue.ToString(); // رقم السجل التجارى
            //seller street name mandatory
            inv.SupplierParty.postalAddress.StreetName = textBox11.Text; // اجبارى
            //inv.SupplierParty.postalAddress.AdditionalStreetName = ""; // اختيارى
           //seller buliding number mandatory must be 4 digits
            inv.SupplierParty.postalAddress.BuildingNumber = textBox12.Text; // اجبارى رقم المبنى
           // inv.SupplierParty.postalAddress.PlotIdentification = "9833"; //اختيارى
           //seller city name 
            inv.SupplierParty.postalAddress.CityName = "taif"; // اسم المدينة
            //seller postal zone must be 5 digits 
            inv.SupplierParty.postalAddress.PostalZone = textBox14.Text; // الرقم البريدي
            //inv.SupplierParty.postalAddress.CountrySubentity = "Riyadh Region"; // اسم المحافظة او المدينة مثال (مكة) اختيارى
            //seller City Subdivision Name
            inv.SupplierParty.postalAddress.CitySubdivisionName = textBox13.Text; // اسم المنطقة او الحى 
            //SA for Saudi it must be SA with seller data
            inv.SupplierParty.postalAddress.country.IdentificationCode = "SA";
            // seller company name
            inv.SupplierParty.partyLegalEntity.RegistrationName = textBox10.Text; // اسم الشركة المسجل فى الهيئة
           //seller vat registration number must be 15 digits and start with 3 and end with 3
            inv.SupplierParty.partyTaxScheme.CompanyID = textBox9.Text;  // رقم التسجيل الضريبي

            if (inv.invoiceTypeCode.Name.Substring(0, 2) == "01")
            {
                // بيانات المشترى
                inv.CustomerParty.partyIdentification.ID = textBox21.Text; // رقم السجل التجارى الخاص بالمشترى
                inv.CustomerParty.partyIdentification.schemeID = cmb_buyer_scheme.SelectedValue.ToString(); //رقم السجل التجارى
                inv.CustomerParty.postalAddress.StreetName = textBox18.Text; // اجبارى
                                                                             //inv.CustomerParty.postalAddress.AdditionalStreetName = "street name"; // اختيارى
                inv.CustomerParty.postalAddress.BuildingNumber = textBox17.Text; // اجبارى رقم المبنى
                                                                                 // inv.CustomerParty.postalAddress.PlotIdentification = "9833"; // اختيارى رقم القطعة
                inv.CustomerParty.postalAddress.CityName = "Jeddah"; // اسم المدينة
                inv.CustomerParty.postalAddress.PostalZone = textBox15.Text; // الرقم البريدي
                                                                             //inv.CustomerParty.postalAddress.CountrySubentity = "Makkah"; // اسم المحافظة او المدينة مثال (مكة) اختيارى
                inv.CustomerParty.postalAddress.CitySubdivisionName = textBox16.Text; // اسم المنطقة او الحى 
                inv.CustomerParty.postalAddress.country.IdentificationCode = "SA";
                inv.CustomerParty.partyLegalEntity.RegistrationName = textBox19.Text; // اسم الشركة المسجل فى الهيئة
                inv.CustomerParty.partyTaxScheme.CompanyID = textBox20.Text; // رقم التسجيل الضريبي
            }
            //inv.CustomerParty.contact.Name = "Amr Sobhy";  
            //inv.CustomerParty.contact.Telephone = "0555252";
            //inv.CustomerParty.contact.ElectronicMail = "amr@amr.com";
            //inv.CustomerParty.contact.Note = "notes other notes";
            decimal invoicediscount = 0;
            Decimal.TryParse(TextBox2.Text, out invoicediscount);
            if (invoicediscount > 0)
            {
                //this code incase of there is a discount in invoice level 
                AllowanceCharge allowance = new AllowanceCharge();
                //ChargeIndicator = false means that this is discount
                //ChargeIndicator = true means that this is charges(like cleaning service - transportation)
                allowance.ChargeIndicator = false;
                //write this lines in case you will make discount as percentage
                allowance.MultiplierFactorNumeric = 0; //dscount percentage like 10
                allowance.BaseAmount = 0; // the amount we will apply percentage on example (MultiplierFactorNumeric=10 ,BaseAmount=1000 then AllowanceAmount will be 100 SAR)

                // in case we will make discount as Amount 
                allowance.Amount =  invoicediscount; // 
               // allowance.AllowanceChargeReasonCode = "95"; //discount or charge reason code
                allowance.AllowanceChargeReason = "discount"; //discount or charge reson
                allowance.taxCategory.ID = "S";// كود الضريبة tax code (S Z O E )
                allowance.taxCategory.Percent = 15;// نسبة الضريبة tax percentage (0 - 15 - 5 )
                //فى حالة عندى اكثر من خصم بعمل loop على الاسطر السابقة
                inv.allowanceCharges.Add(allowance);
            }
            
            decimal payableamount = 0;
            Decimal.TryParse(textBox23.Text, out payableamount);
            //this is the invoice total amount (invoice total with vat) and you can set its value with Zero and i will calculate it from sdk
            inv.legalMonetaryTotal.PayableAmount = payableamount;
            // فى حالة فى اكتر من منتج فى الفاتورة هانعمل ليست من invoiceline مثال الكود التالى
            //here we will mention all invoice lines data
            foreach (InvoiceItems item in invlines)
            {
                InvoiceLine invline = new InvoiceLine();
                //Product Quantity
                invline.InvoiceQuantity = item.ProductQuantity;
                //Product Name
                invline.item.Name = item.ProductName;

                if (item.VatPercentage == 0)
                {
                    //item Tax code
                    invline.item.classifiedTaxCategory.ID = "Z"; // كود الضريبة
                    //item Tax code
                    invline.taxTotal.TaxSubtotal.taxCategory.ID = "Z"; // كود الضريبة
                     //item Tax Exemption Reason Code mentioned in zatca pdf page(32-33)
                    invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReasonCode = "VATEX-SA-35"; // كود الضريبة
                   //item Tax Exemption Reason mentioned in zatca pdf page(32-33)
                    invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReason = "Medicines and medical equipment"; // كود الضريبة

                }
                else
                {
                    //item Tax code
                    invline.item.classifiedTaxCategory.ID = "S"; // كود الضريبة
                     //item Tax code
                    invline.taxTotal.TaxSubtotal.taxCategory.ID = "S"; // كود الضريبة
                }
                //item Tax percentage
                invline.item.classifiedTaxCategory.Percent = item.VatPercentage; // نسبة الضريبة
                invline.taxTotal.TaxSubtotal.taxCategory.Percent = item.VatPercentage; // نسبة الضريبة
                //EncludingVat = false this flag will be false in case you will give me Product Price not including vat
                //EncludingVat = true this flag will be true in case you will give me Product Price including vat
                invline.price.EncludingVat = false;
                //Product Price
                invline.price.PriceAmount = item.ProductPrice;

                if (item.DiscountValue > 0)
                {
                    // incase there is discount in invoice line level
                    AllowanceCharge allowanceCharge = new AllowanceCharge();
                    // فى حالة الرسوم incase of charges
                    // allowanceCharge.ChargeIndicator = true;
                    // فى حالة الخصم incase of discount
                    allowanceCharge.ChargeIndicator = false;

                    allowanceCharge.AllowanceChargeReason = "discount"; // سبب الخصم على مستوى المنتج
                    // allowanceCharge.AllowanceChargeReasonCode = "90"; // سبب الخصم على مستوى المنتج
                    allowanceCharge.Amount = item.DiscountValue; // قيم الخصم discount amount or charge amount

                    allowanceCharge.MultiplierFactorNumeric = 0;
                    allowanceCharge.BaseAmount = 0;
                    invline.allowanceCharges.Add(allowanceCharge);
                }
                inv.InvoiceLines.Add(invline);
            }

            //this calculation just for test invoice calculation and you may don't need this lines
            //start
            InvoiceTotal invoiceTotal = ubl.CalculateInvoiceTotal(inv.InvoiceLines, inv.allowanceCharges);
            TextBox1.Text = invoiceTotal.LineExtensionAmount.ToString("0.00");
            TextBox3.Text = invoiceTotal.TaxExclusiveAmount.ToString("0.00");
            TextBox6.Text = invoiceTotal.TaxInclusiveAmount.ToString("0.00");
            TextBox5.Text = (invoiceTotal.TaxInclusiveAmount - invoiceTotal.TaxExclusiveAmount).ToString("0.00");
            //end

            // here you can pass csid data
            //this is csid or publickey
            //inv.cSIDInfo.CertPem publickey
            //inv.cSIDInfo.PrivateKey
            inv.cSIDInfo.CertPem = @"MIIFADCCBKWgAwIBAgITbQAAGw/UXgsmTms9LgABAAAbDzAKBggqhkjOPQQDAjBiMRUwEwYKCZImiZPyLGQBGRYFbG9jYWwxEzARBgoJkiaJk/IsZAEZFgNnb3YxFzAVBgoJkiaJk/IsZAEZFgdleHRnYXp0MRswGQYDVQQDExJQRVpFSU5WT0lDRVNDQTItQ0EwHhcNMjMwOTIxMDgxODAyWhcNMjUwOTIxMDgyODAyWjBcMQswCQYDVQQGEwJTQTEMMAoGA1UEChMDVFNUMRYwFAYDVQQLEw1SaXlhZGggQnJhbmNoMScwJQYDVQQDEx5UU1QtMjA1MDAxMjA5NS0zMDAwMDAxMzUyMjAwMDMwVjAQBgcqhkjOPQIBBgUrgQQACgNCAASbUK/x5nG7tMATY9Z/u60/eKzfGtdM2WbAFe654OPM1Fb1aBj/JEqgSp5dJQtuahldiKPfJ8aCH8I1tN0cbRxBo4IDQTCCAz0wJwYJKwYBBAGCNxUKBBowGDAKBggrBgEFBQcDAjAKBggrBgEFBQcDAzA8BgkrBgEEAYI3FQcELzAtBiUrBgEEAYI3FQiBhqgdhND7EobtnSSHzvsZ08BVZoGc2C2D5cVdAgFkAgETMIHNBggrBgEFBQcBAQSBwDCBvTCBugYIKwYBBQUHMAKGga1sZGFwOi8vL0NOPVBFWkVJTlZPSUNFU0NBMi1DQSxDTj1BSUEsQ049UHVibGljJTIwS2V5JTIwU2VydmljZXMsQ049U2VydmljZXMsQ049Q29uZmlndXJhdGlvbixEQz1leHRnYXp0LERDPWdvdixEQz1sb2NhbD9jQUNlcnRpZmljYXRlP2Jhc2U/b2JqZWN0Q2xhc3M9Y2VydGlmaWNhdGlvbkF1dGhvcml0eTAdBgNVHQ4EFgQU6PKLogVxfkECr0gYpM0CSaBn1m8wDgYDVR0PAQH/BAQDAgeAMIGtBgNVHREEgaUwgaKkgZ8wgZwxOzA5BgNVBAQMMjEtVFNUfDItVFNUfDMtOTVjNjRhZjgtYTI4NS00ZGFlLTg4MDMtYWYwNzNhZmU4ZjBkMR8wHQYKCZImiZPyLGQBAQwPMzAwMDAwMTM1MjIwMDAzMQ0wCwYDVQQMDAQxMTAwMQ4wDAYDVQQaDAVNYWtrYTEdMBsGA1UEDwwUTWVkaWNhbCBMYWJvcmF0b3JpZXMwgeQGA1UdHwSB3DCB2TCB1qCB06CB0IaBzWxkYXA6Ly8vQ049UEVaRUlOVk9JQ0VTQ0EyLUNBKDEpLENOPVBFWkVpbnZvaWNlc2NhMixDTj1DRFAsQ049UHVibGljJTIwS2V5JTIwU2VydmljZXMsQ049U2VydmljZXMsQ049Q29uZmlndXJhdGlvbixEQz1leHRnYXp0LERDPWdvdixEQz1sb2NhbD9jZXJ0aWZpY2F0ZVJldm9jYXRpb25MaXN0P2Jhc2U/b2JqZWN0Q2xhc3M9Y1JMRGlzdHJpYnV0aW9uUG9pbnQwHwYDVR0jBBgwFoAUgfKje3J7vVCjap/x6NON1nuccLUwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMDMAoGCCqGSM49BAMCA0kAMEYCIQD52GbWVIWpbdu7B4BnDe+fIKlrAxRUjnGtcc8HiKCEDAIhAJqHLuv0Krp5+HiNCB6w5VPXBPhTKbKidRkZHeb2VTJ+";
            inv.cSIDInfo.PrivateKey = @"MHQCAQEEIFMxGrBBfmGxmv3rAmuAKgGrqnyNQYAfKqr7OVKDzgDYoAcGBSuBBAAKoUQDQgAEm1Cv8eZxu7TAE2PWf7utP3is3xrXTNlmwBXuueDjzNRW9WgY/yRKoEqeXSULbmoZXYij3yfGgh/CNbTdHG0cQQ==";
            string secretkey = "lHntHtEGWi+ZJtssv167Dy+R64uxf/PTMXg3CEGYfvM=";
            CSIDInfo info = MainSettings.GetCSIDInfo();
            //this keys is CSID information you can generate it from AutoGenerateCSR form
            //this keys you will generate it one time and it will be valid in simulation for 2 years - for production 5 years

            //inv.cSIDInfo.CertPem = info.PublicKey;
            //inv.cSIDInfo.PrivateKey = info.PrivateKey;
            //string secretkey = info.SecretKey;
            try
            {
                //string g=Guid.NewGuid().ToString();
                //if you need to save xml file true if not false;
                bool savexmlfile = rdb_savexml.Checked;
                // this method is used to generate xml file with invoice data 
                //Directory.GetCurrentDirectory() or Directory that you need to save xml file 
                //after call this method xml is created if res.IsValid=true
                res = ubl.GenerateInvoiceXML(inv, Directory.GetCurrentDirectory(), savexmlfile);
               // res.IsValid must equal true to be ready to send to zatca api
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString() + "\n\n" + ex.InnerException.ToString());
            }
            //
            if (res.IsValid)
            {
                if (string.IsNullOrEmpty(textBox23.Text))
                {
                    textBox23.Text = res.PayableAmount;
                }
                //res.SingedXML palintext
                //res.EncodedInvoice base64
                //res.InvoiceHash
                //res.UUID
                //res.QRCode
                //res.TaxAmount
                // here you can save all xml data into database
                //and show QRCode
                //all data you must save is
            }
            else
            {

                //
                MessageBox.Show(res.ErrorMessage);
                return;
                

            }
            //second step
            //Sending modes
            // developer mode (for developers only)
            if (rdb_simulation.Checked)
                mode = Mode.Simulation; //simulation mode (for test)
            else if (rdb_production.Checked)
                mode = Mode.Production;//production mode for live
            else
                mode = Mode.developer;

            // zatca call api
            ApiRequestLogic apireqlogic = new ApiRequestLogic(mode, Directory.GetCurrentDirectory(), true);

            InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest();
            invrequestbody.invoice = res.EncodedInvoice; //this is xml file with base 64 format
            invrequestbody.invoiceHash = res.InvoiceHash; // invoicehash of xml
            invrequestbody.uuid = res.UUID;//uuid for xml



            //all this data is required for sending to zatca api
            if (rdb_complaice.Checked)
            {
                ComplianceCsrResponse tokenresponse = new ComplianceCsrResponse();
                string csr = @"-----BEGIN CERTIFICATE REQUEST-----
MIIB5DCCAYoCAQAwVTELMAkGA1UEBhMCU0ExFjAUBgNVBAsMDUVuZ2F6YXRCcmFu
Y2gxEDAOBgNVBAoMB0VuZ2F6YXQxHDAaBgNVBAMME1RTVC0zMDAzMDA4Njg2MDAw
MDMwVjAQBgcqhkjOPQIBBgUrgQQACgNCAARYvqwxwBzinhARQZYQnWBoSr8wMmmw
CdfTSleD+rZoh/NeJMF8reXaBFrMCrlPK0hTRXmCyXuc6nFUfjSvZU/goIHVMIHS
BgkqhkiG9w0BCQ4xgcQwgcEwIgYJKwYBBAGCNxQCBBUTE1RTVFpBVENBQ29kZVNp
Z25pbmcwgZoGA1UdEQSBkjCBj6SBjDCBiTE7MDkGA1UEBAwyMS1UU1R8Mi1UU1R8
My1lZDIyZjFkOC1lNmEyLTExMTgtOWI1OC1kOWE4ZjExZTQ0NWYxHzAdBgoJkiaJ
k/IsZAEBDA8zMDAzMDA4Njg2MDAwMDMxDTALBgNVBAwMBDExMDAxDDAKBgNVBBoM
A1RTVDEMMAoGA1UEDwwDVFNUMAoGCCqGSM49BAMCA0gAMEUCIQDRroaukEGwwRXW
RhOudGrd/OGrcUnnn2ftb6Jk4dDGFgIgaV+sXmaZlKbxR7k/lMhnf/2j95XHDkso
hup1ROPc+cc=
-----END CERTIFICATE REQUEST-----
";
                tokenresponse = apireqlogic.GetComplianceCSIDAPI("12345", csr);
                if (String.IsNullOrEmpty(tokenresponse.ErrorMessage))
                {
                   InvoiceReportingResponse responsemodel = apireqlogic.CallComplianceInvoiceAPI(tokenresponse.BinarySecurityToken, tokenresponse.Secret, invrequestbody);
                    if (responsemodel.IsSuccess)
                    {
                        if (responsemodel.StatusCode == 202)
                        {
                            //save warning message in database to solve for next invoices
                            //responsemodel.WarningMessage
                        }
                        MessageBox.Show(responsemodel.ReportingStatus + responsemodel.ClearanceStatus); //REPORTED
                        PictureBox1.Image = QrCodeImage(res.QRCode, 200, 200);

                    }
                    else
                    {
                        MessageBox.Show(responsemodel.ErrorMessage);
                    }
                }
                else
                {
                    MessageBox.Show(tokenresponse.ErrorMessage);
                }
            }
            else
            {
                //this code is for simulation and production mode

                if (inv.invoiceTypeCode.Name.Substring(0,2) == "01")
                {
                    // to send standard invoices for clearing
                    //this this the calling of api 
                    //CLearedInvoice is a new xml after clearing it from zatca api with base64 format
                    InvoiceClearanceResponse responsemodel = apireqlogic.CallClearanceAPI(Utility.ToBase64Encode(inv.cSIDInfo.CertPem), secretkey, invrequestbody);
                    //if responsemodel.IsSuccess = true this means that your xml is successfully sent to zatca 
                    if (responsemodel.IsSuccess)
                    {
                        ///////////
                        
                        //if status code =202 it means that xml accepted but with warning 
                        //no need to sent xml again but you must solve that warning messages for the next invoices
                        if (responsemodel.StatusCode == 202)
                        { 
                            //save warning message in database to solve for next invoices
                            //responsemodel.WarningMessage
                        }

                        // Cleared
                        //MessageBox.Show(responsemodel.QRCode);
                        //responsemodel.ClearedInvoice
                        PictureBox1.Image = QrCodeImage(responsemodel.QRCode);
                        MessageBox.Show(responsemodel.ClearanceStatus);
                    }
                    else
                    {
                        MessageBox.Show(responsemodel.ErrorMessage);
                    }
                }
                else
                {
                    //to send simplified invoices for reporting
                    //this this the calling of api 
                    InvoiceReportingResponse responsemodel = apireqlogic.CallReportingAPI(Utility.ToBase64Encode(inv.cSIDInfo.CertPem), secretkey, invrequestbody);
                    MessageBox.Show(responsemodel.StatusCode.ToString());
                    if (responsemodel.IsSuccess)
                    {
                        //if status code =202 it means that xml accespted but with warning 
                        //no need to sent xml again but you must solve that warning messages for the next invoices
                        if (responsemodel.StatusCode == 202)
                        {
                            //save warning message in database to solve for next invoices
                            //responsemodel.WarningMessage
                        }
                       
                        PictureBox1.Image = QrCodeImage(res.QRCode);
                        MessageBox.Show(responsemodel.ReportingStatus);// Reported
                    }
                    else
                        MessageBox.Show(responsemodel.ErrorMessage);
                }


            }

        }


        private void Calculate()
        {
            try
            {
                invlines = new List<InvoiceItems>();
                for (int i = 0; i <= DataGridView1.RowCount - 1; i++)
                {
                    if (DataGridView1.Rows[i].Cells["TotalWithVat"].Value != null
                        && DataGridView1.Rows[i].Cells["ProductPrice"].Value != null
                        && DataGridView1.Rows[i].Cells["ProductQuantity"].Value != null
                        && DataGridView1.Rows[i].Cells["DiscountValue"].Value != null
                        && DataGridView1.Rows[i].Cells["VatPercentage"].Value != null)
                    {
                        var line = new InvoiceItems();
                        line.ProductName = DataGridView1.Rows[i].Cells["ProductName"].Value.ToString();
                        line.ProductPrice = Convert.ToDecimal(DataGridView1.Rows[i].Cells["ProductPrice"].Value.ToString());
                        line.ProductQuantity = Convert.ToDecimal(DataGridView1.Rows[i].Cells["ProductQuantity"].Value.ToString());
                        line.TotalPrice = Convert.ToDecimal(DataGridView1.Rows[i].Cells["TotalPrice"].Value.ToString());
                        line.DiscountValue = Convert.ToDecimal(DataGridView1.Rows[i].Cells["DiscountValue"].Value.ToString());
                        line.TotalPriceAfterDiscount = Convert.ToDecimal(DataGridView1.Rows[i].Cells["TotalPriceAfterDiscount"].Value.ToString());
                        line.VatPercentage = Convert.ToDecimal(DataGridView1.Rows[i].Cells["VatPercentage"].Value.ToString());
                        line.VatValue = Convert.ToDecimal(DataGridView1.Rows[i].Cells["VatValue"].Value.ToString());
                        line.TotalWithVat = Convert.ToDecimal(DataGridView1.Rows[i].Cells["TotalWithVat"].Value.ToString());
                        invlines.Add(line);
                    }

                }
                TextBox1.Text = invlines.Sum(m => m.TotalPrice).ToString();
                //TextBox2.Text = invlines.Sum(m => m.DiscountValue).ToString();
                decimal invoicediscount = 0;
                Decimal.TryParse(TextBox2.Text, out invoicediscount);
                TextBox3.Text = (invlines.Sum(m => m.TotalPriceAfterDiscount) - invoicediscount).ToString();
                TextBox5.Text = invlines.Sum(m => m.VatValue).ToString();
                TextBox6.Text = invlines.Sum(m => m.TotalWithVat).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString() + "\n\n" + ex.InnerException.ToString());
            }

        }

        private string GetPaymentMethod()
        {
            //PaymentMeansCode payment method codes
            //10 In cash
            //30 Credit
            //42 Payment to bank account
            //48 Bank card
            //1 Instrument Not defined(Free text)
            string PaymentCode = "";
            if (RadioButton14.Checked)
                PaymentCode = "10";
            else if (RadioButton11.Checked)
                PaymentCode = "30";
            else if (RadioButton10.Checked)
                PaymentCode = "42";
            else if (RadioButton9.Checked)
                PaymentCode = "48";
            else if (RadioButton12.Checked)
                PaymentCode = "1";
            else
                PaymentCode = "";
            return PaymentCode;
        }
        private int GetInvoiceType()
        {
            int InvoiceType = 388;
            if (RadioButton3.Checked)
                InvoiceType = 388;
            else if (RadioButton6.Checked)
                InvoiceType = 388;
            else if (RadioButton7.Checked)
                InvoiceType = 383;
            else if (RadioButton8.Checked)
                InvoiceType = 383;
            else if (RadioButton5.Checked)
                InvoiceType = 381;
            else if (RadioButton4.Checked)
                InvoiceType = 381;

            return InvoiceType;
        }

        public string GetInvoiceTypeName()
        {
            string InvoiceTypeName = "0100000";
            if (RadioButton3.Checked)
                InvoiceTypeName = "0200000";
            else if (RadioButton6.Checked)
                InvoiceTypeName = "0100000";
            else if (RadioButton7.Checked)
                InvoiceTypeName = "0200000";
            else if (RadioButton8.Checked)
                InvoiceTypeName = "0100000";
            else if (RadioButton5.Checked)
                InvoiceTypeName = "0200000";
            else if (RadioButton4.Checked)
                InvoiceTypeName = "0100000";
            return InvoiceTypeName;
        }
        public Bitmap QrCodeImage(string Qrcode, int width = 250, int height = 250)
        {

            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height
                }
            };
            Bitmap QrCode = barcodeWriter.Write(Qrcode);

            return QrCode;
        }
        public byte[] QrCodeToByteArray(string Qrcode, int width = 250, int height = 250)
        {
            byte[] data;
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height
                }
            };
            Bitmap QrCode = barcodeWriter.Write(Qrcode);
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                QrCode.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                data = stream.ToArray();
            }
            return data;
           
        }

        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //e.ColumnIndex = 1 price
            //e.ColumnIndex = 2 qty
            //e.ColumnIndex = 3 total price
            //e.ColumnIndex = 4 Discount Value
            //e.ColumnIndex = 5 Total Price After Discount
            //e.ColumnIndex = 6 Vat Percentage
            //e.ColumnIndex = 7 Vat Value

            decimal itemprice = 0;
            decimal itemqty = 0;
            decimal totalprice = 0;
            decimal discount = 0;
            decimal totalpriceafterdiscount = 0;
            decimal vatpercentage = 0;
            try
            {
                if (e.ColumnIndex == 1 || e.ColumnIndex == 2)
                {
                    if (DataGridView1.Rows[e.RowIndex].Cells["ProductPrice"].Value != null)
                    {
                        itemprice = Convert.ToDecimal(DataGridView1.Rows[e.RowIndex].Cells["ProductPrice"].Value.ToString());
                    }
                    if (DataGridView1.Rows[e.RowIndex].Cells["ProductQuantity"].Value != null)
                    {
                        itemqty = Convert.ToDecimal(DataGridView1.Rows[e.RowIndex].Cells["ProductQuantity"].Value.ToString());
                    }
                    DataGridView1.Rows[e.RowIndex].Cells["TotalPrice"].Value = itemprice * itemqty;
                }
                if (e.ColumnIndex == 4)
                {
                    if (DataGridView1.Rows[e.RowIndex].Cells["TotalPrice"].Value != null)
                    {
                        totalprice = Convert.ToDecimal(DataGridView1.Rows[e.RowIndex].Cells["TotalPrice"].Value.ToString());
                    }
                    if (DataGridView1.Rows[e.RowIndex].Cells["DiscountValue"].Value != null)
                    {
                        discount = Convert.ToDecimal(DataGridView1.Rows[e.RowIndex].Cells["DiscountValue"].Value.ToString());
                    }
                    DataGridView1.Rows[e.RowIndex].Cells["TotalPriceAfterDiscount"].Value = totalprice - discount;
                }
                if (e.ColumnIndex == 6)
                {
                    if (DataGridView1.Rows[e.RowIndex].Cells["TotalPriceAfterDiscount"].Value != null)
                    {
                        totalpriceafterdiscount = Convert.ToDecimal(DataGridView1.Rows[e.RowIndex].Cells["TotalPriceAfterDiscount"].Value.ToString());
                    }
                    if (DataGridView1.Rows[e.RowIndex].Cells["VatPercentage"].Value != null)
                    {
                        vatpercentage = Convert.ToDecimal(DataGridView1.Rows[e.RowIndex].Cells["VatPercentage"].Value.ToString());
                    }
                    DataGridView1.Rows[e.RowIndex].Cells["VatValue"].Value = Math.Round(totalpriceafterdiscount * vatpercentage / 100, 2, MidpointRounding.AwayFromZero);
                    DataGridView1.Rows[e.RowIndex].Cells["TotalWithVat"].Value = Math.Round(totalpriceafterdiscount + (totalpriceafterdiscount * vatpercentage / 100), 2, MidpointRounding.AwayFromZero);
                }

                Calculate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString() + "\n\n" + ex.InnerException.ToString());
            }

        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            Calculate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Calculate();
            UBLXML ubl = new UBLXML();
            Invoice inv = new Invoice();

            //
            decimal invoicediscount = 0;
            Decimal.TryParse(TextBox2.Text, out invoicediscount);
            if (invoicediscount > 0)
            {
                AllowanceCharge allowance = new AllowanceCharge();
                allowance.ChargeIndicator = false;
                //write this lines in case you will make discount as percentage
                allowance.MultiplierFactorNumeric = 0; //dscount percentage like 10
                allowance.BaseAmount = 0; // the amount we will apply percentage on example (MultiplierFactorNumeric=10 ,BaseAmount=1000 then AllowanceAmount will be 100 SAR)

                // in case we will make discount as Amount 
                allowance.Amount = invoicediscount; // 
                allowance.AllowanceChargeReasonCode = ""; //سبب الخصم
                allowance.AllowanceChargeReason = "discount"; //سبب الخصم
                allowance.taxCategory.ID = "S";// كود الضريبة
                allowance.taxCategory.Percent = 15;// نسبة الضريبة
                //فى حالة عندى اكثر من خصم بعمل loop على الاسطر السابقة
                inv.allowanceCharges.Add(allowance);
            }
            foreach (InvoiceItems item in invlines)
            {
                InvoiceLine invline = new InvoiceLine();
                invline.InvoiceQuantity = item.ProductQuantity;
                invline.item.Name = item.ProductName;
                if (item.VatPercentage == 0)
                {
                    invline.item.classifiedTaxCategory.ID = "Z"; // كود الضريبة
                    invline.taxTotal.TaxSubtotal.taxCategory.ID = "Z"; // كود الضريبة
                    invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReasonCode = "VATEX-SA-HEA"; // كود الضريبة
                    invline.taxTotal.TaxSubtotal.taxCategory.TaxExemptionReason = "Private healthcare to citizen"; // كود الضريبة
                }
                else
                {
                    invline.item.classifiedTaxCategory.ID = "S"; // كود الضريبة
                    invline.taxTotal.TaxSubtotal.taxCategory.ID = "S"; // كود الضريبة
                }
                invline.item.classifiedTaxCategory.Percent = item.VatPercentage; // نسبة الضريبة
                invline.taxTotal.TaxSubtotal.taxCategory.Percent = item.VatPercentage; // نسبة الضريبة


                invline.price.PriceAmount = item.ProductPrice;

                if (item.DiscountValue > 0)
                {
                    AllowanceCharge allowanceCharge = new AllowanceCharge();
                    // فى حالة الرسوم
                    // allowanceCharge.ChargeIndicator = true;
                    // فى حالة الخصم
                    allowanceCharge.ChargeIndicator = false;

                    allowanceCharge.AllowanceChargeReason = "discount"; // سبب الخصم على مستوى المنتج
                    // allowanceCharge.AllowanceChargeReasonCode = "90"; // سبب الخصم على مستوى المنتج
                    allowanceCharge.Amount = item.DiscountValue; // قيم الخصم

                    allowanceCharge.MultiplierFactorNumeric = 0;
                    allowanceCharge.BaseAmount = 0;
                    invline.allowanceCharges.Add(allowanceCharge);
                }
                inv.InvoiceLines.Add(invline);
            }


            InvoiceTotal invoiceTotal = ubl.CalculateInvoiceTotal(inv.InvoiceLines, inv.allowanceCharges);
            TextBox1.Text = invoiceTotal.LineExtensionAmount.ToString("0.00");
            TextBox3.Text = invoiceTotal.TaxExclusiveAmount.ToString("0.00");
            TextBox6.Text = invoiceTotal.TaxInclusiveAmount.ToString("0.00");
            TextBox5.Text = (invoiceTotal.TaxInclusiveAmount - invoiceTotal.TaxExclusiveAmount).ToString("0.00");

        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("payable amount = invoice total amount with vat + rounding amount \r\n example total amount with vat 100.08 and payable amount =101 then rounding amount 0.02 ");
        }
        private void FillSellerOtherIdentification()
        {
            Dictionary<string, string> schemes = new Dictionary<string, string>()
                    {
                        {"رقم السجل التجارى","CRN" },
                        {"رخصة وزارة الشؤون البلدية والقروية والإسكان","MOM" },
                        {"رخصة وزارة الموارد البشرية والتنمية الاجتماعية","MLS" },
                        {"رخصة وزارة الاستثمار","SAG" },
                        {"معرف آخر","OTH" }
                        };

            cmb_seller_scheme.DataSource = new BindingSource(schemes, null);
            cmb_seller_scheme.DisplayMember = "Key";
            cmb_seller_scheme.ValueMember = "Value";
        }
        private void FillBuyerOtherIdentification()
        {
            Dictionary<string, string> schemes = new Dictionary<string, string>()
                    {
                        {"رقم السجل التجارى","CRN" },
                        {"رخصة وزارة الشؤون البلدية والقروية والإسكان","MOM" },
                        {"رخصة وزارة الموارد البشرية والتنمية الاجتماعية","MLS" },
                        {"رخصة وزارة الاستثمار","SAG" },
                        {"معرف آخر","OTH" },
                        {"الرقم المميز","TIN" },
                        {" مكتب العمل700 Number","700" },
                        {"رقم الهوية","NAT" },
                        {"مجلس التعاون الخليجى","GCC" },
                        {"رقم الاقامة","IQA" },
                         {"رقم الباسبور","PAS" },
                        };

            cmb_buyer_scheme.DataSource = new BindingSource(schemes, null);
            cmb_buyer_scheme.DisplayMember = "Key";
            cmb_buyer_scheme.ValueMember = "Value";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        //private void SendXmlToZatca(string xmlpath)
        //{
        //    string CertPem = "CertPem from database";
        //    string secretkey = "SecretKey from database";
        //    string errormessage = "";
        //    string ReportingStatus = "";
        //    string warningmessage = "";
        //    string QRCode = "";
        //    int StatusCode = 0;
        //    Mode sendingmode = Mode.Production;
        //    XmlDocument doc = new XmlDocument();
        //    doc.PreserveWhitespace = true;
        //    try
        //    {
        //        doc.Load(xmlpath);
        //    }
        //    catch
        //    {
        //        errormessage ="Can not load XML file";
        //        return;
        //    }
        //    string EncodedInvoice = Utility.ToBase64Encode(doc.OuterXml);
        //    string UUID = Utility.GetNodeInnerText(doc, SettingsParams.UUID_XPATH);
        //    string InvoiceHash = Utility.GetNodeInnerText(doc, SettingsParams.Hash_XPATH);
        //    string invoicetype = Utility.GetNodeInnerText(doc, SettingsParams.Invoice_Type_XPATH);
        //    QRCode= Utility.GetNodeInnerText(doc, SettingsParams.QR_CODE_XPATH);
        //    // zatca call api
        //    ApiRequestLogic apireqlogic = new ApiRequestLogic(sendingmode, Directory.GetCurrentDirectory(), true);

        //    InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest();
        //    invrequestbody.invoice = EncodedInvoice;
        //    invrequestbody.invoiceHash = InvoiceHash;
        //    invrequestbody.uuid = UUID;
        //   //this code is for simulation and production mode

        //        if (invoicetype.Substring(0, 2) == "01")
        //        {
        //            // to send standard invoices for clearing
        //            //this this the calling of api 
        //            InvoiceClearanceResponse responsemodel = apireqlogic.CallClearanceAPI(Utility.ToBase64Encode(CertPem), secretkey, invrequestbody);
        //        StatusCode = responsemodel.StatusCode;
        //        //if responsemodel.IsSuccess = true this means that your xml is successfully sent to zatca 
        //            if (responsemodel.IsSuccess)
        //            {
        //                ///////////
        //                //if status code =202 it means that xml accepted but with warning 
        //                //no need to sent xml again but you must solve that warning messages for the next invoices
        //                if (responsemodel.StatusCode == 202)
        //                {
        //                //save warning message in database to solve for next invoices
        //                warningmessage = responsemodel.WarningMessage;
        //                }

        //            ReportingStatus = responsemodel.ClearanceStatus;
        //            QRCode = responsemodel.QRCode;

        //            }
        //            else
        //            {
        //                errormessage=responsemodel.ErrorMessage;
        //            return;
        //            }
        //        }
        //        else
        //        {
        //            //to send simplified invoices for reporting
        //            //this this the calling of api 
        //        InvoiceReportingResponse responsemodel = apireqlogic.CallReportingAPI(Utility.ToBase64Encode(CertPem), secretkey, invrequestbody);
        //        StatusCode = responsemodel.StatusCode;
        //        if (responsemodel.IsSuccess)
        //        {
        //            //if status code =202 it means that xml accespted but with warning 
        //            //no need to sent xml again but you must solve that warning messages for the next invoices
        //            if (responsemodel.StatusCode == 202)
        //            {
        //                //save warning message in database to solve for next invoices
        //                warningmessage = responsemodel.WarningMessage;
        //            }
        //            ReportingStatus=responsemodel.ReportingStatus;// Reported
        //        }
        //        else
        //        {
        //            errormessage = responsemodel.ErrorMessage;
        //            return;
        //        }

        //        }

        //    //save all this values
        //    // ReportingStatus 
        //    // warningmessage 
        //    // QRCode
        //    // StatusCode

        //}
    }
}

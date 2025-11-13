using ZatcaIntegrationSDK.APIHelper;
using ZatcaIntegrationSDK.BLL;
using ZatcaIntegrationSDK.HelperContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZatcaIntegrationSDK;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Utilities.IO.Pem;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1.Crmf;

namespace SimpleInvoice
{
    public partial class RenewalCSID : Form
    {
        private Mode mode { get; set; }
        public RenewalCSID()
        {
            InitializeComponent();
        }
        private CertificateRenewalRequest GetRenewalRequest()
        {
            CertificateRenewalRequest certrequest = new CertificateRenewalRequest();
            certrequest.OTP = txt_otp.Text;
            certrequest.PrivateKey = txt_privatekey.Text;
            certrequest.OldCSR = txt_oldcsr.Text;
            certrequest.OldPublicKey = txt_oldpublickey.Text;
            certrequest.OldSecret = txt_oldsecret.Text;
            certrequest.InvoiceType = CSIDGenerator.GetInvoiceType(txt_oldcsr.Text.Trim());
            return certrequest;
        }
        private void btn_csid_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_otp.Text))
            {
                MessageBox.Show("You must enter OTP !");
                return;
            }
            if (string.IsNullOrEmpty(txt_oldcsr.Text))
            {
                MessageBox.Show("You must enter old csr first !");
                return;
            }
            if (string.IsNullOrEmpty(txt_oldpublickey.Text))
            {
                MessageBox.Show("You must enter old CSID first !");
                return;
            }
            if (string.IsNullOrEmpty(txt_oldsecret.Text))
            {
                MessageBox.Show("You must enter old SecretKey first !");
                return;
            }

            
                Invoice inv = new Invoice();

                inv.ID = "INV00001"; // مثال SME00010

                inv.IssueDate = DateTime.Now.ToString("yyyy-MM-dd");
                inv.IssueTime = DateTime.Now.ToString("HH:mm:ss"); // "09:32:40"
                inv.delivery.ActualDeliveryDate = DateTime.Now.ToString("yyyy-MM-dd");
                inv.delivery.LatestDeliveryDate = DateTime.Now.ToString("yyyy-MM-dd");

            inv.DocumentCurrencyCode = "SAR";
                inv.TaxCurrencyCode = "SAR";
            if (inv.invoiceTypeCode.id == 383 || inv.invoiceTypeCode.id == 381)
            {
                // فى حالة ان اشعار دائن او مدين فقط هانكتب رقم الفاتورة اللى اصدرنا الاشعار ليها
                InvoiceDocumentReference invoiceDocumentReference = new InvoiceDocumentReference();
                invoiceDocumentReference.ID = "Invoice Number: 354; Invoice Issue Date: 2021-02-10"; // اجبارى
                inv.billingReference.invoiceDocumentReferences.Add(invoiceDocumentReference);
            }
                inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";

                inv.AdditionalDocumentReferenceICV.UUID = 123;
                PaymentMeans paymentMeans = new PaymentMeans();
                paymentMeans.PaymentMeansCode = "10";
                paymentMeans.InstructionNote = "Payment Notes";
                inv.paymentmeans.Add(paymentMeans);
                // بيانات البائع 
                inv.SupplierParty.partyIdentification.ID = "2050012095"; //هنا رقم السجل التجارى للشركة
                inv.SupplierParty.partyIdentification.schemeID = "CRN";
                inv.SupplierParty.postalAddress.StreetName = "شارع تجربة"; // اجبارى
                inv.SupplierParty.postalAddress.AdditionalStreetName = "شارع اضافى"; // اختيارى
                inv.SupplierParty.postalAddress.BuildingNumber = "1234"; // اجبارى رقم المبنى
                inv.SupplierParty.postalAddress.PlotIdentification = "9833";
                inv.SupplierParty.postalAddress.CityName = "taif";
                inv.SupplierParty.postalAddress.PostalZone = "12345"; // الرقم البريدي
                inv.SupplierParty.postalAddress.CountrySubentity = "المحافظة"; // اسم المحافظة او المدينة مثال (مكة) اختيارى
                inv.SupplierParty.postalAddress.CitySubdivisionName = "اسم المنطقة"; // اسم المنطقة او الحى 
                inv.SupplierParty.postalAddress.country.IdentificationCode = "SA";
                inv.SupplierParty.partyLegalEntity.RegistrationName = CSIDGenerator.GetOrganizationName(txt_oldcsr.Text.Trim()); // "شركة الصناعات الغذائية المتحده"; // اسم الشركة المسجل فى الهيئة
                inv.SupplierParty.partyTaxScheme.CompanyID = CSIDGenerator.GetOrganizationIdentifier(txt_oldcsr.Text.Trim()); // "300518376300003";  // رقم التسجيل الضريبي

                inv.CustomerParty.partyIdentification.ID = "1234567"; // رقم القومى الخاض بالمشترى
                inv.CustomerParty.partyIdentification.schemeID = "CRN"; // الرقم القومى
                inv.CustomerParty.postalAddress.StreetName = "شارع تجربة"; // اجبارى
                inv.CustomerParty.postalAddress.AdditionalStreetName = "شارع اضافى"; // اختيارى
                inv.CustomerParty.postalAddress.BuildingNumber = "1234"; // اجبارى رقم المبنى
                inv.CustomerParty.postalAddress.PlotIdentification = "9833"; // اختيارى رقم القطعة
                inv.CustomerParty.postalAddress.CityName = "Jeddah"; // اسم المدينة
                inv.CustomerParty.postalAddress.PostalZone = "12345"; // الرقم البريدي
                inv.CustomerParty.postalAddress.CountrySubentity = "Makkah"; // اسم المحافظة او المدينة مثال (مكة) اختيارى
                inv.CustomerParty.postalAddress.CitySubdivisionName = "المحافظة"; // اسم المنطقة او الحى 
                inv.CustomerParty.postalAddress.country.IdentificationCode = "SA";
                inv.CustomerParty.partyLegalEntity.RegistrationName = "اسم شركة المشترى"; // اسم الشركة المسجل فى الهيئة
                inv.CustomerParty.partyTaxScheme.CompanyID = "310424415000003"; // رقم التسجيل الضريبي


                inv.legalMonetaryTotal.PrepaidAmount = 0;
                inv.legalMonetaryTotal.PayableAmount = 0;

                InvoiceLine invline = new InvoiceLine();
                invline.InvoiceQuantity = 1;
                invline.item.Name = "منتج تجربة";
                invline.item.classifiedTaxCategory.ID = "S"; // كود الضريبة
                invline.taxTotal.TaxSubtotal.taxCategory.ID = "S"; // كود الضريبة
                invline.item.classifiedTaxCategory.Percent = 15; // نسبة الضريبة
                invline.taxTotal.TaxSubtotal.taxCategory.Percent = 15; // نسبة الضريبة
                invline.price.PriceAmount = 120;
                inv.InvoiceLines.Add(invline);


                CertificateRenewalRequest certrequest = GetRenewalRequest();

                if (rdb_simulation.Checked)
                    mode = Mode.Simulation;
                else if (rdb_production.Checked)
                    mode = Mode.Production;
                else
                    mode = Mode.developer;
                CSIDGenerator generator = new CSIDGenerator(mode);
                CertificateResponse response = generator.GenerateRenewalCSID(certrequest, inv, Directory.GetCurrentDirectory());
                if (response.IsSuccess)
                {
                    // get all certificate data
                    
                    //txt_privatekey.Text = response.PrivateKey;
                    txt_publickey.Text = response.CSID;
                    txt_secret.Text = response.SecretKey;
                    btn_publickey_save.Visible = true;
                    //btn_info.Visible = true;
                   // btn_privatekey_save.Visible = true;
                    //btn_csr_save.Visible = true;
                    btn_secretkey_save.Visible = true;
                }
                else
                {
                    MessageBox.Show(response.ErrorMessage);
                }

           
            }
        private void btn_publickey_save_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "cert files (*.pem)|*.pem";
                saveFileDialog1.FileName = "cert.pem";
                DialogResult result = saveFileDialog1.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string filename = saveFileDialog1.FileName;
                    string publickey = txt_publickey.Text.Trim();
                    File.WriteAllText(filename, publickey);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
        }

        private void btn_secretkey_save_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "secretkey files (*.txt)|*.txt";
                saveFileDialog1.FileName = "secret.txt";
                DialogResult result = saveFileDialog1.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string filename = saveFileDialog1.FileName;
                    string secretkey = txt_secret.Text;
                    File.WriteAllText(filename, secretkey);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
        }

       
    }
}

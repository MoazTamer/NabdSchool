using ZatcaIntegrationSDK.APIHelper;
using ZatcaIntegrationSDK.HelperContracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZatcaIntegrationSDK;


namespace SimpleInvoice
{
    public partial class AutoGenerateCSR : Form
    {
        private Mode mode { get; set; }
        public AutoGenerateCSR()
        {
            InitializeComponent();
            label12.Text = "Mobile : +201090838734";
            label13.Text = "Copyright ©. All rights reserved. Developed by Amr Sobhy";
            fillcontrols();
        }
        private void fillcontrols()
        {
            //txt_commonName.Text = "TST-886431145-311111111101113";
            txt_commonName.Text = "TST-2050012095-300589284900003";
            txt_organizationName.Text = "TST";
            txt_organizationUnitName.Text = "Riyadh Branch";
            txt_countryName.Text = "SA";
            txt_serialNumber.Text = "1-TST|2-TST|3-" + Guid.NewGuid().ToString();
            txt_organizationIdentifier.Text = "300589284900003";
            txt_location.Text = "Makka";
            txt_industry.Text = "Medical Laboratories";
        }
        private CertificateRequest GetCSRRequest()
        {
            CertificateRequest certrequest = new CertificateRequest();
            certrequest.OTP = txt_otp.Text;
            certrequest.CommonName = txt_commonName.Text;
            certrequest.OrganizationName = txt_organizationName.Text;
            certrequest.OrganizationUnitName = txt_organizationUnitName.Text;
            certrequest.CountryName = txt_countryName.Text;
            certrequest.SerialNumber = txt_serialNumber.Text;
            certrequest.OrganizationIdentifier = txt_organizationIdentifier.Text;
            certrequest.Location = txt_location.Text;
            certrequest.BusinessCategory = txt_industry.Text;
            certrequest.InvoiceType = cmb_invoicetype.SelectedValue.ToString();
            return certrequest;
        }

        private void GenerateCSR_Load(object sender, EventArgs e)
        {
            FillInvoiceTypes();
            btn_csr_save.Visible = false;
            btn_privatekey_save.Visible = false;
            btn_secretkey_save.Visible = false;
            btn_publickey_save.Visible = false;
            btn_info.Visible = false;
        }

       
        private void FillInvoiceTypes()
        {
            Dictionary<string, string> types = new Dictionary<string, string>()
                    {
                        {"Standard &Simplified Invoices ** فاتورة ضريبية & مبسطة ","1100" },
                        {"Standard Invoices Only ** فاتورة ضريبية فقط ","1000" },
                        {"Simplified Invoices Only ** فاتورة مبسطة فقط ","0100" }
                        };

            cmb_invoicetype.DataSource = new BindingSource(types, null);
            cmb_invoicetype.DisplayMember = "Key";
            cmb_invoicetype.ValueMember = "Value";
        }
        private void btn_csr_save_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "csr files (*.csr)|*.csr";
                saveFileDialog1.FileName = "csr.csr";
                DialogResult result = saveFileDialog1.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string filename = saveFileDialog1.FileName;
                    File.WriteAllText(filename, txt_csr.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }

        }

        private void btn_privatekey_save_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "csr files (*.pem)|*.pem";
                saveFileDialog1.FileName = "key.pem";
                DialogResult result = saveFileDialog1.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string filename = saveFileDialog1.FileName;
                    string privatekey = txt_privatekey.Text.Trim().Replace("-----BEGIN EC PRIVATE KEY-----", "").Replace(Environment.NewLine, "").Replace("-----END EC PRIVATE KEY-----", "");
                    File.WriteAllText(filename, privatekey);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }

        }

        private void btn_csid_Click(object sender, EventArgs e)
        {
            Invoice inv = new Invoice();

            inv.ID = "INV00001"; // مثال SME00010

            inv.IssueDate = DateTime.Now.ToString("yyyy-MM-dd");
            inv.IssueTime = DateTime.Now.ToString("HH:mm:ss"); // "09:32:40"

            inv.DocumentCurrencyCode = "SAR";
            inv.TaxCurrencyCode = "SAR";
            
                // فى حالة ان اشعار دائن او مدين فقط هانكتب رقم الفاتورة اللى اصدرنا الاشعار ليها
                InvoiceDocumentReference invoiceDocumentReference = new InvoiceDocumentReference();
                invoiceDocumentReference.ID = "Invoice Number: 354; Invoice Issue Date: 2021-02-10"; // اجبارى
                inv.billingReference.invoiceDocumentReferences.Add(invoiceDocumentReference);
            inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";

            inv.AdditionalDocumentReferenceICV.UUID = 1;
            PaymentMeans paymentMeans = new PaymentMeans();
            paymentMeans.PaymentMeansCode = "10";
            paymentMeans.InstructionNote = "Payment Notes";
            inv.paymentmeans.Add(paymentMeans);
            // بيانات البائع 
            inv.SupplierParty.partyIdentification.ID = txt_seller_otherid.Text.Trim();// 300038065900003; //هنا رقم السجل التجارى للشركة
            inv.SupplierParty.partyIdentification.schemeID = "CRN";
            inv.SupplierParty.postalAddress.StreetName = txt_seller_street.Text.Trim(); // اجبارى
            inv.SupplierParty.postalAddress.AdditionalStreetName = ""; // اختيارى
            inv.SupplierParty.postalAddress.BuildingNumber = txt_seller_buildingnumber.Text.Trim(); // اجبارى رقم المبنى
            inv.SupplierParty.postalAddress.PlotIdentification = "";
            inv.SupplierParty.postalAddress.CityName = txt_seller_citysubdiv.Text.Trim();
            inv.SupplierParty.postalAddress.PostalZone = txt_seller_postalzone.Text.Trim(); // الرقم البريدي
            inv.SupplierParty.postalAddress.CountrySubentity = ""; // اسم المحافظة او المدينة مثال (مكة) اختيارى
            inv.SupplierParty.postalAddress.CitySubdivisionName = txt_seller_cityname.Text.Trim(); // اسم المنطقة او الحى 
            inv.SupplierParty.postalAddress.country.IdentificationCode = "SA";
            inv.SupplierParty.partyLegalEntity.RegistrationName = txt_organizationName.Text.Trim(); // "شركة الصناعات الغذائية المتحده"; // اسم الشركة المسجل فى الهيئة
            inv.SupplierParty.partyTaxScheme.CompanyID = txt_organizationIdentifier.Text.Trim();// "300518376300003";  // رقم التسجيل الضريبي

            inv.CustomerParty.partyIdentification.ID = txt_buyyer_otherid.Text.Trim(); // رقم القومى الخاض بالمشترى
            inv.CustomerParty.partyIdentification.schemeID = "CRN"; // الرقم القومى
            inv.CustomerParty.postalAddress.StreetName = txt_buyyer_street.Text.Trim(); // اجبارى
            inv.CustomerParty.postalAddress.AdditionalStreetName = ""; // اختيارى
            inv.CustomerParty.postalAddress.BuildingNumber = txt_buyyer_buildingnumber.Text.Trim(); // اجبارى رقم المبنى
            inv.CustomerParty.postalAddress.PlotIdentification = ""; // اختيارى رقم القطعة
            inv.CustomerParty.postalAddress.CityName = txt_buyyer_cityname.Text.Trim(); // اسم المدينة
            inv.CustomerParty.postalAddress.PostalZone = txt_buyyer_postalzone.Text.Trim(); // الرقم البريدي
            inv.CustomerParty.postalAddress.CountrySubentity = ""; // اسم المحافظة او المدينة مثال (مكة) اختيارى
            inv.CustomerParty.postalAddress.CitySubdivisionName = txt_buyyer_citysubdiv.Text.Trim(); // اسم المنطقة او الحى 
            inv.CustomerParty.postalAddress.country.IdentificationCode = "SA";
            inv.CustomerParty.partyLegalEntity.RegistrationName = txt_buyyer_orgnizationname.Text.Trim(); // اسم الشركة المسجل فى الهيئة
            inv.CustomerParty.partyTaxScheme.CompanyID = txt_buyyer_VatNumber.Text.Trim(); // رقم التسجيل الضريبي


            inv.legalMonetaryTotal.PrepaidAmount = 0;
            inv.legalMonetaryTotal.PayableAmount = 0;

            InvoiceLine invline = new InvoiceLine();
            invline.InvoiceQuantity = 1;
            invline.item.Name = "منتج تجربة";
            invline.item.classifiedTaxCategory.ID = "S"; // كود الضريبة
            invline.taxTotal.TaxSubtotal.taxCategory.ID = "S"; // كود الضريبة
            invline.item.classifiedTaxCategory.Percent = 15; // نسبة الضريبة
            invline.taxTotal.TaxSubtotal.taxCategory.Percent = 15; // نسبة الضريبة
            invline.price.PriceAmount = 1;
            inv.InvoiceLines.Add(invline);


            CertificateRequest certrequest = GetCSRRequest();

            if (rdb_simulation.Checked)
                mode = Mode.Simulation;
            else if (rdb_production.Checked)
                mode = Mode.Production;
            else
                mode = Mode.developer;
            CSIDGenerator generator = new CSIDGenerator(mode);
            CertificateResponse response = generator.GenerateCSID(certrequest, inv, Directory.GetCurrentDirectory());
            if (response.IsSuccess)
            {
                //save to db zatcaCSID table
                // get all certificate data
                txt_csr.Text = response.CSR;
                txt_privatekey.Text = response.PrivateKey;
                txt_publickey.Text = response.CSID;
                txt_secret.Text = response.SecretKey;
                btn_publickey_save.Visible = true;
                btn_info.Visible = true;
                btn_privatekey_save.Visible = true;
                btn_csr_save.Visible = true;
                btn_secretkey_save.Visible = true;
            }
            else
            {
                MessageBox.Show(response.ErrorMessage);
            }
        }

        private void btn_refresh1_Click(object sender, EventArgs e)
        {
            RefreshSerialNumber();
        }


        private void btn_refresh2_Click(object sender, EventArgs e)
        {
            RefreshSerialNumber();
        }
        private void RefreshSerialNumber()
        {
            string input = txt_serialNumber.Text.Trim();
            int index = input.LastIndexOf("|3-");
            if (index >= 0)
            {
                input = input.Substring(0, index + 3);
                txt_serialNumber.Text = input + Guid.NewGuid().ToString();
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

        private void btn_info_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txt_publickey.Text))
            {
                try
                {
                    sbyte[] certificateBytes = (from x in Encoding.UTF8.GetBytes(txt_publickey.Text)
                                                select (sbyte)x).ToArray();
                    System.Security.Cryptography.X509Certificates.X509Certificate2 cert = new System.Security.Cryptography.X509Certificates.X509Certificate2((byte[])(object)certificateBytes);
                    MessageBox.Show(GetCertificateInfo(cert));
                }
                catch
                {


                }
            }

        }
        private string GetCertificateInfo(System.Security.Cryptography.X509Certificates.X509Certificate2 cert)
        {
            string info = "";
            //12/24/2023 3:24:15 PM

            DateTime dt = cert.NotAfter;
            DateTime dt1 = cert.NotBefore;
            info = "Valid From :" + cert.GetEffectiveDateString() + "\n";
            info += "Valid To :" + cert.GetExpirationDateString() + "\n";
            return info;
        }

       
    }
}

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
using System.Xml.Serialization;
using ZatcaIntegrationSDK;

namespace SimpleInvoice
{
    public partial class CSIDInfoForm : Form
    {
        public CSIDInfoForm()
        {
            InitializeComponent();
        }
        private void CSIDInfoForm_Load(object sender, EventArgs e)
        {
            CSIDInfo info = MainSettings.GetCSIDInfo();
            csid_txt.Text = info.PublicKey;
            private_txt.Text = info.PrivateKey;
            secret_txt.Text = info.SecretKey;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                    if (string.IsNullOrEmpty(csid_txt.Text) || string.IsNullOrEmpty(private_txt.Text))
                    {
                        MessageBox.Show("Please Enter All Data ", "SimpleInvoice Program ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        CSIDInfo setting = getCSIDData();
                        if (!SaveCSIDData(setting))
                        {
                            MessageBox.Show("Error While saving csid info", "SimpleInvoice Program ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("CSID info Saved Successfully", "SimpleInvoice Program ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
               
            }
            catch 
            {
                MessageBox.Show("Error While saving csid info", "SimpleInvoice Program ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private CSIDInfo getCSIDData()
        {
            CSIDInfo info = new CSIDInfo();
            info.PrivateKey = private_txt.Text.Trim();
            info.PublicKey = csid_txt.Text.Trim();
            info.SecretKey = secret_txt.Text.Trim();
            return info;
        }
        private bool SaveCSIDData(CSIDInfo info)
        {
            try
            {
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\cert\\"))
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\cert\\");
                File.WriteAllText(Directory.GetCurrentDirectory() + "\\cert\\cert.pem", info.PublicKey.Trim());
                File.WriteAllText(Directory.GetCurrentDirectory() + "\\cert\\key.pem", info.PrivateKey.Trim());
                File.WriteAllText(Directory.GetCurrentDirectory() + "\\cert\\secret.txt", info.SecretKey.Trim());
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleInvoice
{
    public partial class Introfrm : Form
    {
        public Introfrm()
        {
            InitializeComponent();
            label12.Text = "Mobile : +201090838734";
            label13.Text = "Copyright ©. All rights reserved. Developed by Amr Sobhy";
            label3.Text = @"This Application Is a Trail Version To Test Our ZatcaIntegrationSDK 
            (E-invoice Integration With ZATCA Phase two)
            1- Generate CSID to Generate Keys for signing and sending to zatca
            (Simulation valid for 2 years - Production valid for 5 years ).
            2- Renew CSID Before Expire date 
            3- Simple Invoice Form for explain what the values we need for integration.
            4-Generate PDF-A3 File .";

        }

        private void invoiceFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 childForm = new Form1();
            childForm.Show();
        }

        private void toolStripMenuItem0_Click(object sender, EventArgs e)
        {
            AutoGenerateCSR childForm = new AutoGenerateCSR();
            childForm.Show();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            RenewalCSID childForm = new RenewalCSID();
            childForm.Show();
        }

        private void generatePDFA3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GeneratePDFA3 childForm = new GeneratePDFA3();
            childForm.Show();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you need to close ?", "Zatca Integration (Trail Version)", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void addCSIDDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CSIDInfoForm childForm = new CSIDInfoForm();
            childForm.Show();
        }
    }
}

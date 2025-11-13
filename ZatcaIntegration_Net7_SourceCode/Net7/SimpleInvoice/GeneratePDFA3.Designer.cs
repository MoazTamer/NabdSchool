namespace SimpleInvoice
{
    partial class GeneratePDFA3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txt_xmlpath = new System.Windows.Forms.TextBox();
            this.btn_browse_xml = new System.Windows.Forms.Button();
            this.txt_pdfpath = new System.Windows.Forms.TextBox();
            this.btn_browse_pdf = new System.Windows.Forms.Button();
            this.btnpdfa3 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // txt_xmlpath
            // 
            this.txt_xmlpath.Location = new System.Drawing.Point(118, 31);
            this.txt_xmlpath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txt_xmlpath.Name = "txt_xmlpath";
            this.txt_xmlpath.ReadOnly = true;
            this.txt_xmlpath.Size = new System.Drawing.Size(780, 22);
            this.txt_xmlpath.TabIndex = 12;
            this.txt_xmlpath.Text = "Upload Cleared Xml File";
            // 
            // btn_browse_xml
            // 
            this.btn_browse_xml.Location = new System.Drawing.Point(12, 31);
            this.btn_browse_xml.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_browse_xml.Name = "btn_browse_xml";
            this.btn_browse_xml.Size = new System.Drawing.Size(99, 28);
            this.btn_browse_xml.TabIndex = 11;
            this.btn_browse_xml.Text = "Browse xml";
            this.btn_browse_xml.UseVisualStyleBackColor = true;
            this.btn_browse_xml.Click += new System.EventHandler(this.btn_browse_xml_Click);
            // 
            // txt_pdfpath
            // 
            this.txt_pdfpath.Location = new System.Drawing.Point(118, 83);
            this.txt_pdfpath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txt_pdfpath.Name = "txt_pdfpath";
            this.txt_pdfpath.ReadOnly = true;
            this.txt_pdfpath.Size = new System.Drawing.Size(780, 22);
            this.txt_pdfpath.TabIndex = 14;
            this.txt_pdfpath.Text = "Upload Normal PDF for Invoice";
            // 
            // btn_browse_pdf
            // 
            this.btn_browse_pdf.Location = new System.Drawing.Point(12, 83);
            this.btn_browse_pdf.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_browse_pdf.Name = "btn_browse_pdf";
            this.btn_browse_pdf.Size = new System.Drawing.Size(99, 28);
            this.btn_browse_pdf.TabIndex = 13;
            this.btn_browse_pdf.Text = "Browse pdf";
            this.btn_browse_pdf.UseVisualStyleBackColor = true;
            this.btn_browse_pdf.Click += new System.EventHandler(this.btn_browse_pdf_Click);
            // 
            // btnpdfa3
            // 
            this.btnpdfa3.Location = new System.Drawing.Point(331, 132);
            this.btnpdfa3.Name = "btnpdfa3";
            this.btnpdfa3.Size = new System.Drawing.Size(203, 42);
            this.btnpdfa3.TabIndex = 15;
            this.btnpdfa3.Text = "Generate PDF-A3";
            this.btnpdfa3.UseVisualStyleBackColor = true;
            this.btnpdfa3.Click += new System.EventHandler(this.btnpdfa3_Click);
            // 
            // GeneratePDFA3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(917, 200);
            this.Controls.Add(this.btnpdfa3);
            this.Controls.Add(this.txt_pdfpath);
            this.Controls.Add(this.btn_browse_pdf);
            this.Controls.Add(this.txt_xmlpath);
            this.Controls.Add(this.btn_browse_xml);
            this.Name = "GeneratePDFA3";
            this.Text = "Generate PDF-A3";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_xmlpath;
        private System.Windows.Forms.Button btn_browse_xml;
        private System.Windows.Forms.TextBox txt_pdfpath;
        private System.Windows.Forms.Button btn_browse_pdf;
        private System.Windows.Forms.Button btnpdfa3;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}
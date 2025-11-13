
namespace SimpleInvoice
{
    partial class CSIDInfoForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.csid_txt = new System.Windows.Forms.TextBox();
            this.private_txt = new System.Windows.Forms.TextBox();
            this.secret_txt = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "CSID :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 216);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Private Key :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 368);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Secret Key :";
            // 
            // csid_txt
            // 
            this.csid_txt.Location = new System.Drawing.Point(122, 27);
            this.csid_txt.Multiline = true;
            this.csid_txt.Name = "csid_txt";
            this.csid_txt.Size = new System.Drawing.Size(666, 124);
            this.csid_txt.TabIndex = 3;
            // 
            // private_txt
            // 
            this.private_txt.Location = new System.Drawing.Point(122, 167);
            this.private_txt.Multiline = true;
            this.private_txt.Name = "private_txt";
            this.private_txt.Size = new System.Drawing.Size(666, 124);
            this.private_txt.TabIndex = 4;
            // 
            // secret_txt
            // 
            this.secret_txt.Location = new System.Drawing.Point(122, 314);
            this.secret_txt.Multiline = true;
            this.secret_txt.Name = "secret_txt";
            this.secret_txt.Size = new System.Drawing.Size(666, 124);
            this.secret_txt.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(373, 445);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 31);
            this.button1.TabIndex = 6;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // CSIDInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 488);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.secret_txt);
            this.Controls.Add(this.private_txt);
            this.Controls.Add(this.csid_txt);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "CSIDInfoForm";
            this.Text = "CSIDInfoForm";
            this.Load += new System.EventHandler(this.CSIDInfoForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox csid_txt;
        private System.Windows.Forms.TextBox private_txt;
        private System.Windows.Forms.TextBox secret_txt;
        private System.Windows.Forms.Button button1;
    }
}
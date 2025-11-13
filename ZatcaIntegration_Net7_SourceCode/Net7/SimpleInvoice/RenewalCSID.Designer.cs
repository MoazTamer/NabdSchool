namespace SimpleInvoice
{
    partial class RenewalCSID
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
            this.btn_publickey_save = new System.Windows.Forms.Button();
            this.label25 = new System.Windows.Forms.Label();
            this.txt_publickey = new System.Windows.Forms.TextBox();
            this.btn_secretkey_save = new System.Windows.Forms.Button();
            this.rdb_simulation = new System.Windows.Forms.RadioButton();
            this.label16 = new System.Windows.Forms.Label();
            this.txt_secret = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.txt_otp = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.btn_csid = new System.Windows.Forms.Button();
            this.rdb_production = new System.Windows.Forms.RadioButton();
            this.rdb_compliance = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_oldcsr = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_oldpublickey = new System.Windows.Forms.TextBox();
            this.txt_oldsecret = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.label6 = new System.Windows.Forms.Label();
            this.txt_privatekey = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btn_publickey_save
            // 
            this.btn_publickey_save.Location = new System.Drawing.Point(637, 533);
            this.btn_publickey_save.Name = "btn_publickey_save";
            this.btn_publickey_save.Size = new System.Drawing.Size(120, 27);
            this.btn_publickey_save.TabIndex = 120;
            this.btn_publickey_save.Text = "Save حفظ";
            this.btn_publickey_save.UseVisualStyleBackColor = true;
            this.btn_publickey_save.Visible = false;
            this.btn_publickey_save.Click += new System.EventHandler(this.btn_publickey_save_Click);
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(17, 475);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(141, 17);
            this.label25.TabIndex = 119;
            this.label25.Text = "PublicKey المفتاح العام";
            // 
            // txt_publickey
            // 
            this.txt_publickey.Location = new System.Drawing.Point(9, 499);
            this.txt_publickey.Multiline = true;
            this.txt_publickey.Name = "txt_publickey";
            this.txt_publickey.Size = new System.Drawing.Size(625, 87);
            this.txt_publickey.TabIndex = 118;
            // 
            // btn_secretkey_save
            // 
            this.btn_secretkey_save.Location = new System.Drawing.Point(644, 613);
            this.btn_secretkey_save.Name = "btn_secretkey_save";
            this.btn_secretkey_save.Size = new System.Drawing.Size(120, 27);
            this.btn_secretkey_save.TabIndex = 117;
            this.btn_secretkey_save.Text = "Save حفظ";
            this.btn_secretkey_save.UseVisualStyleBackColor = true;
            this.btn_secretkey_save.Visible = false;
            this.btn_secretkey_save.Click += new System.EventHandler(this.btn_secretkey_save_Click);
            // 
            // rdb_simulation
            // 
            this.rdb_simulation.AutoSize = true;
            this.rdb_simulation.Location = new System.Drawing.Point(9, 10);
            this.rdb_simulation.Name = "rdb_simulation";
            this.rdb_simulation.Size = new System.Drawing.Size(140, 21);
            this.rdb_simulation.TabIndex = 113;
            this.rdb_simulation.Text = "Simulation(محاكاة)";
            this.rdb_simulation.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(655, 12);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(125, 17);
            this.label16.TabIndex = 104;
            this.label16.Text = "الرمز المتغير(6 ارقام)";
            // 
            // txt_secret
            // 
            this.txt_secret.Location = new System.Drawing.Point(12, 613);
            this.txt_secret.Name = "txt_secret";
            this.txt_secret.ReadOnly = true;
            this.txt_secret.Size = new System.Drawing.Size(622, 24);
            this.txt_secret.TabIndex = 102;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(11, 592);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(124, 17);
            this.label14.TabIndex = 101;
            this.label14.Text = "Secret الرقم السرى";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(634, 801);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(0, 17);
            this.label12.TabIndex = 100;
            // 
            // txt_otp
            // 
            this.txt_otp.Location = new System.Drawing.Point(490, 9);
            this.txt_otp.Name = "txt_otp";
            this.txt_otp.Size = new System.Drawing.Size(156, 24);
            this.txt_otp.TabIndex = 99;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(446, 10);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(34, 17);
            this.label11.TabIndex = 98;
            this.label11.Text = "OTP";
            // 
            // btn_csid
            // 
            this.btn_csid.Location = new System.Drawing.Point(260, 439);
            this.btn_csid.Name = "btn_csid";
            this.btn_csid.Size = new System.Drawing.Size(239, 31);
            this.btn_csid.TabIndex = 97;
            this.btn_csid.Text = "Renew CSID تجديد المفتاح العام";
            this.btn_csid.UseVisualStyleBackColor = true;
            this.btn_csid.Click += new System.EventHandler(this.btn_csid_Click);
            // 
            // rdb_production
            // 
            this.rdb_production.AutoSize = true;
            this.rdb_production.Location = new System.Drawing.Point(308, 10);
            this.rdb_production.Name = "rdb_production";
            this.rdb_production.Size = new System.Drawing.Size(137, 21);
            this.rdb_production.TabIndex = 90;
            this.rdb_production.Text = "Production(فعلية)";
            this.rdb_production.UseVisualStyleBackColor = true;
            // 
            // rdb_compliance
            // 
            this.rdb_compliance.AutoSize = true;
            this.rdb_compliance.Checked = true;
            this.rdb_compliance.Location = new System.Drawing.Point(153, 10);
            this.rdb_compliance.Name = "rdb_compliance";
            this.rdb_compliance.Size = new System.Drawing.Size(151, 21);
            this.rdb_compliance.TabIndex = 89;
            this.rdb_compliance.TabStop = true;
            this.rdb_compliance.Text = "Compliance(تجريبية)";
            this.rdb_compliance.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(174, 17);
            this.label2.TabIndex = 122;
            this.label2.Text = "Old CSR طلب توقيع الشهادة";
            // 
            // txt_oldcsr
            // 
            this.txt_oldcsr.Location = new System.Drawing.Point(14, 73);
            this.txt_oldcsr.Multiline = true;
            this.txt_oldcsr.Name = "txt_oldcsr";
            this.txt_oldcsr.Size = new System.Drawing.Size(766, 87);
            this.txt_oldcsr.TabIndex = 121;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 262);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(165, 17);
            this.label3.TabIndex = 124;
            this.label3.Text = "Old PublicKey المفتاح العام";
            // 
            // txt_oldpublickey
            // 
            this.txt_oldpublickey.Location = new System.Drawing.Point(14, 282);
            this.txt_oldpublickey.Multiline = true;
            this.txt_oldpublickey.Name = "txt_oldpublickey";
            this.txt_oldpublickey.Size = new System.Drawing.Size(766, 87);
            this.txt_oldpublickey.TabIndex = 123;
            // 
            // txt_oldsecret
            // 
            this.txt_oldsecret.Location = new System.Drawing.Point(14, 396);
            this.txt_oldsecret.Name = "txt_oldsecret";
            this.txt_oldsecret.Size = new System.Drawing.Size(766, 24);
            this.txt_oldsecret.TabIndex = 126;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 376);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(148, 17);
            this.label4.TabIndex = 125;
            this.label4.Text = "Old Secret الرقم السرى";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 167);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(183, 17);
            this.label6.TabIndex = 134;
            this.label6.Text = "Old PrivateKey المفتاح الخاص";
            // 
            // txt_privatekey
            // 
            this.txt_privatekey.Location = new System.Drawing.Point(12, 187);
            this.txt_privatekey.Multiline = true;
            this.txt_privatekey.Name = "txt_privatekey";
            this.txt_privatekey.Size = new System.Drawing.Size(766, 72);
            this.txt_privatekey.TabIndex = 133;
            // 
            // RenewalCSID
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(787, 688);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txt_privatekey);
            this.Controls.Add(this.txt_oldsecret);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_oldpublickey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_oldcsr);
            this.Controls.Add(this.btn_publickey_save);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.txt_publickey);
            this.Controls.Add(this.btn_secretkey_save);
            this.Controls.Add(this.rdb_simulation);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.txt_secret);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txt_otp);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.btn_csid);
            this.Controls.Add(this.rdb_production);
            this.Controls.Add(this.rdb_compliance);
            this.Name = "RenewalCSID";
            this.Text = "RenewalCSID";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_publickey_save;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox txt_publickey;
        private System.Windows.Forms.Button btn_secretkey_save;
        private System.Windows.Forms.RadioButton rdb_simulation;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txt_secret;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txt_otp;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btn_csid;
        private System.Windows.Forms.RadioButton rdb_production;
        private System.Windows.Forms.RadioButton rdb_compliance;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_oldcsr;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_oldpublickey;
        private System.Windows.Forms.TextBox txt_oldsecret;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txt_privatekey;
    }
}
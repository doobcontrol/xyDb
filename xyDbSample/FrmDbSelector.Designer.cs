namespace xyDbSample
{
    partial class FrmDbSelector
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
            comboBox1 = new ComboBox();
            button1 = new Button();
            checkBox1 = new CheckBox();
            panel1 = new Panel();
            panelTottom = new Panel();
            panelTop = new Panel();
            panelAdminConnPar = new Panel();
            lbdbServer = new Label();
            lbdbName = new Label();
            lbdbUser = new Label();
            lbdbPassword = new Label();
            txtdbServer = new TextBox();
            txtdbName = new TextBox();
            txtdbUser = new TextBox();
            txtdbPassword = new TextBox();
            panel1.SuspendLayout();
            panelTottom.SuspendLayout();
            panelTop.SuspendLayout();
            panelAdminConnPar.SuspendLayout();
            SuspendLayout();
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(12, 12);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(426, 28);
            comboBox1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(3, 3);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 1;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(12, 46);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(101, 24);
            checkBox1.TabIndex = 2;
            checkBox1.Text = "checkBox1";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Right;
            panel1.Location = new Point(347, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(103, 39);
            panel1.TabIndex = 3;
            // 
            // panelTottom
            // 
            panelTottom.Controls.Add(panel1);
            panelTottom.Dock = DockStyle.Bottom;
            panelTottom.Location = new Point(0, 301);
            panelTottom.Name = "panelTottom";
            panelTottom.Size = new Size(450, 39);
            panelTottom.TabIndex = 4;
            // 
            // panelTop
            // 
            panelTop.Controls.Add(comboBox1);
            panelTop.Controls.Add(checkBox1);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(450, 76);
            panelTop.TabIndex = 5;
            // 
            // panelAdminConnPar
            // 
            panelAdminConnPar.Controls.Add(txtdbPassword);
            panelAdminConnPar.Controls.Add(txtdbUser);
            panelAdminConnPar.Controls.Add(txtdbName);
            panelAdminConnPar.Controls.Add(txtdbServer);
            panelAdminConnPar.Controls.Add(lbdbPassword);
            panelAdminConnPar.Controls.Add(lbdbUser);
            panelAdminConnPar.Controls.Add(lbdbName);
            panelAdminConnPar.Controls.Add(lbdbServer);
            panelAdminConnPar.Location = new Point(12, 82);
            panelAdminConnPar.Name = "panelAdminConnPar";
            panelAdminConnPar.Size = new Size(408, 153);
            panelAdminConnPar.TabIndex = 6;
            // 
            // lbdbServer
            // 
            lbdbServer.AutoSize = true;
            lbdbServer.Location = new Point(15, 20);
            lbdbServer.Name = "lbdbServer";
            lbdbServer.Size = new Size(50, 20);
            lbdbServer.TabIndex = 7;
            lbdbServer.Text = "label1";
            // 
            // lbdbName
            // 
            lbdbName.AutoSize = true;
            lbdbName.Location = new Point(15, 49);
            lbdbName.Name = "lbdbName";
            lbdbName.Size = new Size(50, 20);
            lbdbName.TabIndex = 8;
            lbdbName.Text = "label2";
            // 
            // lbdbUser
            // 
            lbdbUser.AutoSize = true;
            lbdbUser.Location = new Point(15, 86);
            lbdbUser.Name = "lbdbUser";
            lbdbUser.Size = new Size(50, 20);
            lbdbUser.TabIndex = 9;
            lbdbUser.Text = "label3";
            // 
            // lbdbPassword
            // 
            lbdbPassword.AutoSize = true;
            lbdbPassword.Location = new Point(15, 115);
            lbdbPassword.Name = "lbdbPassword";
            lbdbPassword.Size = new Size(50, 20);
            lbdbPassword.TabIndex = 10;
            lbdbPassword.Text = "label4";
            // 
            // txtdbServer
            // 
            txtdbServer.Location = new Point(163, 17);
            txtdbServer.Name = "txtdbServer";
            txtdbServer.Size = new Size(233, 27);
            txtdbServer.TabIndex = 11;
            // 
            // txtdbName
            // 
            txtdbName.Location = new Point(163, 50);
            txtdbName.Name = "txtdbName";
            txtdbName.Size = new Size(233, 27);
            txtdbName.TabIndex = 12;
            // 
            // txtdbUser
            // 
            txtdbUser.Location = new Point(163, 83);
            txtdbUser.Name = "txtdbUser";
            txtdbUser.Size = new Size(233, 27);
            txtdbUser.TabIndex = 13;
            // 
            // txtdbPassword
            // 
            txtdbPassword.Location = new Point(163, 116);
            txtdbPassword.Name = "txtdbPassword";
            txtdbPassword.PasswordChar = '*';
            txtdbPassword.Size = new Size(233, 27);
            txtdbPassword.TabIndex = 14;
            // 
            // FrmDbSelector
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(450, 340);
            Controls.Add(panelAdminConnPar);
            Controls.Add(panelTop);
            Controls.Add(panelTottom);
            Name = "FrmDbSelector";
            Text = "FrmDbSelector";
            panel1.ResumeLayout(false);
            panelTottom.ResumeLayout(false);
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelAdminConnPar.ResumeLayout(false);
            panelAdminConnPar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ComboBox comboBox1;
        private Button button1;
        private CheckBox checkBox1;
        private Panel panel1;
        private Panel panelTottom;
        private Panel panelTop;
        private Panel panelAdminConnPar;
        private Label lbdbServer;
        private TextBox txtdbPassword;
        private TextBox txtdbUser;
        private TextBox txtdbName;
        private TextBox txtdbServer;
        private Label lbdbPassword;
        private Label lbdbUser;
        private Label lbdbName;
    }
}
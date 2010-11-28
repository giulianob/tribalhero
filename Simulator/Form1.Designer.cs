namespace Simulator {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tabTroop1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.troopPanel1 = new Simulator.TroopPanel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.troopPanel2 = new Simulator.TroopPanel();
            this.btnSimulate = new System.Windows.Forms.Button();
            this.btnServer = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.troopView1 = new Simulator.TroopView();
            this.tabTroop1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabTroop1
            // 
            this.tabTroop1.Controls.Add(this.tabPage1);
            this.tabTroop1.Controls.Add(this.tabPage2);
            this.tabTroop1.Location = new System.Drawing.Point(12, 269);
            this.tabTroop1.Name = "tabTroop1";
            this.tabTroop1.SelectedIndex = 0;
            this.tabTroop1.Size = new System.Drawing.Size(927, 465);
            this.tabTroop1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.troopPanel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(919, 439);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Defender";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // troopPanel1
            // 
            this.troopPanel1.Location = new System.Drawing.Point(3, 3);
            this.troopPanel1.Name = "troopPanel1";
            this.troopPanel1.Resource = ((Game.Data.Resource)(resources.GetObject("troopPanel1.Resource")));
            this.troopPanel1.Size = new System.Drawing.Size(881, 442);
            this.troopPanel1.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.troopPanel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(890, 439);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Attacker";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // troopPanel2
            // 
            this.troopPanel2.Location = new System.Drawing.Point(3, 3);
            this.troopPanel2.Name = "troopPanel2";
            this.troopPanel2.Resource = ((Game.Data.Resource)(resources.GetObject("troopPanel2.Resource")));
            this.troopPanel2.Size = new System.Drawing.Size(890, 653);
            this.troopPanel2.TabIndex = 0;
            // 
            // btnSimulate
            // 
            this.btnSimulate.Location = new System.Drawing.Point(812, 222);
            this.btnSimulate.Name = "btnSimulate";
            this.btnSimulate.Size = new System.Drawing.Size(127, 23);
            this.btnSimulate.TabIndex = 3;
            this.btnSimulate.Text = "Simulate";
            this.btnSimulate.UseVisualStyleBackColor = true;
            this.btnSimulate.Click += new System.EventHandler(this.btnSimulate_Click);
            // 
            // btnServer
            // 
            this.btnServer.Location = new System.Drawing.Point(812, 12);
            this.btnServer.Name = "btnServer";
            this.btnServer.Size = new System.Drawing.Size(127, 23);
            this.btnServer.TabIndex = 4;
            this.btnServer.Text = "Server";
            this.btnServer.UseVisualStyleBackColor = true;
            this.btnServer.Click += new System.EventHandler(this.btnServer_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(812, 41);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(124, 20);
            this.numericUpDown1.TabIndex = 5;
            this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown1.Value = new decimal(new int[] {
            48888,
            0,
            0,
            0});
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(812, 98);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(127, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Connect To";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(813, 128);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(126, 20);
            this.textBox1.TabIndex = 7;
            this.textBox1.Text = "127.0.0.1:48888";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // troopView1
            // 
            this.troopView1.AllowDrop = true;
            this.troopView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.troopView1.Location = new System.Drawing.Point(12, 12);
            this.troopView1.Name = "troopView1";
            this.troopView1.Size = new System.Drawing.Size(796, 232);
            this.troopView1.TabIndex = 0;
            this.troopView1.Load += new System.EventHandler(this.troopView1_Load);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(963, 754);
            this.Controls.Add(this.troopView1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.btnServer);
            this.Controls.Add(this.btnSimulate);
            this.Controls.Add(this.tabTroop1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabTroop1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TroopPanel troopPanel1;
        private System.Windows.Forms.TabControl tabTroop1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private TroopPanel troopPanel2;
        private System.Windows.Forms.Button btnSimulate;
        private System.Windows.Forms.Button btnServer;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private TroopView troopView1;


    }
}


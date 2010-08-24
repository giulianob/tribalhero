namespace Debugger {
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
            this.ServerTextBox = new System.Windows.Forms.TextBox();
            this.ConnectBtn = new System.Windows.Forms.Button();
            this.disconnectBtn = new System.Windows.Forms.Button();
            this.LoginBtn = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listParamView = new System.Windows.Forms.ListView();
            this.ch2type = new System.Windows.Forms.ColumnHeader();
            this.ch2Value = new System.Windows.Forms.ColumnHeader();
            this.Int32Btn = new System.Windows.Forms.Button();
            this.Int16Btn = new System.Windows.Forms.Button();
            this.UInt32Btn = new System.Windows.Forms.Button();
            this.UInt16Btn = new System.Windows.Forms.Button();
            this.StringBtn = new System.Windows.Forms.Button();
            this.byteBtn = new System.Windows.Forms.Button();
            this.ValueTextBox = new System.Windows.Forms.TextBox();
            this.button8 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.CompressChkBox = new System.Windows.Forms.CheckBox();
            this.listPacketView = new System.Windows.Forms.ListView();
            this.chType = new System.Windows.Forms.ColumnHeader();
            this.chTime = new System.Windows.Forms.ColumnHeader();
            this.chSeq = new System.Windows.Forms.ColumnHeader();
            this.chCommand = new System.Windows.Forms.ColumnHeader();
            this.chOption = new System.Windows.Forms.ColumnHeader();
            this.chRawLen = new System.Windows.Forms.ColumnHeader();
            this.chRealLen = new System.Windows.Forms.ColumnHeader();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ServerTextBox
            // 
            this.ServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ServerTextBox.Location = new System.Drawing.Point(598, 29);
            this.ServerTextBox.Name = "ServerTextBox";
            this.ServerTextBox.Size = new System.Drawing.Size(144, 20);
            this.ServerTextBox.TabIndex = 0;
            this.ServerTextBox.Text = "127.0.0.1:48888";
            // 
            // ConnectBtn
            // 
            this.ConnectBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectBtn.Location = new System.Drawing.Point(598, 61);
            this.ConnectBtn.Name = "ConnectBtn";
            this.ConnectBtn.Size = new System.Drawing.Size(144, 25);
            this.ConnectBtn.TabIndex = 1;
            this.ConnectBtn.Text = "Connect";
            this.ConnectBtn.UseVisualStyleBackColor = true;
            this.ConnectBtn.Click += new System.EventHandler(this.ConnectBtn_Click);
            // 
            // disconnectBtn
            // 
            this.disconnectBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.disconnectBtn.Location = new System.Drawing.Point(598, 92);
            this.disconnectBtn.Name = "disconnectBtn";
            this.disconnectBtn.Size = new System.Drawing.Size(144, 25);
            this.disconnectBtn.TabIndex = 1;
            this.disconnectBtn.Text = "Disconnect";
            this.disconnectBtn.UseVisualStyleBackColor = true;
            this.disconnectBtn.Click += new System.EventHandler(this.disconnectBtn_Click);
            // 
            // LoginBtn
            // 
            this.LoginBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LoginBtn.Location = new System.Drawing.Point(598, 127);
            this.LoginBtn.Name = "LoginBtn";
            this.LoginBtn.Size = new System.Drawing.Size(143, 27);
            this.LoginBtn.TabIndex = 2;
            this.LoginBtn.Text = "Login";
            this.LoginBtn.UseVisualStyleBackColor = true;
            this.LoginBtn.Click += new System.EventHandler(this.LoginBtn_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(19, 19);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(157, 21);
            this.comboBox1.TabIndex = 4;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listParamView);
            this.groupBox1.Controls.Add(this.Int32Btn);
            this.groupBox1.Controls.Add(this.Int16Btn);
            this.groupBox1.Controls.Add(this.UInt32Btn);
            this.groupBox1.Controls.Add(this.UInt16Btn);
            this.groupBox1.Controls.Add(this.StringBtn);
            this.groupBox1.Controls.Add(this.byteBtn);
            this.groupBox1.Controls.Add(this.ValueTextBox);
            this.groupBox1.Controls.Add(this.button8);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.CompressChkBox);
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(580, 277);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Packet Builder";
            // 
            // listParamView
            // 
            this.listParamView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ch2type,
            this.ch2Value});
            this.listParamView.FullRowSelect = true;
            this.listParamView.GridLines = true;
            this.listParamView.Location = new System.Drawing.Point(6, 87);
            this.listParamView.Name = "listParamView";
            this.listParamView.Size = new System.Drawing.Size(277, 184);
            this.listParamView.TabIndex = 9;
            this.listParamView.UseCompatibleStateImageBehavior = false;
            this.listParamView.View = System.Windows.Forms.View.Details;
            // 
            // ch2type
            // 
            this.ch2type.Text = "Type";
            this.ch2type.Width = 72;
            // 
            // ch2Value
            // 
            this.ch2Value.Text = "Value";
            this.ch2Value.Width = 200;
            // 
            // Int32Btn
            // 
            this.Int32Btn.Location = new System.Drawing.Point(366, 179);
            this.Int32Btn.Name = "Int32Btn";
            this.Int32Btn.Size = new System.Drawing.Size(75, 24);
            this.Int32Btn.TabIndex = 8;
            this.Int32Btn.Text = "Int32";
            this.Int32Btn.UseVisualStyleBackColor = true;
            this.Int32Btn.Click += new System.EventHandler(this.Int32Btn_Click);
            // 
            // Int16Btn
            // 
            this.Int16Btn.Location = new System.Drawing.Point(366, 150);
            this.Int16Btn.Name = "Int16Btn";
            this.Int16Btn.Size = new System.Drawing.Size(75, 24);
            this.Int16Btn.TabIndex = 8;
            this.Int16Btn.Text = "Int16";
            this.Int16Btn.UseVisualStyleBackColor = true;
            this.Int16Btn.Click += new System.EventHandler(this.Int16Btn_Click);
            // 
            // UInt32Btn
            // 
            this.UInt32Btn.Location = new System.Drawing.Point(289, 179);
            this.UInt32Btn.Name = "UInt32Btn";
            this.UInt32Btn.Size = new System.Drawing.Size(71, 24);
            this.UInt32Btn.TabIndex = 8;
            this.UInt32Btn.Text = "UInt32";
            this.UInt32Btn.UseVisualStyleBackColor = true;
            this.UInt32Btn.Click += new System.EventHandler(this.UInt32Btn_Click);
            // 
            // UInt16Btn
            // 
            this.UInt16Btn.Location = new System.Drawing.Point(289, 149);
            this.UInt16Btn.Name = "UInt16Btn";
            this.UInt16Btn.Size = new System.Drawing.Size(71, 24);
            this.UInt16Btn.TabIndex = 8;
            this.UInt16Btn.Text = "UInt16";
            this.UInt16Btn.UseVisualStyleBackColor = true;
            this.UInt16Btn.Click += new System.EventHandler(this.UInt16Btn_Click);
            // 
            // StringBtn
            // 
            this.StringBtn.Location = new System.Drawing.Point(366, 118);
            this.StringBtn.Name = "StringBtn";
            this.StringBtn.Size = new System.Drawing.Size(75, 24);
            this.StringBtn.TabIndex = 8;
            this.StringBtn.Text = "String";
            this.StringBtn.UseVisualStyleBackColor = true;
            this.StringBtn.Click += new System.EventHandler(this.StringBtn_Click);
            // 
            // byteBtn
            // 
            this.byteBtn.Location = new System.Drawing.Point(289, 118);
            this.byteBtn.Name = "byteBtn";
            this.byteBtn.Size = new System.Drawing.Size(71, 24);
            this.byteBtn.TabIndex = 8;
            this.byteBtn.Text = "Byte";
            this.byteBtn.UseVisualStyleBackColor = true;
            this.byteBtn.Click += new System.EventHandler(this.byteBtn_Click);
            // 
            // ValueTextBox
            // 
            this.ValueTextBox.Location = new System.Drawing.Point(289, 87);
            this.ValueTextBox.Name = "ValueTextBox";
            this.ValueTextBox.Size = new System.Drawing.Size(152, 20);
            this.ValueTextBox.TabIndex = 7;
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(467, 208);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(95, 24);
            this.button8.TabIndex = 6;
            this.button8.Text = "Clear";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(467, 238);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(95, 24);
            this.button1.TabIndex = 6;
            this.button1.Text = "Send";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // CompressChkBox
            // 
            this.CompressChkBox.AutoSize = true;
            this.CompressChkBox.Location = new System.Drawing.Point(21, 51);
            this.CompressChkBox.Name = "CompressChkBox";
            this.CompressChkBox.Size = new System.Drawing.Size(72, 17);
            this.CompressChkBox.TabIndex = 5;
            this.CompressChkBox.Text = "Compress";
            this.CompressChkBox.UseVisualStyleBackColor = true;
            // 
            // listPacketView
            // 
            this.listPacketView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.listPacketView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chType,
            this.chTime,
            this.chSeq,
            this.chCommand,
            this.chOption,
            this.chRawLen,
            this.chRealLen});
            this.listPacketView.FullRowSelect = true;
            this.listPacketView.GridLines = true;
            this.listPacketView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listPacketView.Location = new System.Drawing.Point(14, 301);
            this.listPacketView.Name = "listPacketView";
            this.listPacketView.Size = new System.Drawing.Size(572, 280);
            this.listPacketView.TabIndex = 6;
            this.listPacketView.UseCompatibleStateImageBehavior = false;
            this.listPacketView.View = System.Windows.Forms.View.Details;
            this.listPacketView.DoubleClick += new System.EventHandler(this.listPacketView_DoubleClick);
            this.listPacketView.SelectedIndexChanged += new System.EventHandler(this.listPacketView_SelectedIndexChanged);
            // 
            // chType
            // 
            this.chType.Text = "";
            this.chType.Width = 30;
            // 
            // chTime
            // 
            this.chTime.Text = "Timestamp";
            this.chTime.Width = 150;
            // 
            // chSeq
            // 
            this.chSeq.Text = "Seq #";
            this.chSeq.Width = 50;
            // 
            // chCommand
            // 
            this.chCommand.Text = "Command";
            // 
            // chOption
            // 
            this.chOption.Text = "Option";
            this.chOption.Width = 50;
            // 
            // chRawLen
            // 
            this.chRawLen.Text = "Uncompressed";
            this.chRawLen.Width = 50;
            // 
            // chRealLen
            // 
            this.chRealLen.Text = "Param Len";
            this.chRealLen.Width = 50;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 593);
            this.Controls.Add(this.listPacketView);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.LoginBtn);
            this.Controls.Add(this.disconnectBtn);
            this.Controls.Add(this.ConnectBtn);
            this.Controls.Add(this.ServerTextBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ServerTextBox;
        private System.Windows.Forms.Button ConnectBtn;
        private System.Windows.Forms.Button disconnectBtn;
        private System.Windows.Forms.Button LoginBtn;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox CompressChkBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button byteBtn;
        private System.Windows.Forms.TextBox ValueTextBox;
        private System.Windows.Forms.Button Int32Btn;
        private System.Windows.Forms.Button Int16Btn;
        private System.Windows.Forms.Button UInt32Btn;
        private System.Windows.Forms.Button UInt16Btn;
        private System.Windows.Forms.Button StringBtn;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.ListView listPacketView;
        private System.Windows.Forms.ColumnHeader chType;
        private System.Windows.Forms.ColumnHeader chCommand;
        private System.Windows.Forms.ColumnHeader chOption;
        private System.Windows.Forms.ColumnHeader chRawLen;
        private System.Windows.Forms.ColumnHeader chRealLen;
        private System.Windows.Forms.ColumnHeader chSeq;
        private System.Windows.Forms.ColumnHeader chTime;
        private System.Windows.Forms.ListView listParamView;
        private System.Windows.Forms.ColumnHeader ch2type;
        private System.Windows.Forms.ColumnHeader ch2Value;
    }
}


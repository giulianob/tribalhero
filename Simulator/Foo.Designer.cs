namespace Simulator {
    partial class Foo {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Foo));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "AntiAir.gif");
            this.imageList1.Images.SetKeyName(1, "APC.gif");
            this.imageList1.Images.SetKeyName(2, "Artillery.gif");
            this.imageList1.Images.SetKeyName(3, "Bomber.gif");
            this.imageList1.Images.SetKeyName(4, "Copter.gif");
            this.imageList1.Images.SetKeyName(5, "Fighter.gif");
            this.imageList1.Images.SetKeyName(6, "Infantry.gif");
            this.imageList1.Images.SetKeyName(7, "Mech.gif");
            this.imageList1.Images.SetKeyName(8, "MediumTank.gif");
            this.imageList1.Images.SetKeyName(9, "Missile.gif");
            this.imageList1.Images.SetKeyName(10, "Recon.gif");
            this.imageList1.Images.SetKeyName(11, "Rocket.gif");
            this.imageList1.Images.SetKeyName(12, "Tank.gif");
            this.imageList1.Images.SetKeyName(13, "Transport.gif");
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(540, 354);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.Draw += new System.Windows.Forms.DrawToolTipEventHandler(this.toolTip1_Draw);
            // 
            // troopListView1
            // 
            // 
            // Foo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(822, 698);
            this.Controls.Add(this.button1);
            this.Name = "Foo";
            this.Text = "foo";
            this.Load += new System.EventHandler(this.Foo_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
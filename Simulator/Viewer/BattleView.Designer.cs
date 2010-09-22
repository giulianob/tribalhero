namespace Simulator {
    partial class BattleView {
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblRound = new System.Windows.Forms.Label();
            this.lblTurn = new System.Windows.Forms.Label();
            this.lblStamina = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.troopListView2 = new Simulator.Viewer.TroopListView();
            this.troopListView1 = new Simulator.Viewer.TroopListView();
            this.textViewer1 = new Simulator.Viewer.TextViewer();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(957, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 31);
            this.label1.TabIndex = 2;
            this.label1.Text = "Defender";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(158, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 31);
            this.label2.TabIndex = 2;
            this.label2.Text = "Attacker";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(443, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 24);
            this.label3.TabIndex = 3;
            this.label3.Text = "Round:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(557, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 24);
            this.label4.TabIndex = 3;
            this.label4.Text = "Turn:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(654, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 24);
            this.label5.TabIndex = 3;
            this.label5.Text = "Stamina:";
            // 
            // lblRound
            // 
            this.lblRound.AutoSize = true;
            this.lblRound.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRound.Location = new System.Drawing.Point(526, 13);
            this.lblRound.Name = "lblRound";
            this.lblRound.Size = new System.Drawing.Size(20, 24);
            this.lblRound.TabIndex = 3;
            this.lblRound.Text = "0";
            // 
            // lblTurn
            // 
            this.lblTurn.AutoSize = true;
            this.lblTurn.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTurn.Location = new System.Drawing.Point(623, 13);
            this.lblTurn.Name = "lblTurn";
            this.lblTurn.Size = new System.Drawing.Size(20, 24);
            this.lblTurn.TabIndex = 3;
            this.lblTurn.Text = "0";
            // 
            // lblStamina
            // 
            this.lblStamina.AutoSize = true;
            this.lblStamina.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStamina.Location = new System.Drawing.Point(747, 13);
            this.lblStamina.Name = "lblStamina";
            this.lblStamina.Size = new System.Drawing.Size(20, 24);
            this.lblStamina.TabIndex = 3;
            this.lblStamina.Text = "0";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(394, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // troopListView2
            // 
            this.troopListView2.Location = new System.Drawing.Point(801, 53);
            this.troopListView2.Name = "troopListView2";
            this.troopListView2.Size = new System.Drawing.Size(454, 583);
            this.troopListView2.TabIndex = 1;
            // 
            // troopListView1
            // 
            this.troopListView1.Location = new System.Drawing.Point(-1, 53);
            this.troopListView1.Name = "troopListView1";
            this.troopListView1.Size = new System.Drawing.Size(454, 671);
            this.troopListView1.TabIndex = 1;
            // 
            // textViewer1
            // 
            this.textViewer1.Location = new System.Drawing.Point(460, 53);
            this.textViewer1.Name = "textViewer1";
            this.textViewer1.Size = new System.Drawing.Size(337, 517);
            this.textViewer1.TabIndex = 0;
            this.textViewer1.Load += new System.EventHandler(this.textViewer1_Load);
            // 
            // BattleView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1255, 722);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblStamina);
            this.Controls.Add(this.lblTurn);
            this.Controls.Add(this.lblRound);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.troopListView2);
            this.Controls.Add(this.troopListView1);
            this.Controls.Add(this.textViewer1);
            this.Name = "BattleView";
            this.Text = "BattleView";
            this.Load += new System.EventHandler(this.BattleView_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BattleView_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Simulator.Viewer.TextViewer textViewer1;
        private Simulator.Viewer.TroopListView troopListView1;
        private Simulator.Viewer.TroopListView troopListView2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblRound;
        private System.Windows.Forms.Label lblTurn;
        private System.Windows.Forms.Label lblStamina;
        private System.Windows.Forms.Button button1;
    }
}
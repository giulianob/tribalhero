namespace Simulator {
    partial class FormationPanel {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.typeLabel = new System.Windows.Forms.Label();
            this.UnitList = new System.Windows.Forms.ListView();
            this.chName = new System.Windows.Forms.ColumnHeader();
            this.chLvl = new System.Windows.Forms.ColumnHeader();
            this.chCount = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // typeLabel
            // 
            this.typeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.typeLabel.AutoSize = true;
            this.typeLabel.Location = new System.Drawing.Point(96, 9);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Size = new System.Drawing.Size(53, 13);
            this.typeLabel.TabIndex = 1;
            this.typeLabel.Text = "Formation";
            this.typeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.typeLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormationPanel_MouseMove);
            this.typeLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormationPanel_MouseDown);
            this.typeLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormationPanel_MouseUp);
            // 
            // UnitList
            // 
            this.UnitList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName,
            this.chLvl,
            this.chCount});
            this.UnitList.FullRowSelect = true;
            this.UnitList.Location = new System.Drawing.Point(0, 25);
            this.UnitList.MultiSelect = false;
            this.UnitList.Name = "UnitList";
            this.UnitList.Size = new System.Drawing.Size(258, 220);
            this.UnitList.TabIndex = 2;
            this.UnitList.UseCompatibleStateImageBehavior = false;
            this.UnitList.View = System.Windows.Forms.View.Details;
            this.UnitList.SelectedIndexChanged += new System.EventHandler(this.UnitList_SelectedIndexChanged);
            this.UnitList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UnitList_KeyDown);
            this.UnitList.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.UnitList_ItemDrag);
            // 
            // chName
            // 
            this.chName.Text = "Name";
            this.chName.Width = 100;
            // 
            // chLvl
            // 
            this.chLvl.Text = "Lvl";
            this.chLvl.Width = 30;
            // 
            // chCount
            // 
            this.chCount.Text = "Count";
            this.chCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // FormationPanel
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.UnitList);
            this.Controls.Add(this.typeLabel);
            this.Name = "FormationPanel";
            this.Size = new System.Drawing.Size(258, 245);
            this.Load += new System.EventHandler(this.FormationPanel_Load);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormationPanel_MouseMove);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormationPanel_DragDrop);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormationPanel_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormationPanel_MouseUp);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FormationPanel_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.ListView UnitList;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chLvl;
        private System.Windows.Forms.ColumnHeader chCount;

    }
}

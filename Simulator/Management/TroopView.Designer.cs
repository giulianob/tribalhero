namespace Simulator {
    partial class TroopView {
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
            this.UnitList = new System.Windows.Forms.ListView();
            this.chName = new System.Windows.Forms.ColumnHeader();
            this.chType = new System.Windows.Forms.ColumnHeader();
            this.chLvl = new System.Windows.Forms.ColumnHeader();
            this.chHp = new System.Windows.Forms.ColumnHeader();
            this.chAtk = new System.Windows.Forms.ColumnHeader("(none)");
            this.chDef = new System.Windows.Forms.ColumnHeader();
            this.chRng = new System.Windows.Forms.ColumnHeader();
            this.chStl = new System.Windows.Forms.ColumnHeader();
            this.chCrop = new System.Windows.Forms.ColumnHeader();
            this.chGold = new System.Windows.Forms.ColumnHeader();
            this.chIron = new System.Windows.Forms.ColumnHeader();
            this.chWood = new System.Windows.Forms.ColumnHeader();
            this.chUpkeep = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // UnitList
            // 
            this.UnitList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName,
            this.chType,
            this.chLvl,
            this.chHp,
            this.chAtk,
            this.chDef,
            this.chRng,
            this.chStl,
            this.chCrop,
            this.chGold,
            this.chIron,
            this.chWood,
            this.chUpkeep});
            this.UnitList.FullRowSelect = true;
            this.UnitList.Location = new System.Drawing.Point(3, 3);
            this.UnitList.MultiSelect = false;
            this.UnitList.Name = "UnitList";
            this.UnitList.Size = new System.Drawing.Size(790, 226);
            this.UnitList.TabIndex = 1;
            this.UnitList.UseCompatibleStateImageBehavior = false;
            this.UnitList.View = System.Windows.Forms.View.Details;
            this.UnitList.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.UnitList_QueryContinueDrag);
            this.UnitList.SelectedIndexChanged += new System.EventHandler(this.UnitList_SelectedIndexChanged);
            this.UnitList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.UnitList_MouseUp);
            this.UnitList.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.UnitList_GiveFeedback);
            this.UnitList.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.UnitList_ItemDrag);
            // 
            // chName
            // 
            this.chName.Text = "Name";
            this.chName.Width = 100;
            // 
            // chType
            // 
            this.chType.Text = "ID";
            // 
            // chLvl
            // 
            this.chLvl.Text = "Lvl";
            this.chLvl.Width = 30;
            // 
            // chHp
            // 
            this.chHp.Text = "HP";
            // 
            // chAtk
            // 
            this.chAtk.Text = "Atk";
            this.chAtk.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.chAtk.Width = 80;
            // 
            // chDef
            // 
            this.chDef.Text = "Def";
            this.chDef.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chDef.Width = 50;
            // 
            // chRng
            // 
            this.chRng.Text = "Rng";
            this.chRng.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chRng.Width = 50;
            // 
            // chStl
            // 
            this.chStl.Text = "Stl";
            this.chStl.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chStl.Width = 50;
            // 
            // chCrop
            // 
            this.chCrop.Text = "Crop";
            this.chCrop.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chCrop.Width = 50;
            // 
            // chGold
            // 
            this.chGold.Text = "Gold";
            this.chGold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chGold.Width = 50;
            // 
            // chIron
            // 
            this.chIron.Text = "Iron";
            this.chIron.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chIron.Width = 50;
            // 
            // chWood
            // 
            this.chWood.Text = "Wood";
            this.chWood.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chWood.Width = 50;
            // 
            // chUpkeep
            // 
            this.chUpkeep.Text = "Upkeep";
            this.chUpkeep.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TroopView
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.UnitList);
            this.Name = "TroopView";
            this.Size = new System.Drawing.Size(796, 232);
            this.Load += new System.EventHandler(this.TroopView_Load_1);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.TroopView_DragDrop);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView UnitList;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chType;
        private System.Windows.Forms.ColumnHeader chLvl;
        private System.Windows.Forms.ColumnHeader chAtk;
        private System.Windows.Forms.ColumnHeader chDef;
        private System.Windows.Forms.ColumnHeader chRng;
        private System.Windows.Forms.ColumnHeader chStl;
        private System.Windows.Forms.ColumnHeader chCrop;
        private System.Windows.Forms.ColumnHeader chGold;
        private System.Windows.Forms.ColumnHeader chIron;
        private System.Windows.Forms.ColumnHeader chWood;
        private System.Windows.Forms.ColumnHeader chHp;
        private System.Windows.Forms.ColumnHeader chUpkeep;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Game.Fighting;
using Game.Data;
using Game.Setup;

namespace Simulator {

    
    public partial class FormationPanel : UserControl {
        bool isDragging = false;
        private int clickOffsetX, clickOffsetY;

        public class DragDropItem {
            public delegate int MoveCount(FormationPanel panel, Unit unit, object custom);
            public Unit unit;
            public MoveCount callback;
            public object custom;
            
        }

        public FormationPanel() {
            InitializeComponent();
        }

        public FormationPanel(string name) {
            InitializeComponent();
            this.typeLabel.Text = name;

        
        }

        private void FormationPanel_Load(object sender, EventArgs e) {
            
        }

        public void add(Unit unit) {
            UnitStats stats = UnitFactory.getUnitStats((ushort)unit.type, unit.lvl);
            Resource resource = stats.resource;
            ListViewItem lparent = new ListViewItem(stats.name);
            lparent.Tag = unit;
            lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, unit.lvl.ToString()));
            lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, unit.count.ToString()));
            this.UnitList.Items.Add(lparent);
            lparent.EnsureVisible();

        }

        private void FormationPanel_DragDrop(object sender, DragEventArgs e) {
            DragDropItem ddi = (DragDropItem)e.Data.GetData(typeof(DragDropItem));
            Unit unit = ddi.unit;
            if ((unit.count = (ushort)ddi.callback(this,ddi.unit, ddi.custom)) == 0) {
                MessageBox.Show("Count must be greater than 0 or You dont have enough resource.");
                return;
            }
            this.add(unit);
        }

        private void FormationPanel_DragEnter(object sender, DragEventArgs e) {
            
            if (e.Data.GetDataPresent(typeof(DragDropItem))) {
                e.Effect = DragDropEffects.Move;
            } else {
                e.Effect = DragDropEffects.None;
            }
        }

        private void UnitList_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void UnitList_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Delete && UnitList.SelectedItems.Count>0 ) {
                int index = UnitList.SelectedIndices[0];
                Unit unit = (Unit)this.UnitList.SelectedItems[0].Tag;
                ((TroopPanel)Parent).Resource += (unit.Stats.resource * unit.count);
                this.UnitList.Items.Remove(this.UnitList.SelectedItems[0]);
                if( index<UnitList.Items.Count ) {
                    UnitList.Items[index].Selected = true;
                } else if (index > 0) {
                    UnitList.Items[index - 1].Selected = true;
                }
            }
        }

        private int dragdrop_callback(FormationPanel fp, Unit unit, object custom) {
            int ret = unit.count;
            this.UnitList.Items.RemoveAt((int)custom);
            return ret;
        }

        private void UnitList_ItemDrag(object sender, ItemDragEventArgs e) {
            DragDropItem item = new DragDropItem();
            item.unit = (Unit)this.UnitList.SelectedItems[0].Tag;
            item.callback = this.dragdrop_callback;
            item.custom = this.UnitList.SelectedIndices[0];
            this.DoDragDrop(item, DragDropEffects.Move);
        }

        private void FormationPanel_MouseDown(object sender, MouseEventArgs e) {
            isDragging = true;
            clickOffsetX = e.X;
            clickOffsetY = e.Y;
        }

        private void FormationPanel_MouseUp(object sender, MouseEventArgs e) {
            isDragging = false;
        }

        private void FormationPanel_MouseMove(object sender, MouseEventArgs e) {
            if (isDragging == true) {
                // The control coordinates are converted into form coordinates
                // by adding the label position offset.
                // The offset where the user clicked in the control is also
                // accounted for. Otherwise, it looks like the top-left corner
                // of the label is attached to the mouse.
                this.Left = e.X + this.Left - clickOffsetX;
                this.Top = e.Y + this.Top - clickOffsetY;
            }
        }

        public IEnumerable<Unit> getUnits() {
            foreach (ListViewItem lvi in UnitList.Items) {
                yield return (Unit)lvi.Tag;
            }
        }

        internal void LoadFormation(BattleFormation formation) {
            this.UnitList.Items.Clear();
            foreach (Unit unit in formation) {
                this.add(unit);
            }
        }

        internal void DeleteAll() {
            foreach (ListViewItem lvi in this.UnitList.Items) {
                Unit unit = (Unit)lvi.Tag;
                ((TroopPanel)Parent).Resource += (unit.Stats.resource * unit.count);

            }
            UnitList.Items.Clear();
        }
    }
}

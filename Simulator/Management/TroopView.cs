using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Game.Setup;
namespace Simulator {
    public partial class TroopView : UserControl {
        public TroopView() {
            InitializeComponent();
        }

        private void UnitList_SelectedIndexChanged(object sender, EventArgs e) {
            
        }

        private void TroopView_DragDrop(object sender, DragEventArgs e) {

        }
        private int getCount(FormationPanel fp, Unit unit, object custom) {
            GenericInputBox box = new GenericInputBox("Count", "Quantity", "1");
            if (box.ShowDialog() == DialogResult.OK) {
                int ret = int.Parse(box.Value);
                // reduce resbource
                if (!((TroopPanel)fp.Parent).Resource.hasEnough(unit.Stats.resource * ret)) {
                    return 0;
                }
                ((TroopPanel)fp.Parent).Resource -= (unit.Stats.resource * ret);
                return ret;
            } else {
                return 0;
            }

        }

        private void UnitList_ItemDrag(object sender, ItemDragEventArgs e) {
            UnitStats stats = (UnitStats)this.UnitList.SelectedItems[0].Tag;
            Unit unit = new Unit();
            unit.type = stats.type;
            unit.lvl = stats.lvl;

            FormationPanel.DragDropItem ddnu = new FormationPanel.DragDropItem();
            ddnu.callback = getCount;
            ddnu.unit = unit;
            this.DoDragDrop(ddnu, DragDropEffects.Move);
        }

        private void UnitList_MouseUp(object sender, MouseEventArgs e) {

        }

        private void UnitList_QueryContinueDrag(object sender, QueryContinueDragEventArgs e) {
            if (e.Action == DragAction.Drop) {
                Console.Out.Write("drop");
            }
        }

        private void UnitList_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
            Console.Out.WriteLine("abc");
        }

        private void TroopView_Load_1(object sender, EventArgs e) {
            if (UnitFactory.dict != null) {
                foreach (KeyValuePair<int, UnitStats> kvp in UnitFactory.dict) {
                    ListViewItem lparent = new ListViewItem(kvp.Value.name);

                    lparent.Tag = kvp.Value;

                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, String.Format("{0}", kvp.Key / 100)));
                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, kvp.Value.lvl.ToString()));
                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, kvp.Value.stats.MaxHp.ToString()));
                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, kvp.Value.stats.Atk.ToString()));
                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, kvp.Value.stats.Def.ToString()));
                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, kvp.Value.stats.Rng.ToString()));
                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, kvp.Value.stats.Stl.ToString()));

                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, kvp.Value.resource.Crop.ToString()));
                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, kvp.Value.resource.Gold.ToString()));
                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, kvp.Value.resource.Iron.ToString()));
                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, kvp.Value.resource.Wood.ToString()));
                    lparent.SubItems.Add(new ListViewItem.ListViewSubItem(lparent, kvp.Value.upkeep.ToString()));

                    this.UnitList.Items.Add(lparent);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using Game.Fighting;
using System.Drawing.Imaging;
using Game.Battle;
using Game.Data;
using Game.Setup;

namespace Simulator.Viewer {

    public partial class TroopListView : UserControl {
        public class ListViewData {
            public CombatObject obj;
            public uint mhp;
            public ListViewData(CombatObject obj) {
                this.obj = obj;
                this.mhp = obj.HP;
            }
        }
        TroopStub stub;
        UnitTemplate template;
        Dictionary<ushort, Label> reportTableDict;

        Pen gray_pen = new Pen(Color.DarkSlateGray, 2);
        Pen red_pen = new Pen(Color.Red, 2);
        Font font = new Font(FontFamily.GenericSerif, 24,FontStyle.Bold);
        Font font2 = new Font(FontFamily.GenericSerif, 8, FontStyle.Regular);
        SolidBrush brush = new SolidBrush(Color.Black);
        Graphics g;

        BattleExecutor battle;
        CombatList troop;
        bool attack_troop;
        Dictionary<uint,ListViewItem> dict;

        public TroopListView() {
            InitializeComponent();
            g = listView1.CreateGraphics();
        }

        public void MonitorBattle(BattleExecutor battle, bool attack_troop) {
            this.battle = battle;
            this.attack_troop = attack_troop;
            battle.ActionAttacked += new BattleBase.OnAttack(battle_ActionAttacked);
            battle.EnterBattle += new BattleBase.OnBattle(battle_EnterBattle);
            battle.ExitBattle += new BattleBase.OnBattle(battle_ExitBattle);

        }

        public void ExitMonitorBattle() {
            battle.ActionAttacked -= new BattleBase.OnAttack(battle_ActionAttacked);
            battle.EnterBattle -= new BattleBase.OnBattle(battle_EnterBattle);
            battle.ExitBattle -= new BattleBase.OnBattle(battle_ExitBattle);
        }

        delegate void OnActionAttackedCallback(CombatObject source, CombatObject target, ushort damage);
        void battle_ActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            if (this.InvokeRequired) {
                // Execute the same method, but this time on the GUI thread
                this.BeginInvoke(new OnActionAttackedCallback(battle_ActionAttacked), source, target, damage);
                return;
            }
            ListViewItem viewitem;
            if (!attack_troop) {
                Console.Out.Write("hello");
            }
            this.RefreshTable();
            listView1.Refresh();
            if (dict.TryGetValue(source.Id, out viewitem)) {
                g.DrawRectangle(gray_pen, viewitem.Bounds);
            } else if (dict.TryGetValue(target.Id, out viewitem)) {
                g.DrawString(damage.ToString(), font, Brushes.Red, viewitem.Bounds.X + 15, viewitem.Bounds.Y + 10);
                g.DrawRectangle(red_pen, viewitem.Bounds);
            }
            
        }

        delegate void OnEnterBattleCallback(CombatList atk, CombatList def);
        void battle_ExitBattle(CombatList atk, CombatList def) {
            
        }
        void battle_EnterBattle(CombatList atk, CombatList def) {
            if (this.InvokeRequired) {
                // Execute the same method, but this time on the GUI thread
                this.BeginInvoke(new OnEnterBattleCallback(battle_EnterBattle), atk,def);
                return;
            }
            if (attack_troop) {
                troop = atk;
                AttackCombatUnit unit = troop[0] as AttackCombatUnit;
                this.stub = unit.TroopStub;
                this.template = unit.City.Template;
                Set(stub, template);
            } else {
                troop = def;
                DefenseCombatUnit unit = troop[0] as DefenseCombatUnit;
                this.stub = unit.TroopStub;
                this.template = unit.City.Template;
                Set(stub, template);
            }
            dict = new Dictionary<uint, ListViewItem>();

            foreach (CombatObject obj in troop) {
                ICombatUnit unit = obj as ICombatUnit;
                ListViewItem item = new ListViewItem();
               // tableLayoutPanel1.Controls.
     //           item.ToolTipText = getToolTipString(unititem);
                item.Tag = new ListViewData(obj);
                listView1.Items.Add(item);
                dict[obj.Id] = item;
            }
        }

        delegate void OnUnitUpdatedCallback(Troop troop, FormationType type, Unit unit);
        void battle_UnitUpdated(Troop troop, FormationType type, Unit unit) {
             if (this.InvokeRequired) {
                // Execute the same method, but this time on the GUI thread
                 this.BeginInvoke(new OnUnitUpdatedCallback(battle_UnitUpdated),troop,type,unit);
                return;
            }
    /*        if (this.troop == troop) {
                ListViewItem item;
                if (dict.TryGetValue(unit.id, out item)) {
                    UnitItem unititem = (UnitItem)item.Tag;
                    item.ToolTipText = getToolTipString(unititem);
                    listView1.RedrawItems(item.Index, item.Index, false);
                }
            }                */
        }


        private void listView1_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void TroopListView_Load(object sender, EventArgs e) {

        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e) {
            ListViewData data = (ListViewData)e.Item.Tag;
            CombatObject item = data.obj;
            ushort type=0;
            ushort count=0;
            if (item is AttackCombatUnit) {
                type = (item as AttackCombatUnit).Type;
                count = (item as AttackCombatUnit).Count;
            } else if (item is DefenseCombatUnit) {
                type = (item as DefenseCombatUnit).Type;
                count = (item as DefenseCombatUnit).Count;
            }
            Bitmap image;
      //      image = (Bitmap)ImageFactory.getImage(type);
           /* if (unit != null) {
            } else {
              //  Bitmap image = (Bitmap)ImageFactory.getImage(item.Type);
            }*/

            
            e.Graphics.DrawRectangle(Pens.Black, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, 10);
            
            
            if (count == 0) {
                e.Graphics.FillRectangle(Brushes.Red, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, 10);
                e.Graphics.DrawString("Died", Font, Brushes.Black, e.Bounds.X, e.Bounds.Y);
            } else {
          /*      if (their_vsn > item.unit.stats.stealth) {
                    e.Graphics.FillRectangle(Brushes.Green, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, 10);
                } else {
                    e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, 10);
                }*/
                e.Graphics.FillRectangle(Brushes.Green, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, 10);
                int green_x = (int)(e.Bounds.Width * item.HP / data.mhp);
                e.Graphics.FillRectangle(Brushes.Red, e.Bounds.X + green_x, e.Bounds.Y, e.Bounds.Width - green_x, 10);

            }
            UnitStats stats = UnitFactory.getUnitStats(item.Type, item.Lvl);
            e.Graphics.DrawString(stats.name.Substring(0,stats.name.Length>11?11:stats.name.Length), font2, brush, e.Bounds.X,e.Bounds.Y+10);
            e.Graphics.DrawString(item.HP.ToString()+"["+item.Count+"]", font2, brush, e.Bounds.X,e.Bounds.Y+20);
            e.Graphics.DrawString("Dmg: "+item.DmgDealt.ToString(), font2, brush, e.Bounds.X, e.Bounds.Y + 30);
            e.Graphics.DrawString("Rcv: "+item.DmgRecv.ToString(), font2, brush, e.Bounds.X, e.Bounds.Y + 40);
            /*       e.Graphics.DrawImageUnscaled(image,
                                               e.Bounds.X + (e.Bounds.Width - image.Width) / 2,
                                               e.Bounds.Y + (e.Bounds.Height - image.Height) / 2 + 10);*/

            e.DrawFocusRectangle();
        }
        string getToolTipString(ICombatUnit item) {
            string str = "not implemented";
        /*    string str = item.unit.name;
            str += "\nCount:" + item.unit.count;
            str += "\nMax:" + item.max_count;
            str += "\nAtk:" + item.unit.stats.atk;
            str += "\nDef:" + item.unit.stats.def;*/
            return str;
        }

        internal void Close() {
        }

        public void RefreshTable() {
            Formation formation;
            if (!attack_troop) {
                Console.Out.WriteLine("hello");
            }
                if (stub.TryGetValue(FormationType.Normal, out formation)) {
                    ushort count;
                    foreach (KeyValuePair<ushort, Label> kvp in new Dictionary<ushort, Label>(reportTableDict)) {
                        if (formation.TryGetValue(kvp.Key, out count)) {
                            reportTableDict[kvp.Key].Text = count.ToString();
                        } else {
                            reportTableDict[kvp.Key].Text = "0";
                        }
                    }
                }
            
        }

        public void Set(TroopStub stub, UnitTemplate template) {
            this.stub = stub;
            this.template = template;

            this.tableLayoutPanel1.Controls.Clear();
            this.tableLayoutPanel1.Visible = false;
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel1.Padding = new Padding(1, 1, 4, 5);

            Formation formation;
            if (stub.TryGetValue(FormationType.Normal, out formation)) {
                reportTableDict = new Dictionary<ushort, Label>();
                this.tableLayoutPanel1.RowCount = 3;
                
                for (int i = 0; i < 3; ++i) {
                    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute,16));
                }
                this.tableLayoutPanel1.ColumnCount = formation.Count;
                for (int i = 0; i < formation.Count; ++i) {
                    this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,40));
                }
                int index = 0;
                foreach (KeyValuePair<ushort, ushort> kvp in formation) {
                    UnitStats stats = template[kvp.Key];

                    Label lbl = new Label();
                    lbl.Text = stats.name;
//                    lbl.Dock = DockStyle.Fill;
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    this.tableLayoutPanel1.Controls.Add(lbl, index, 0);

                    lbl = new Label();
                 //   lbl.Dock = DockStyle.Fill;
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.Text = kvp.Value.ToString();
                    this.tableLayoutPanel1.Controls.Add(lbl, index, 1);

                    lbl = new Label();
                 //   lbl.Dock = DockStyle.Fill;
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.Text = kvp.Value.ToString();
                    this.tableLayoutPanel1.Controls.Add(lbl, index, 2);
                    reportTableDict.Add(kvp.Key, lbl);
                    ++index;
                }
            }
            this.tableLayoutPanel1.Visible = true;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e) {

        }
    }
}

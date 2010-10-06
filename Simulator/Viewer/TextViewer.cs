using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Game.Fighting;
using Game.Battle;

namespace Simulator.Viewer {
    public partial class TextViewer : UserControl {
        BattleExecutor battle;
        public TextViewer() {
            InitializeComponent();
            richTextBox1.BackColor = Color.Green;
            richTextBox1.ForeColor = Color.Yellow;
        }

        private void btnClear_Click(object sender, EventArgs e) {

            this.richTextBox1.Clear();
        }

        private void button2_Click(object sender, EventArgs e) {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
                Stream a = File.Create(saveFileDialog1.FileName);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(a, richTextBox1.Text);
                a.Close();
            }
        }
        delegate void OnAppendCallback(string text);
        public void Append(string text) {
            if (this.InvokeRequired) {
                // Execute the same method, but this time on the GUI thread
                this.BeginInvoke(new OnAppendCallback(Append), text);
                return;
            }
            richTextBox1.AppendText(text);
    //        this.richTextBox1.AppendText(packet.ToString());
        }

        public void MonitorBattle(BattleExecutor battle) {
            this.battle = battle;
        //    this.trackBar1.Value = this.trackBar1.Maximum - (int)battle.Delay.TotalMilliseconds +200;
            battle.EnterBattle += new BattleBase.OnBattle(battle_EnterBattle);
            battle.ExitBattle += new BattleBase.OnBattle(battle_ExitBattle);
            battle.EnterTurn += new BattleBase.OnTurn(battle_EnterTurn);
            battle.ExitTurn += new BattleBase.OnTurn(battle_ExitTurn);
            battle.UnitRemoved += new BattleBase.OnUnitUpdate(battle_UnitRemoved);
            battle.ActionAttacked += new BattleBase.OnAttack(battle_ActionAttacked);
            battle.EnterRound += new BattleBase.OnRound(battle_EnterRound);
        }

        void battle_EnterRound(CombatList atk, CombatList def, uint round, int stamina) {
            Append("Round[" + round + "] Stamina[" + stamina+ "]\n");
        
        }

        public void ExitMonitorBattle() {
            //    this.trackBar1.Value = this.trackBar1.Maximum - (int)battle.Delay.TotalMilliseconds +200;
            battle.EnterBattle -= new BattleBase.OnBattle(battle_EnterBattle);
            battle.ExitBattle -= new BattleBase.OnBattle(battle_ExitBattle);
            battle.EnterTurn -= new BattleBase.OnTurn(battle_EnterTurn);
            battle.ExitTurn -= new BattleBase.OnTurn(battle_ExitTurn);
            battle.UnitRemoved -= new BattleBase.OnUnitUpdate(battle_UnitRemoved);
            battle.ActionAttacked -= new BattleBase.OnAttack(battle_ActionAttacked);
            battle.EnterRound -= new BattleBase.OnRound(battle_EnterRound);
        }

        void battle_ActionAttacked(CombatObject source, CombatObject target, ushort damage)
        {

            Append("Unit[" + source.Id + "] Attacked Unit[" + target.Id + "] --- Damage[" + damage + "]\n");
        }

        void battle_UnitRemoved(CombatObject obj)
        {
            ICombatUnit unit = obj as Game.Battle.ICombatUnit;
            if (unit == null)
            {
                Append("structure[" + obj.Id + "] is removed from Troop[" + obj.CombatList.Id + "]\n");
            }
            else
            {
                Append("Unit[" + obj.Id + "] is removed from Troop[" + obj.CombatList.Id + "] Formation [" + Enum.GetName(typeof(Game.Fighting.FormationType), unit.Formation) + "]\n");
            }
        }

        void battle_ExitTurn(CombatList atk, CombatList def, int turn) {
            Append("Turn[" + turn + "] Ended with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        void battle_EnterTurn(CombatList atk, CombatList def, int turn) {
            Append("Turn[" + turn + "] Started with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        void battle_ExitBattle(CombatList atk, CombatList def) {
            Append("Battle Ended with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }

        void battle_EnterBattle(CombatList atk, CombatList def) {
            Append("Battle Started with atk_size[" + atk.Count + "] def_size[" + def.Count + "]\n");
        }






        public void Close() {
            battle.ExitBattle -= battle_ExitBattle;
            battle.EnterBattle -= battle_EnterBattle;
            battle.ExitTurn -= battle_ExitTurn;
            battle.EnterTurn -= battle_EnterTurn;
            battle.UnitRemoved -= battle_UnitRemoved;
            battle.ActionAttacked -= battle_ActionAttacked;

        }


        private void richTextBox1_TextChanged(object sender, EventArgs e) {

        }

        private void trackBar1_ValueChanged(object sender, EventArgs e) {
            this.battle.Delay = TimeSpan.FromMilliseconds(trackBar1.Maximum-trackBar1.Value+200);
            this.richTextBox1.Focus();
        }

        private void trackBar1_Scroll(object sender, EventArgs e) {

        }
    }
}

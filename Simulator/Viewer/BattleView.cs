using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Game.Battle;

namespace Simulator {
    public partial class BattleView : Form {
        BattleManager bm;
        public BattleView() {
            InitializeComponent();
        }

        public BattleExecutor Battle {
            set {
                bm = value;
                this.textViewer1.MonitorBattle(value);
                this.troopListView1.MonitorBattle(value,true);
                this.troopListView2.MonitorBattle(value, false);
                value.EnterRound += new BattleBase.OnRound(value_EnterRound);
              //  value.EnterTurn += new BattleBase.OnTurn(value_EnterTurn);
                value.ActionAttacked += new BattleBase.OnAttack(value_ActionAttacked);
                value.ExitBattle += new BattleBase.OnBattle(value_ExitBattle);
            }
        }

        void value_ExitBattle(CombatList atk, CombatList def) {
            Console.Out.WriteLine("done");
        }

        delegate void OnValue_ActionAttacked(CombatObject source, CombatObject target, ushort damage);
        void value_ActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            if (this.InvokeRequired) {
                // Execute the same method, but this time on the GUI thread
                this.BeginInvoke(new OnValue_ActionAttacked(value_ActionAttacked), source, target, damage);
                return;
            }
            this.lblTurn.Text = (int.Parse(this.lblTurn.Text) + 1).ToString();
        }

        delegate void OnValue_EnterTurn(CombatList atk, CombatList def, int turn);
        void value_EnterTurn(CombatList atk, CombatList def, int turn) {
            if (this.InvokeRequired) {
                // Execute the same method, but this time on the GUI thread
                this.BeginInvoke(new OnValue_EnterTurn(value_EnterTurn), atk, def, turn);
                return;
            }
            this.lblTurn.Text = turn.ToString();
        }

        delegate void OnValue_EnterRound(CombatList atk, CombatList def, uint turn,int stamina);
        void value_EnterRound(CombatList atk, CombatList def, uint round, int stamina) {
            if (this.InvokeRequired) {
                this.BeginInvoke(new OnValue_EnterRound(value_EnterRound), atk, def, round, stamina);
                return;
            }
            this.lblRound.Text = round.ToString();
            this.lblStamina.Text = stamina.ToString();
        }

        private void BattleView_Load(object sender, EventArgs e) {
            
        }

        private void BattleView_FormClosing(object sender, FormClosingEventArgs e) {
            bm.EnterRound -= new BattleBase.OnRound(value_EnterRound);
      //      bm.EnterTurn -= new BattleBase.OnTurn(value_EnterTurn);
            bm.ActionAttacked -= new BattleBase.OnAttack(value_ActionAttacked);
            this.textViewer1.ExitMonitorBattle();
            this.textViewer1.Close();
            this.troopListView1.ExitMonitorBattle();
            this.troopListView1.Close();
            this.troopListView2.ExitMonitorBattle();
            this.troopListView2.Close();
 
        }

        private void textViewer1_Load(object sender, EventArgs e) {

        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Game.Fighting;
using Game.Data;
using Game.Battle;
using Game;
using Game.Setup;
namespace Simulator {

    public partial class Form1 : Form {
        Resource resource1 = new Resource(500, 500, 500, 500,0);
        Resource resource2 = new Resource(500, 500, 500, 500,0);

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            UnitFactory.init("c:\\source\\GameServer\\Game\\Setup\\CSV\\unit.csv");
            
        }

        public BattleFormation getFormation() {
            return null;
        }

        private void troopView1_Load(object sender, EventArgs e) {
            this.troopPanel1.Resource = resource1;
            this.troopPanel2.Resource = resource2;
        }

        private void btnSimulate_Click(object sender, EventArgs e) {
            Structure main = new Structure(2000, 1, new StructureStats(new BattleStats(), 0));
            Structure main2 = new Structure(2000, 1, new StructureStats(new BattleStats(), 0));
            City city = new City(new Player(1, "Player 1"), "NEW MODULE", new Resource(), main);
            TroopStub localStub = this.troopPanel1.getTroopStub();
            city.DefaultTroop.add(localStub);
            city.DefaultTroop[FormationType.InBattle] = city.DefaultTroop[FormationType.Normal];
            //city.DefaultTroop[FormationType.Normal] = new Formation(city.DefaultTroop);

            City city2 = new City(new Player(2, "Player 1"), "NEW MODULE2", new Resource(), main2);
            TroopStub atkStub = this.troopPanel2.getTroopStub();
            byte id;
            city2.Troops.Add(atkStub,out id);
            TroopObject troop = new TroopObject(city2.Troops[id]);
            atkStub.TroopObject = troop;
            city2.add(troop);
            BattleExecutor bm = new BattleExecutor(city);

            List<TroopStub> defenders = new List<TroopStub>();
            defenders.Add(city.DefaultTroop);
            bm.AddToLocal(defenders, ReportState.Entering);

            List<TroopStub> attackers = new List<TroopStub>();
            attackers.Add(atkStub);
            bm.addToAttack(attackers);

            BattleView bv = new BattleView();
            bv.Battle = bm;
            bv.Show();
            bm.execute();
        }

        private void btnServer_Click(object sender, EventArgs e) {
         //   RemoteView rv = new RemoteView((int)this.numericUpDown1.Value,true,this.troopPanel1.getTroop());
         ///   rv.ShowDialog();
        }

    }
}
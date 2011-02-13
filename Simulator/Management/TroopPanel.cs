using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Game.Fighting;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Game.Data;

namespace Simulator {
    
    public partial class TroopPanel : UserControl {
        [Serializable()]
        class PanelData {
            internal Troop troop;
            internal Resource resource;
        }

        Troop troop = new Troop();
        Dictionary<FormationType, FormationPanel> panels = new Dictionary<FormationType, FormationPanel>();
        public TroopPanel() {
            InitializeComponent();
        }
        public Game.Data.Resource Resource {
            get { return resourcePanel1.Resource; }
            set { resourcePanel1.Resource = value; }
        }

        private void TroopPanel_Load(object sender, EventArgs e) {
            this.cbFormationType.DataSource = Enum.GetValues(typeof(FormationType));
        }

        private void btnAdd_Click(object sender, EventArgs e) {
            Random random = new Random();
            if( !panels.ContainsKey((FormationType)this.cbFormationType.SelectedValue ) ) {
                FormationPanel fp = new FormationPanel(Enum.GetName(typeof(FormationType),this.cbFormationType.SelectedValue));
                fp.AllowDrop = true;
                fp.BorderStyle = BorderStyle.FixedSingle;
                fp.Location = new System.Drawing.Point(random.Next(100,300),random.Next(100,300));
                fp.Name = Enum.GetName(typeof(FormationType), this.cbFormationType.SelectedValue);
                fp.Size = new System.Drawing.Size(258, 245);
                fp.Enter += new EventHandler(fp_Enter);
                //.fp.this.formationPanel1.TabIndex = 2;
                this.Controls.Add(fp);
                
                panels[(FormationType)this.cbFormationType.SelectedValue] = fp;
            }
        }

        void fp_Enter(object sender, EventArgs e) {
            ((FormationPanel)sender).BringToFront();
        }

        private void btnDelete_Click(object sender, EventArgs e) {
            
            FormationPanel fp;
            if (panels.TryGetValue((FormationType)this.cbFormationType.SelectedValue,out fp)) {
                fp.DeleteAll();
                this.Controls.Remove(fp);
                panels.Remove((FormationType)this.cbFormationType.SelectedValue);
            }
        }

        private void resourcePanel1_Load(object sender, EventArgs e) {
        }

        public Troop getTroop() {
            Troop troop = new Troop();
            foreach (KeyValuePair<FormationType, FormationPanel> kvp in panels) {
                troop.add_formation(kvp.Key);

                foreach (Unit unit in kvp.Value.getUnits()) {
                    troop.add_unit(kvp.Key, (Unit)unit.Clone());
                }
            }
            return troop;
        }
        public TroopStub getTroopStub() {
            TroopStub stub = new TroopStub(null);
            foreach (KeyValuePair<FormationType, FormationPanel> formation in panels) {
                stub.addFormation(formation.Key);
                foreach (Unit unit in formation.Value.getUnits()) {
                    stub.addUnit(formation.Key, (ushort)unit.type, unit.count);
                }
            }
            return stub;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
                Stream a= File.Create(saveFileDialog1.FileName);
                BinaryFormatter bf=new BinaryFormatter();
                PanelData pd = new PanelData();
                pd.troop = this.getTroop();
                pd.resource = this.Resource;
                bf.Serialize(a,pd);
                a.Close();
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                FileStream file = new FileStream(openFileDialog1.FileName,FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                PanelData obj = bf.Deserialize(file) as PanelData;
                file.Close();
                this.LoadTroop(obj.troop);
                this.Resource = obj.resource;
            }
        }

        private void LoadTroop(Troop obj) {
            Random random = new Random();
            foreach( FormationPanel fp in panels.Values) {
                this.Controls.Remove(fp);
            }
            panels.Clear();
            foreach (KeyValuePair<FormationType, BattleFormation> kvp in obj.formations) {
                FormationPanel fp = new FormationPanel(Enum.GetName(typeof(FormationType),kvp.Key));
                fp.AllowDrop = true;
                fp.BorderStyle = BorderStyle.FixedSingle;
                fp.Location = new System.Drawing.Point(random.Next(100, 300), random.Next(100, 300));
                fp.Name = Enum.GetName(typeof(FormationType), kvp.Key);
                fp.Size = new System.Drawing.Size(258, 245);
                fp.Enter += new EventHandler(fp_Enter);
                //.fp.this.formationPanel1.TabIndex = 2;
                this.Controls.Add(fp);

                panels[(FormationType)kvp.Key] = fp;
                fp.LoadFormation(kvp.Value);
 
            }
           
        }
    }
}

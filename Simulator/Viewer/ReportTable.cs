using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Game.Data;
using Game.Fighting;
using Game.Setup;

namespace Simulator.Viewer {
    public partial class ReportTable : UserControl {
        TroopStub stub;
        UnitTemplate template;

        Dictionary<ushort, Label> dict = new Dictionary<ushort, Label>();
        public ReportTable() {
            InitializeComponent();

        }


        public void RefreshTable() {
            Formation formation;
            if (stub.TryGetValue(FormationType.Normal, out formation)) {
                ushort count;
                foreach (KeyValuePair<ushort, Label> kvp in new Dictionary<ushort,Label>(dict)) {
                    if( formation.TryGetValue(kvp.Key,out count) ) {
                        dict[kvp.Key].Text = count.ToString();
                    } else {
                        dict[kvp.Key].Text = "0";
                    }
                }
            }
        }

        public void Set(TroopStub stub, UnitTemplate template) {
            this.stub = stub;
            this.template = template;

          //  this.tableLayoutPanel1.Controls.Clear();

            Formation formation;
            if( stub.TryGetValue(FormationType.Normal,out formation) ) {
                dict = new Dictionary<ushort, Label>();
                for (int i = 0; i < 3; ++i) {
                    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                }
                for (int i = 0; i < formation.Count; ++i) {
                    this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                }
                int index = 0;
                foreach (KeyValuePair<ushort, ushort> kvp in formation) {
                    UnitStats stats = template[kvp.Key];

                    Label lbl = new Label();
                    lbl.Text = stats.name;
                    this.tableLayoutPanel1.Controls.Add(lbl, index, 0);

                    lbl = new Label();
                    lbl.Text = kvp.Value.ToString();
                    this.tableLayoutPanel1.Controls.Add(lbl, index, 1);

                    lbl = new Label();
                    lbl.Text = kvp.Value.ToString();
                    this.tableLayoutPanel1.Controls.Add(lbl, index, 2);
                    dict.Add(kvp.Key, lbl);
                    ++index;
                }
            }
        }
    }
}

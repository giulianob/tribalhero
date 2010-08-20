using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Game.Data;
namespace Simulator {
    public partial class ResourcePanel : UserControl {
        Resource resource = new Resource(500, 500, 500, 500,0);
        int time = 50000;

        public ResourcePanel() {
            InitializeComponent();
        }
        private void update() {
            this.lblCrop.Text = resource.Crop.ToString();
            this.lblGold.Text = resource.Gold.ToString();
            this.lblron.Text = resource.Iron.ToString();
            this.lblWood.Text = resource.Wood.ToString();
        }
        public Resource Resource {
            get { return resource; }
            set {
                resource = value; 
                update();
            }
        }
        public int Time {
            get { return time; }
            set { time = value; }
        }
        private void label1_Click(object sender, EventArgs e) {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using Game.Fighting;

namespace Simulator {
    
    public partial class Foo : Form {
        public class UnitListItem : ListViewItem {
            public int init_count;
            public delegate void OnChanged(Troop troop, FormationType type, Unit unit);
        //    public event OnChanged UnitChanged
        }
        Hashtable ht = new Hashtable();
        Graphics g;
        int index = 0;
        public Foo() {

            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            g = Graphics.FromHwnd(this.Handle);
            if( index== imageList1.Images.Count ) {
                this.Refresh();
              //  g.DrawImage(this.imageList1.Images[0], new Point(0, 0));
                index = 1;
            } else {
                this.Refresh();
                this.imageList1.Draw(g, 30, 30, index);
                ++index;
            }

        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e) {
            e.Graphics.DrawRectangle(new Pen(Color.Violet), e.Bounds);
            e.Graphics.FillRectangle(Brushes.Beige,e.Bounds);
            e.Graphics.DrawImageUnscaled((Bitmap)ht["Infantry.gif"], e.Bounds.X, e.Bounds.Y + 10);
            
        }

        private void toolTip1_Draw(object sender, DrawToolTipEventArgs e) {
            e.Graphics.DrawString("Helloworld\n", e.Font, Brushes.Beige, e.Bounds.X, e.Bounds.Y);
            
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void Foo_Load(object sender, EventArgs e) {
            
            
        }
    }
}
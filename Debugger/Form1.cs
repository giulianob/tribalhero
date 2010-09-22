using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using Game.Comm;
using System.Threading;
namespace Debugger {
    public partial class Form1 : Form {
        ClientProcessor processor;
        TcpClientSession tcpclient;
      //  Packet packet;

        delegate void OnReceivePacketCallback(Packet packet);
        public void OnReceivePacket(Packet packet) {
            if (this.InvokeRequired) {
                // Execute the same method, but this time on the GUI thread
                this.BeginInvoke(new OnReceivePacketCallback(OnReceivePacket), packet);
                return;
            }
            logPacket(packet, true);
    //        this.richTextBox1.AppendText(packet.ToString());
        }

        public Form1() {
            InitializeComponent();
            processor = new ClientProcessor();
            
            tcpclient = new TcpClientSession("Client", processor);
            tcpclient.ReceivedEvent += new TcpClientSession.OnReceive(this.logInPacket);
            tcpclient.SendEvent += new TcpClientSession.OnSend(this.logOutPacket);
        }
 
        private void ConnectBtn_Click(object sender, EventArgs e) {
            String[] address = ServerTextBox.Text.Split(':');
            if( address.Length<2 ) return;
            if (tcpclient.connect(address[0], int.Parse(address[1]))) {
                ServerTextBox.Enabled = false;
            }
        }

        private void disconnectBtn_Click(object sender, EventArgs e) {
            tcpclient.disconnect();
            ServerTextBox.Enabled = true;
        }

        private void LoginBtn_Click(object sender, EventArgs e) {
            Packet p = new Packet(Command.LOGIN);
            p.addString("abcde");
            tcpclient.write(p);
        }

        private void Form1_Load(object sender, EventArgs e) {
            this.comboBox1.DataSource = Enum.GetValues(typeof(Command));
        }

        private void button1_Click(object sender, EventArgs e) {
            Packet packet = new Packet((Command)comboBox1.SelectedValue);
            foreach (ListViewItem item in listParamView.Items) {
                packet.addParamater((Parameter)item.Tag);
            }
            tcpclient.write(packet);
        }
        public void logInPacket(Packet packet) {
            logPacket(packet, true);
        }
        public void logOutPacket(Packet packet) {
            Packet new_packet = new Packet(packet.getBytes());
            logPacket(new_packet, false);
        }
        delegate void OnLogPackage(Packet packet, bool incoming);
        public void logPacket(Packet packet, bool incoming) {
            if (InvokeRequired) {
                this.BeginInvoke(new OnLogPackage(logPacket), packet,incoming);
            }
            else {
                if (listPacketView.Items.Count > 1000) {
                    listPacketView.Items.RemoveAt(0);
                }

                //add item
                ListViewItem lparent = new ListViewItem(incoming ? "R" : "S");

                lparent.Tag = packet;
                // Time
                ListViewItem.ListViewSubItem subTime =
                    new ListViewItem.ListViewSubItem(lparent, System.DateTime.Now.ToString());
                lparent.SubItems.Add(subTime);
                // Seq
                ListViewItem.ListViewSubItem subSeq = new ListViewItem.ListViewSubItem(lparent, Convert.ToString(packet.Seq));
                lparent.SubItems.Add(subSeq);
                // Cmd
                string cmd = Enum.GetName(typeof(Command), packet.Cmd);
                if (cmd == "") cmd = Convert.ToString(packet.Cmd);
                ListViewItem.ListViewSubItem subCmd =
                    new ListViewItem.ListViewSubItem(lparent, cmd);
                lparent.SubItems.Add(subCmd);
                // Option
                ListViewItem.ListViewSubItem subOpt =
                    new ListViewItem.ListViewSubItem(lparent, Convert.ToString(packet.Option));
                lparent.SubItems.Add(subOpt);
                // rawLen
                ListViewItem.ListViewSubItem subraw =
                    new ListViewItem.ListViewSubItem(lparent, Convert.ToString(packet.Length));
                lparent.SubItems.Add(subraw);
                /*     // rawLen
                     ListViewItem.ListViewSubItem subraw =
                         new ListViewItem.ListViewSubItem(lparent, Convert.ToString(packet.Length));
                     lparent.SubItems.Add(subraw);
                     //String status = (string)Program.errors.GetName(packet.Status);
                     string status = packet.Status.ToString();
                     if (status == "") status = Convert.ToString(packet.Status);
                     ListViewItem.ListViewSubItem subStatus =
                         new ListViewItem.ListViewSubItem(lparent, status);
                     lparent.SubItems.Add(subStatus);

                     ListViewItem.ListViewSubItem subMsgId =
                         new ListViewItem.ListViewSubItem(lparent, Convert.ToString(packet.MsgId));
                     lparent.SubItems.Add(subMsgId);

                     ListViewItem.ListViewSubItem subLength =
                         new ListViewItem.ListViewSubItem(lparent, Convert.ToString(packet.Length));
                     lparent.SubItems.Add(subLength);*/

                listPacketView.Items.Add(lparent);
                listPacketView.Items[listPacketView.Items.Count - 1].EnsureVisible();
            }
        
        }

        private void listPacketView_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void listPacketView_DoubleClick(object sender, EventArgs e) {
            if (listPacketView.SelectedIndices.Count == 0) return;
      /*      if (index > lstPacketViewer.Items.Count - 1) return;*/

            PacketViewer p = new PacketViewer();
            Packet packet = this.listPacketView.SelectedItems[0].Tag as Packet;
            p.Text = "Packet Viewer";
            p.packetTree.Nodes.Clear();
            PacketTree packetTree = new PacketTree(packet);
            p.packetTree.Nodes.AddRange(packetTree.getTreeNodes());
            p.Show();
        }

        private void addParam(Parameter param, string type) {
            ListViewItem parent = new ListViewItem(type);
            parent.Tag = param;
            parent.SubItems.Add(new ListViewItem.ListViewSubItem(parent, ValueTextBox.Text));
            listParamView.Items.Add(parent);
        }
        private void UInt16Btn_Click(object sender, EventArgs e) {
            ushort result;
            if (!ushort.TryParse(ValueTextBox.Text, out result)) return;
            addParam(new Parameter(result), "UInt16");
        }

        private void byteBtn_Click(object sender, EventArgs e) {
            byte result;
            if( !byte.TryParse(ValueTextBox.Text,out result) ) return;
            addParam(new Parameter(result), "Byte");
        }


        private void UInt32Btn_Click(object sender, EventArgs e) {
            uint result;
            if (!uint.TryParse(ValueTextBox.Text, out result)) return;
            addParam(new Parameter(result), "UInt32");
        }

        private void StringBtn_Click(object sender, EventArgs e) {
            if (ValueTextBox.Text.Length==0 ) return;
            addParam(new Parameter(ValueTextBox.Text), "String");
        }

        private void Int16Btn_Click(object sender, EventArgs e) {
            short result;
            if (!short.TryParse(ValueTextBox.Text, out result)) return;
            addParam(new Parameter(result), "Int16");
        }

        private void Int32Btn_Click(object sender, EventArgs e) {
            int result;
            if (!int.TryParse(ValueTextBox.Text, out result)) return;
            addParam(new Parameter(result), "Int32");
        }

        private void button8_Click(object sender, EventArgs e) {
            listParamView.Items.Clear();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {

        }
    }
}
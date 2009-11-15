using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Simulator.Server;
using Game.Fighting;

namespace Simulator {
    public partial class RemoteView : Form {
        int port;
        RemoteServer remote_server;
        BattleRegulator br;
        public RemoteView(int port, bool isServer,Troop troop) {
            InitializeComponent();
            if( isServer ) {
                remote_server = new RemoteServer();
                this.port = port;
                remote_server.Port = this.port;
                remote_server.start();
                br = new BattleRegulator();
                br.start(port,troop);
            }
      
        }

        private void RemoteView_FormClosing(object sender, FormClosingEventArgs e) {
            remote_server.stop();
        }
    }
}
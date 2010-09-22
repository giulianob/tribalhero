using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Simulator.Client;
using Game.Fighting;

namespace Simulator {
    class RegulatorEventSink : AbstractBroadcastedMessageEventSink {

        public event RemoteClientJoinedCallback RemoteJoined;

        protected override void internalcallback(string str) {
            //throw new Exception("The method or operation is not implemented.");
        }

        protected override void remoteClientJoinedCallback(Game.Fighting.Troop troop) {
            if (RemoteJoined != null) RemoteJoined(troop);
        }
    }

    class BattleRegulator {
        TcpChannel m_TcpChan;
        RegulatorEventSink sink;
        Battle battle;
        Troop atk;

        internal void start(int port, Troop troop) {
            m_TcpChan = new TcpChannel(1012);
            ChannelServices.RegisterChannel(m_TcpChan, false);
            AbstractServer m_RemoteObject = (AbstractServer)
              Activator.GetObject(typeof(AbstractServer),
              "tcp://localhost:"+port+"/FirstRemote");

            RemotingConfiguration.RegisterWellKnownServiceType(
              typeof(EventSink),
              "ServerEvents",
              WellKnownObjectMode.Singleton);
            battle = new Battle();
            atk = troop;

            // Battle View still need to register here


            // regulator register here
            sink = new RegulatorEventSink();
   			m_RemoteObject.myevent += new myeventhandler(sink.myCallback);

            sink.RemoteJoined += new RemoteClientJoinedCallback(sink_RemoteJoined);

        }

        public void sink_RemoteJoined(Troop troop) {
            battle.start(atk, troop);
        }
    }
}

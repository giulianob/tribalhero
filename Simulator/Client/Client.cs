using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Simulator.Client {

    class EventSink : AbstractBroadcastedMessageEventSink {
        
        protected override void internalcallback(string str) {
            Console.WriteLine("Your message in callback ");
        }

        protected override void remoteClientJoinedCallback(Game.Fighting.Troop troop) {
            
        }
    }

    class Client {
        TcpChannel m_TcpChan;
        string target = "localhost";
        void start() {
            m_TcpChan = new TcpChannel(1011);
            ChannelServices.RegisterChannel(m_TcpChan,false);
            AbstractServer m_RemoteObject = (AbstractServer)
              Activator.GetObject(typeof(AbstractServer),
              "tcp://"+target+"/FirstRemote");

            RemotingConfiguration.RegisterWellKnownServiceType(
              typeof(EventSink),
              "ServerEvents",
              WellKnownObjectMode.Singleton);

            EventSink sink = new EventSink();
            Console.WriteLine("Subscribing");
            m_RemoteObject.myevent += new myeventhandler(sink.myCallback);
            m_RemoteObject.myfunc("Hello");
        }
    }
}

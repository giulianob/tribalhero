using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;
namespace Simulator.Server {

    public class RemoteServer {
        TcpChannel m_TcpChan;
        int port;
        public int Port {
            get { return port; }
            set { port = value; }
        }

        public void start() {
            BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
            serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

            BinaryClientFormatterSinkProvider clientProv = new BinaryClientFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = 1234;

            HttpChannel chan = new HttpChannel(props, clientProv, serverProv);
            ChannelServices.RegisterChannel(chan,false);


            m_TcpChan = new TcpChannel(port); //open a channel
        //    ChannelServices.RegisterChannel(m_TcpChan);
            Type theType = new ServerClass().GetType();
            RemotingConfiguration.RegisterWellKnownServiceType(
              theType,
              "FirstRemote",
              WellKnownObjectMode.Singleton);

//            System.Console.WriteLine("Press ENTER to quit");
 //           System.Console.ReadLine();
        }

       public void stop() {
            ChannelServices.UnregisterChannel(m_TcpChan);
        }
    }

    //component implementation

    public class ServerClass : AbstractServer {
        
        public override string myfunc(string what) {
            Console.WriteLine("in myfunc");
            FireNewBroadcastedMessageEvent("Event: " + what + " was said");
            return "done";
        }

        public event myeventhandler myHandler;

        public override event myeventhandler myevent {
            add {
                Console.WriteLine("in event myevent + add");

                myHandler = value;
            }
            remove {
                Console.WriteLine("in event myevent + remove");
            }
        }

        protected void FireNewBroadcastedMessageEvent(string text) {
            Console.WriteLine("Broadcasting...");
            myHandler("hai");
        }

        public event RemoteClientJoinedCallback LocalRemoteClientJoined;

        public override event RemoteClientJoinedCallback RemoteClientJoined {
            add { LocalRemoteClientJoined = value; }
            remove { LocalRemoteClientJoined = value; }
        }

        public override void join(Game.Fighting.Troop troop) {
            this.LocalRemoteClientJoined(troop);
        }
    }


}

using System;
using System.Collections.Generic;
using System.Text;
using Game.Fighting;

namespace Simulator {
    public delegate void myeventhandler(string str);
    public delegate void RemoteClientJoinedCallback(Troop troop);
    //component defenition is as below


    public abstract class AbstractServer : MarshalByRefObject {
        public abstract void join(Troop troop);

        public abstract string myfunc(string what);
        public abstract event myeventhandler myevent;

        public abstract event RemoteClientJoinedCallback RemoteClientJoined;
    }


    public abstract class AbstractBroadcastedMessageEventSink : MarshalByRefObject {

        public void remoteClientJoined(Troop troop) {
            remoteClientJoinedCallback(troop);
        }

        public void myCallback(string str) {
            internalcallback(str);
        }
        protected abstract void internalcallback(string str);
        protected abstract void remoteClientJoinedCallback(Troop troop);
    }

}

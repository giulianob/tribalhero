using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Util;

namespace Game.Comm {
    public partial class Processor {
        public void EventOnConnect(Session session, Packet packet) {
        }

        public void EventOnDisconnect(Session session, Packet packet) {
            if (session.Player == null) return;
            using (new MultiObjectLock(session.Player)) {
                Global.Channel.Unsubscribe(session);
            }
        }
    }
}

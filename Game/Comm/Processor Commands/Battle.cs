using System;
using System.Collections.Generic;
using System.Text;
using Game.Battle;
using Game.Util;
using Game.Setup;

namespace Game.Comm {
    public partial class Processor {
        public void CmdBattleSubscribe(Session session, Packet packet) {
            uint cityId;
            City city;
            try {
                cityId = packet.getUInt32();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(cityId, out city)) {
                if (city == null || city.Battle == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }
                Packet reply = new Packet(packet);
                PacketHelper.AddToPacket(city.Battle.Attacker, reply);
                PacketHelper.AddToPacket(city.Battle.Defender, reply);
                city.Battle.subscribe(session);
                session.write(reply);
            }
        }

        void session_OnClose(Session session) {
            
        }

        public void CmdBattleUnsubscribe(Session session, Packet packet) {
            uint cityId;
            City city;
            try {
                cityId = packet.getUInt32();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(cityId, out city)) {
                if (city == null || city.Battle == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }
                if (city.Battle == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                city.Battle.unsubscribe(session);
            }

            reply_success(session, packet);
        }
    }
}

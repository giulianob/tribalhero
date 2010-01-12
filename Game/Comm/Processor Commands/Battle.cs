#region

using System;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

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
                reply.addUInt16(city.Battle.Stamina);
                PacketHelper.AddToPacket(city.Battle.Attacker, reply);
                PacketHelper.AddToPacket(city.Battle.Defender, reply);
                city.Battle.Subscribe(session);
                session.write(reply);
            }
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

                city.Battle.Unsubscribe(session);
            }

            reply_success(session, packet);
        }
    }
}
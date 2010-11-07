#region

using System;
using System.Collections.Generic;
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
                cityId = packet.GetUInt32();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            if (!Global.World.TryGetObjects(cityId, out city)) {
                ReplyError(session, packet, Error.CITY_NOT_FOUND);
            }

            CallbackLock.CallbackLockHandler lockHandler = delegate {
                List<ILockable> toBeLocked = new List<ILockable> {
                                                                     session.Player
                                                                 };

                if (city.Battle != null)
                    toBeLocked.AddRange(city.Battle.LockList);

                return toBeLocked.ToArray();
            };   

            using (new CallbackLock(lockHandler, null, city)) {
                if (city.Battle == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                int roundsLeft;
                if (!Config.battle_instant_watch && !city.Battle.CanWatchBattle(session.Player, out roundsLeft)) {
                    packet = ReplyError(session, packet, Error.BATTLE_NOT_VIEWABLE, false);
                    packet.AddInt32(roundsLeft);
                    session.Write(packet);
                    return;
                }

                Packet reply = new Packet(packet);
                reply.AddUInt16(city.Battle.Stamina);
                PacketHelper.AddToPacket(city.Battle.Attacker, reply);
                PacketHelper.AddToPacket(city.Battle.Defender, reply);
                city.Battle.Subscribe(session);
                session.Write(reply);
            }
        }

        public void CmdBattleUnsubscribe(Session session, Packet packet) {
            uint cityId;
            City city;
            try {
                cityId = packet.GetUInt32();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(cityId, out city)) {
                if (city == null || city.Battle == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (city.Battle == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                city.Battle.Unsubscribe(session);
            }

            ReplySuccess(session, packet);
        }
    }
}
#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        public void CmdBattleSubscribe(Session session, Packet packet)
        {
            uint cityId;
            City city;
            try
            {
                cityId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (!Global.World.TryGetObjects(cityId, out city))
                ReplyError(session, packet, Error.CityNotFound);

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    var toBeLocked = new List<ILockable> {session.Player};

                    if (city.Battle != null)
                        toBeLocked.AddRange(city.Battle.LockList);

                    return toBeLocked.ToArray();
                };

            using (Ioc.Kernel.Get<CallbackLock>().Lock(lockHandler, null, city))
            {
                if (city.Battle == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                int roundsLeft;
                if (!Config.battle_instant_watch && !city.Battle.CanWatchBattle(session.Player, out roundsLeft))
                {
                    packet = ReplyError(session, packet, Error.BattleNotViewable, false);
                    packet.AddInt32(roundsLeft);
                    session.Write(packet);
                    return;
                }

                var reply = new Packet(packet);
                PacketHelper.AddToPacket(city.Battle.Attacker, reply);
                PacketHelper.AddToPacket(city.Battle.Defender, reply);
                city.Battle.Subscribe(session);
                session.Write(reply);
            }
        }

        public void CmdBattleUnsubscribe(Session session, Packet packet)
        {
            uint cityId;
            City city;
            try
            {
                cityId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city == null || city.Battle == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                city.Battle.Unsubscribe(session);
            }

            ReplySuccess(session, packet);
        }
    }
}
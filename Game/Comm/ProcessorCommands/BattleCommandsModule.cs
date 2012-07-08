#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class BattleCommandsModule : CommandModule
    {
        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.BattleSubscribe, Subscribe);
            processor.RegisterCommand(Command.BattleUnsubscribe, Unsubscribe);
        }

        private void Subscribe(Session session, Packet packet)
        {
            uint cityId;
            ICity city;
            try
            {
                cityId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (!World.Current.TryGetObjects(cityId, out city))
                ReplyError(session, packet, Error.CityNotFound);

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    var toBeLocked = new List<ILockable> {session.Player};

                    if (city.Battle != null)
                        toBeLocked.AddRange(city.Battle.LockList);

                    return toBeLocked.ToArray();
                };

            using (Concurrency.Current.Lock(lockHandler, null, city))
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
                reply.AddUInt32(city.Battle.BattleId);
                reply.AddUInt32(city.Battle.Round);
                PacketHelper.AddToPacket(city.Battle.Attackers, reply);
                PacketHelper.AddToPacket(city.Battle.Defender, reply);

                // TODO: This used to be in the battle manager but it doesnt belong there
                //  so I put it in here for now but it should not be here either. Need to make some other place
                //  that takes care of the battle channel stuff
                try
                {
                    Global.Channel.Subscribe(session, "/BATTLE/" + city.Id);
                }
                catch (DuplicateSubscriptionException)
                {
                }
                
                session.Write(reply);
            }
        }

        private void Unsubscribe(Session session, Packet packet)
        {
            uint cityId;
            ICity city;
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
                    ReplySuccess(session, packet);
                    return;
                }

                // TODO: See comment for subscribe. Applies here too.
                Global.Channel.Unsubscribe(session, "/BATTLE/" + city.Id);
            }

            ReplySuccess(session, packet);
        }
    }
}
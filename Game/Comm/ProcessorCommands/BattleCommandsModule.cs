#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
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
            uint battleId;
            try
            {
                battleId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            IBattleManager battleManager;
            if (!World.Current.TryGetObjects(battleId, out battleManager))
            {
                ReplyError(session, packet, Error.BattleNotViewable);
                return;
            }

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    var toBeLocked = new List<ILockable>();

                    toBeLocked.AddRange(battleManager.LockList);

                    return toBeLocked.ToArray();
                };

            using (Concurrency.Current.Lock(lockHandler, null, session.Player))
            {
                int roundsLeft;
                if (!Config.battle_instant_watch && !battleManager.CanWatchBattle(session.Player, out roundsLeft))
                {
                    packet = ReplyError(session, packet, Error.BattleNotViewable, false);
                    packet.AddInt32(roundsLeft);
                    session.Write(packet);
                    return;
                }
               
                var reply = new Packet(packet);
                reply.AddUInt32(battleManager.BattleId);
                reply.AddByte((byte)battleManager.Location.Type);
                reply.AddUInt32(battleManager.Location.Id);
                reply.AddString(battleManager.Location.GetName());
                reply.AddUInt32(battleManager.Round);
                PacketHelper.AddToPacket(battleManager.Attackers, reply);
                PacketHelper.AddToPacket(battleManager.Defenders, reply);
                
                try
                {
                    Global.Channel.Subscribe(session, "/BATTLE/" + battleManager.BattleId);
                }
                catch (DuplicateSubscriptionException)
                {
                }
                
                session.Write(reply);
            }
        }

        private void Unsubscribe(Session session, Packet packet)
        {
            uint battleId;
            try
            {
                battleId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Concurrency.Current.Lock(session.Player))
            {
                Global.Channel.Unsubscribe(session, "/BATTLE/" + battleId);
            }

            ReplySuccess(session, packet);
        }
    }
}
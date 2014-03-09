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
        private readonly ILocker locker;

        private readonly IGameObjectLocator world;

        public BattleCommandsModule(ILocker locker, IGameObjectLocator world)
        {
            this.locker = locker;
            this.world = world;
        }

        public override void RegisterCommands(IProcessor processor)
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
            if (!world.TryGetObjects(battleId, out battleManager))
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

            locker.Lock(lockHandler, null, session.Player).Do(() =>
            {
                IEnumerable<string> errorParams;
                var canWatchBattle = battleManager.CanWatchBattle(session.Player, out errorParams);
                if (!Config.battle_instant_watch && canWatchBattle != Error.Ok)
                {
                    packet = ReplyError(session, packet, canWatchBattle, false);
                    packet.AddByte((byte)errorParams.Count());
                    foreach (var errorParam in errorParams)
                    {
                        packet.AddString(errorParam);
                    }
                    session.Write(packet);
                    return;
                }

                var reply = new Packet(packet);
                reply.AddByte((byte)battleManager.Location.Type);
                reply.AddUInt32(battleManager.Location.Id);
                reply.AddString(battleManager.Location.GetName());
                reply.AddUInt32(battleManager.Round);

                // Battle properties
                PacketHelper.AddBattleProperties(battleManager.ListProperties(), reply);
                PacketHelper.AddToPacket(battleManager.Attackers, reply);
                PacketHelper.AddToPacket(battleManager.Defenders, reply);

                try
                {
                    Global.Current.Channel.Subscribe(session, "/BATTLE/" + battleManager.BattleId);
                }
                catch(DuplicateSubscriptionException)
                {
                }

                session.Write(reply);
            });
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

            locker.Lock(session.Player).Do(() =>
            {
                Global.Current.Channel.Unsubscribe(session, "/BATTLE/" + battleId);
            });

            ReplySuccess(session, packet);
        }
    }
}
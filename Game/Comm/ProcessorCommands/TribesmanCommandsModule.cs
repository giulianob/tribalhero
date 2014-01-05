#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Logic.Actions;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class TribesmanCommandsModule : CommandModule
    {
        private readonly IActionFactory actionFactory;

        private readonly IDbManager dbManager;

        private readonly ILocker locker;

        private readonly IStrongholdManager strongholdManager;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly ITribeManager tribeManager;

        private readonly IWorld world;

        public TribesmanCommandsModule(IActionFactory actionFactory,
                                       IStructureCsvFactory structureCsvFactory,
                                       ILocker locker,
                                       IWorld world,
                                       IDbManager dbManager,
                                       IStrongholdManager strongholdManager,
                                       ITribeManager tribeManager)
        {
            this.actionFactory = actionFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.locker = locker;
            this.world = world;
            this.dbManager = dbManager;
            this.strongholdManager = strongholdManager;
            this.tribeManager = tribeManager;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.TribesmanSetRank, SetRank);
            processor.RegisterCommand(Command.TribesmanRemove, Remove);
            processor.RegisterCommand(Command.TribesmanRequest, Request);
            processor.RegisterCommand(Command.TribesmanConfirm, Confirm);
            processor.RegisterCommand(Command.TribesmanLeave, Leave);
            processor.RegisterCommand(Command.TribesmanContribute, Contribute);
            processor.RegisterCommand(Command.TribeTransfer, Transfer);
        }

        public void SetRank(Session session, Packet packet)
        {
            uint playerId;
            byte rank;
            try
            {
                playerId = packet.GetUInt32();
                rank = packet.GetByte();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            Dictionary<uint, IPlayer> players;
            locker.Lock(out players, playerId, session.Player.Tribesman.Tribe.Owner.PlayerId).Do(() =>
            {
                ITribe tribe = session.Player.Tribesman.Tribe;
                if (!tribe.HasRight(session.Player.PlayerId, TribePermission.SetRank))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                var error = tribe.SetRank(playerId, rank);
                if (error == Error.Ok)
                {
                    ReplySuccess(session, packet);
                }
                else
                {
                    ReplyError(session, packet, error);
                }
            });
        }

        public void Transfer(Session session, Packet packet)
        {
            string newOwnerName;

            try
            {
                newOwnerName = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            uint newOwnerPlayerId;
            if (!world.FindPlayerId(newOwnerName, out newOwnerPlayerId))
            {
                ReplyError(session, packet, Error.PlayerNotFound);
                return;
            }

            Dictionary<uint, IPlayer> players;
            locker.Lock(out players, newOwnerPlayerId, session.Player.Tribesman.Tribe.Owner.PlayerId).Do(() =>
            {
                if (players == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                ITribe tribe = session.Player.Tribesman.Tribe;
                if (!tribe.IsOwner(session.Player))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                var result = tribe.Transfer(newOwnerPlayerId);

                ReplyWithResult(session, packet, result);
            });
        }

        public void Request(Session session, Packet packet)
        {
            string playerName;
            try
            {
                playerName = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            uint playerId;
            if (!world.FindPlayerId(playerName, out playerId))
            {
                ReplyError(session, packet, Error.PlayerNotFound);
                return;
            }

            Dictionary<uint, IPlayer> players;
            ITribe tribe = session.Player.Tribesman.Tribe;
            locker.Lock(out players, playerId, tribe.Owner.PlayerId).Do(() =>
            {
                if (!tribe.HasRight(session.Player.PlayerId, TribePermission.Invite))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }
                if (players[playerId].Tribesman != null)
                {
                    ReplyError(session, packet, Error.TribesmanAlreadyInTribe);
                    return;
                }
                if (players[playerId].TribeRequest != 0)
                {
                    ReplyError(session, packet, Error.TribesmanPendingRequest);
                    return;
                }

                players[playerId].TribeRequest = tribe.Id;
                dbManager.Save(players[playerId]);
                ReplySuccess(session, packet);
            });
        }

        public void Confirm(Session session, Packet packet)
        {
            bool isAccepting;
            try
            {
                isAccepting = packet.GetByte() != 0;
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            ITribe tribe = null;

            locker.Lock(session.Player).Do(() =>
            {
                if (session.Player.TribeRequest == 0)
                {
                    ReplyError(session, packet, Error.TribesmanNoRequest);
                    return;
                }

                var tribeRequestId = session.Player.TribeRequest;

                session.Player.TribeRequest = 0;
                dbManager.Save(session.Player);

                if (!isAccepting)
                {
                    ReplySuccess(session, packet);
                    return;
                }

                if (!world.TryGetObjects(tribeRequestId, out tribe))
                {
                    ReplyError(session, packet, Error.TribeNotFound);
                }
            });

            if (tribe == null)
            {
                return;
            }

            locker.Lock(session.Player, tribe).Do(() =>
            {
                if (tribe == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var tribesman = new Tribesman(tribe, session.Player, tribe.DefaultRank);

                var error = tribe.AddTribesman(tribesman);

                if (error != Error.Ok)
                {
                    ReplyError(session, packet, error);
                    return;
                }

                var reply = new Packet(packet);
                reply.AddInt32(tribeManager.GetIncomingList(tribe).Count());
                reply.AddInt16(tribe.AssignmentCount);
                PacketHelper.AddTribeRanksToPacket(tribe, reply);
                session.Write(reply);
            });
        }

        public void Remove(Session session, Packet packet)
        {
            uint playerId;
            try
            {
                playerId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            IPlayer playerToBeRemoved;
            if (!world.TryGetObjects(playerId, out playerToBeRemoved))
            {
                ReplyError(session, packet, Error.PlayerNotFound);
                return;
            }

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    var tribe = session.Player.IsInTribe ? session.Player.Tribesman.Tribe : null;

                    if (tribe == null)
                    {
                        return new ILockable[] {};
                    }

                    var locks =
                            strongholdManager.StrongholdsForTribe(tribe)
                                             .SelectMany(stronghold => stronghold.LockList)
                                             .ToList();

                    locks.Add(tribe);

                    return locks.ToArray();
                };

            locker.Lock(lockHandler, new object[] {}, session.Player, playerToBeRemoved).Do(() =>
            {
                if (!session.Player.IsInTribe || !playerToBeRemoved.IsInTribe ||
                    playerToBeRemoved.Tribesman.Tribe != session.Player.Tribesman.Tribe)
                {
                    ReplyError(session, packet, Error.TribeIsNull);
                    return;
                }

                ITribe tribe = session.Player.Tribesman.Tribe;
                if (!tribe.HasRight(session.Player.PlayerId, TribePermission.Kick))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                if (tribe.IsOwner(playerToBeRemoved))
                {
                    ReplyError(session, packet, Error.TribesmanIsOwner);
                    return;
                }

                var result = session.Player.Tribesman.Tribe.KickTribesman(playerToBeRemoved, session.Player);

                ReplyWithResult(session, packet, result);
            });
        }

        public void Leave(Session session, Packet packet)
        {
            var tribe = session.Player.Tribesman == null ? null : session.Player.Tribesman.Tribe;

            if (tribe == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    var locks =
                            strongholdManager.StrongholdsForTribe(tribe)
                                             .SelectMany(stronghold => stronghold.LockList)
                                             .ToList();

                    return locks.ToArray();
                };

            locker.Lock(lockHandler, new object[] {}, tribe, session.Player).Do(() =>
            {
                if (session.Player.Tribesman == null || session.Player.Tribesman.Tribe != tribe)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var result = tribe.LeaveTribesman(session.Player);

                ReplyWithResult(session, packet, result);
            });
        }

        public void Contribute(Session session, Packet packet)
        {
            uint cityId;
            uint structureId;
            Resource resource;
            try
            {
                cityId = packet.GetUInt32();
                structureId = packet.GetUInt32();
                resource = new Resource(packet.GetInt32(), packet.GetInt32(), packet.GetInt32(), packet.GetInt32(), 0);
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            var tribe = session.Player.Tribesman != null ? session.Player.Tribesman.Tribe : null;

            if (tribe == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            locker.Lock(tribe, session.Player).Do(() =>
            {
                if (session.Player.Tribesman == null || session.Player.Tribesman.Tribe != tribe)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                ICity city = session.Player.GetCity(cityId);
                IStructure structure;

                if (city == null || !city.TryGetStructure(structureId, out structure))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var action = actionFactory.CreateTribeContributeActiveAction(cityId, structureId, resource);
                Error ret = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(structure),
                                                 structure,
                                                 action,
                                                 structure.Technologies);
                ReplyWithResult(session, packet, ret);
            });
        }
    }
}
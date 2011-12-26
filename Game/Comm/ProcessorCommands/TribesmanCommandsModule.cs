#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Tribe;
using Game.Database;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;
using Ninject;
using Persistance;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class TribesmanCommandsModule : CommandModule
    {
        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.TribesmanSetRank, SetRank);
            processor.RegisterCommand(Command.TribesmanAdd, Add);
            processor.RegisterCommand(Command.TribesmanRemove, Remove);
            processor.RegisterCommand(Command.TribesmanUpdate, Update);
            processor.RegisterCommand(Command.TribesmanRequest, Request);
            processor.RegisterCommand(Command.TribesmanConfirm, Confirm);
            processor.RegisterCommand(Command.TribesmanLeave, Leave);
            processor.RegisterCommand(Command.TribesmanContribute, Contribute);            
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
            catch (Exception)
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
            using (Concurrency.Current.Lock(out players, playerId, session.Player.Tribesman.Tribe.Owner.PlayerId))
            {
                ITribe tribe = session.Player.Tribesman.Tribe;
                if (!tribe.IsOwner(session.Player))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                var error = tribe.SetRank(playerId, rank);
                if (error == Error.Ok)
                    ReplySuccess(session, packet);
                else
                    ReplyError(session, packet, error);
            }
        }

        public void Request(Session session, Packet packet)
        {
            string playerName;
            try
            {
                playerName = packet.GetString();
            }
            catch (Exception)
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
            if (!World.Current.FindPlayerId(playerName, out playerId))
            {
                ReplyError(session, packet, Error.PlayerNotFound);
                return;
            }

            Dictionary<uint, IPlayer> players;
            ITribe tribe = session.Player.Tribesman.Tribe;
            using (Concurrency.Current.Lock(out players, playerId, tribe.Owner.PlayerId))
            {
                if (!tribe.HasRight(session.Player.PlayerId, "Request"))
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
                DbPersistance.Current.Save(players[playerId]);
                ReplySuccess(session, packet);
            }

        }

        public void Confirm(Session session, Packet packet)
        {
            bool isAccepting;
            try
            {
                isAccepting = packet.GetByte() != 0;
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            ITribe tribe;

            using (Concurrency.Current.Lock(session.Player))
            {
                if (session.Player.TribeRequest == 0)
                {
                    ReplyError(session, packet, Error.TribesmanNoRequest);
                    return;
                }

                var tribeRequestId = session.Player.TribeRequest;

                session.Player.TribeRequest = 0;
                DbPersistance.Current.Save(session.Player);

                if (!isAccepting)
                {
                    ReplySuccess(session, packet);
                    return;
                }

                if (!Global.Tribes.TryGetValue(tribeRequestId, out tribe))
                {
                    ReplyError(session, packet, Error.TribeNotFound);
                    return;
                }
            }

            using (Concurrency.Current.Lock(session.Player, tribe))
            {
                var tribesman = new Tribesman(tribe, session.Player, 2);
                var error = tribe.AddTribesman(tribesman);
                if (error != Error.Ok)
                {
                    ReplyError(session, packet, error);
                    return;
                }

                var reply = new Packet(packet);
                Global.Channel.Subscribe(session, "/TRIBE/" + tribe.Id);
                reply.AddInt32(tribe.GetIncomingList().Count());
                reply.AddInt16(tribe.AssignmentCount);
                session.Write(reply);
            }
        }

        public void Add(Session session, Packet packet)
        {
            uint playerId;
            try
            {
                playerId = packet.GetUInt32();
            }
            catch (Exception)
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
            using (Concurrency.Current.Lock(out players, playerId, session.Player.Tribesman.Tribe.Owner.PlayerId))
            {
                var tribesman = new Tribesman(session.Player.Tribesman.Tribe, players[playerId], 2);
                session.Player.Tribesman.Tribe.AddTribesman(tribesman);
                packet.AddInt32(session.Player.Tribesman.Tribe.GetIncomingList().Count());
                packet.AddInt16(session.Player.Tribesman.Tribe.AssignmentCount);
                ReplySuccess(session, packet);
            }
        }

        public void Remove(Session session, Packet packet)
        {
            uint playerId;
            try
            {
                playerId = packet.GetUInt32();
            }
            catch (Exception)
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
            using (Concurrency.Current.Lock(out players, playerId, session.Player.Tribesman.Tribe.Owner.PlayerId))
            {
                if (!players.ContainsKey(playerId))
                {
                    ReplyError(session, packet, Error.PlayerNotFound);
                    return;
                }

                ITribe tribe = session.Player.Tribesman.Tribe;
                if (!tribe.HasRight(session.Player.PlayerId, "Kick"))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }
                if (tribe.IsOwner(players[playerId]))
                {
                    ReplyError(session, packet, Error.TribesmanIsOwner);
                    return;
                }
                session.Player.Tribesman.Tribe.RemoveTribesman(playerId);
                Procedure.Current.OnSessionTribesmanQuit(players[playerId].Session, tribe.Id, playerId,true);
                ReplySuccess(session, packet);
            }
        }

        private void Update(Session session, Packet packet)
        {
        }

        public void Leave(Session session, Packet packet)
        {
            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            using (Concurrency.Current.Lock(session.Player.Tribesman.Tribe, session.Player))
            {
                ITribe tribe = session.Player.Tribesman.Tribe;

                if (tribe.IsOwner(session.Player))
                {
                    ReplyError(session, packet, Error.TribesmanIsOwner);
                    return;
                }
                tribe.RemoveTribesman(session.Player.PlayerId);
                Procedure.Current.OnSessionTribesmanQuit(session, tribe.Id, session.Player.PlayerId,false);
                ReplySuccess(session, packet);
            }
        }

        public void Contribute(Session session, Packet packet)
        {
            uint cityId;
            Resource resource;
            try
            {
                cityId = packet.GetUInt32();
                resource = new Resource(packet.GetInt32(), packet.GetInt32(), packet.GetInt32(), packet.GetInt32(), 0);
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

            using (Concurrency.Current.Lock(session.Player.Tribesman.Tribe, session.Player))
            {
                ICity city = session.Player.GetCity(cityId);
                ITribe tribe = session.Player.Tribesman.Tribe;

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                if (!city.Resource.HasEnough(resource))
                {
                    ReplyError(session, packet, Error.ResourceNotEnough);
                    return;
                }

                Error error = tribe.Contribute(session.Player.PlayerId, resource);
                if (error != Error.Ok)
                {
                    ReplyError(session, packet, error);
                    return;
                }
                city.BeginUpdate();
                city.Resource.Subtract(resource);
                city.EndUpdate();
                ReplySuccess(session, packet);
            }
        }

    }
}
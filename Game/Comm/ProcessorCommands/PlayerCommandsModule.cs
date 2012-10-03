#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using Game.Data;
using Game.Database;
using Game.Logic.Actions;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;
using Ninject;
using Persistance;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class PlayerCommandsModule : CommandModule
    {
        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.PlayerProfile, ViewProfile);
            processor.RegisterCommand(Command.PlayerDescriptionSet, SetDescription);
            processor.RegisterCommand(Command.PlayerUsernameGet, GetUsername);
            processor.RegisterCommand(Command.PlayerNameFromCityName, GetCityOwnerName);
            processor.RegisterCommand(Command.CityResourceSend, SendResources);                      
        }

        private void SetDescription(Session session, Packet packet)
        {
            string description;
            try
            {
                description = packet.GetString();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }
            
            using (Concurrency.Current.Lock(session.Player))
            {
                if (description.Length > Player.MAX_DESCRIPTION_LENGTH)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                session.Player.Description = description;

                ReplySuccess(session, packet);
            }
        }

        private void ViewProfile(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            uint playerId;
            string playerName = string.Empty;
            try
            {
                playerId = packet.GetUInt32();
                if (playerId == 0)
                    playerName = packet.GetString();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (playerId == 0)
            {
                if (!World.Current.FindPlayerId(playerName, out playerId))
                {
                    ReplyError(session, packet, Error.PlayerNotFound);
                    return;
                }
            }

            IPlayer player;
            using (Concurrency.Current.Lock(playerId, out player))
            {
                if (player == null)
                {
                    ReplyError(session, reply, Error.PlayerNotFound);
                    return;
                }

                PacketHelper.AddPlayerProfileToPacket(player, reply);

                session.Write(reply);
            }
        }

        private void GetUsername(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            byte count;
            uint[] playerIds;
            try
            {
                count = packet.GetByte();
                playerIds = new uint[count];
                for (int i = 0; i < count; i++)
                    playerIds[i] = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            reply.AddByte(count);
            foreach (var playerId in playerIds)
            {
                IPlayer player;
                if (!World.Current.Players.TryGetValue(playerId, out player))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt32(player.PlayerId);
                reply.AddString(player.Name);
            }

            session.Write(reply);
        }

        private void GetCityOwnerName(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            string cityName;
            try
            {
                cityName = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            uint cityId;
            if (!World.Current.Cities.FindCityId(cityName, out cityId))
            {
                ReplyError(session, packet, Error.CityNotFound);
                return;
            }

            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                reply.AddString(city.Owner.Name);
            }

            session.Write(reply);
        }

        private void SendResources(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            string targetCityName;
            Resource resource;
            bool actuallySend;

            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                targetCityName = packet.GetString().Trim();
                resource = new Resource(packet.GetInt32(), packet.GetInt32(), packet.GetInt32(), packet.GetInt32(), 0);
                actuallySend = packet.GetByte() == 1;
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            uint targetCityId;
            if (!World.Current.Cities.FindCityId(targetCityName, out targetCityId))
            {
                ReplyError(session, packet, Error.CityNotFound);
                return;
            }

            if (cityId == targetCityId)
            {
                ReplyError(session, packet, Error.ActionSelf);
                return;
            }

            using (Concurrency.Current.Lock(session.Player))
            {
                if (session.Player.GetCity(cityId) == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
            }

            Dictionary<uint, ICity> cities;
            using (Concurrency.Current.Lock(out cities, cityId, targetCityId))
            {
                if (cities == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                ICity city = cities[cityId];

                IStructure structure;
                if (!city.TryGetStructure(objectId, out structure))
                    ReplyError(session, packet, Error.Unexpected);

                var action = new ResourceSendActiveAction(cityId, objectId, targetCityId, resource);

                // If actually send then we perform the action, otherwise, we send the player information about the trade.
                if (actuallySend)
                {                    
                    Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(structure), structure, action, structure.Technologies);
                    if (ret != 0)
                        ReplyError(session, packet, ret);
                    else
                        ReplySuccess(session, packet);
                } else
                {
                    var reply = new Packet(packet);
                    reply.AddString(cities[targetCityId].Owner.Name);
                    reply.AddInt32(action.CalculateTradeTime(structure, cities[targetCityId]));

                    session.Write(reply);
                }
            }
        }
    }
}
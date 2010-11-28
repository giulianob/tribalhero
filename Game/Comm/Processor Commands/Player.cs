#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Actions;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Comm {
    public partial class Processor {
        public void CmdGetUsername(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            byte count;
            uint[] playerIds;
            try {
                count = packet.GetByte();
                playerIds = new uint[count];
                for (int i = 0; i < count; i++)
                    playerIds[i] = packet.GetUInt32();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            reply.AddByte(count);
            foreach (uint playerId in playerIds) {
                Player player;
                if (!Global.World.Players.TryGetValue(playerId, out player))
                {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                reply.AddUInt32(player.PlayerId);
                reply.AddString(player.Name);
            }

            session.Write(reply);
        }

        public void CmdGetCityOwnerName(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            string cityName;
            try {
                cityName = packet.GetString();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            uint cityId;
            if (!Global.World.FindCityId(cityName, out cityId)) {
                ReplyError(session, packet, Error.CITY_NOT_FOUND);
                return;
            }

            City city;
            using (new MultiObjectLock(cityId, out city)) {
                reply.AddString(city.Owner.Name);
            }

            session.Write(reply);
        }

        public void CmdSendResources(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            string targetCityName;
            Resource resource;

            try
            {                
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                targetCityName = packet.GetString();
                resource = new Resource(packet.GetInt32(), packet.GetInt32(), packet.GetInt32(), packet.GetInt32(), 0);
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            uint targetCityId;
            if (!Global.World.FindCityId(targetCityName, out targetCityId)) {
                ReplyError(session, packet, Error.CITY_NOT_FOUND);
                return;
            }


            if (cityId == targetCityId)
            {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player))
            {
                if (session.Player.GetCity(cityId) == null)
                {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }
            }

            Dictionary<uint, City> cities;
            using (new MultiObjectLock(out cities, cityId, targetCityId)) {
                if (cities == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                City city = cities[cityId];

                Structure structure;
                if (!city.TryGetStructure(objectId, out structure))
                    ReplyError(session, packet, Error.UNEXPECTED);

                ResourceSendAction action = new ResourceSendAction(cityId, objectId, targetCityId, resource);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(structure), structure, action, structure.Technologies);
                if (ret != 0) {
                    ReplyError(session, packet, ret);
                } else
                    ReplySuccess(session, packet);
            }
        }
    }
}
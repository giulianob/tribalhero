#region

using System;
using System.Collections;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;
using System.Linq;
using NDesk.Options;
using Ninject;
using Persistance;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        public void CmdAssignmentCreate(Session session, Packet packet) {
            uint cityId;
            uint targetCityId;
            uint targetObjectId;
            AttackMode mode;
            DateTime time;
            TroopStub stub;
            try
            {
                mode = (AttackMode)packet.GetByte();
                cityId = packet.GetUInt32();
                targetCityId = packet.GetUInt32();
                targetObjectId = packet.GetUInt32();
                time = DateTime.UtcNow.AddSeconds(packet.GetInt32());
                stub = PacketHelper.ReadStub(packet, FormationType.Attack);
            }
            catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            // First need to find all the objects that should be locked
            uint[] playerIds;
            Dictionary<uint, City> cities;
            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(out cities, cityId, targetCityId))
            {
                if (cities == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var city = cities[cityId];

                // Make sure city belongs to player and he is in a tribe
                if (city.Owner != session.Player || city.Owner.Tribesman == null) {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                City targetCity = cities[targetCityId];

                playerIds = new[] {city.Owner.PlayerId, city.Owner.Tribesman.Tribe.Owner.PlayerId, targetCity.Owner.PlayerId};
            }

            Dictionary<uint, Player> players;
            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(out players, playerIds)) {
                City city;
                City targetCity;
                if (players == null || !Global.World.TryGetObjects(cityId, out city) || !Global.World.TryGetObjects(targetCityId, out targetCity))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
                
                // Make sure this player is ranked high enough
                if (city.Owner.Tribesman == null || !city.Owner.Tribesman.Tribe.HasRight(city.Owner.PlayerId, "Assignment"))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                // Get target structure
                Structure targetStructure;
                if (!targetCity.TryGetStructure(targetObjectId, out targetStructure))
                {
                    ReplyError(session, packet, Error.ObjectStructureNotFound);
                    return;
                }

                // Create troop stub                                
                if (!Procedure.TroopStubCreate(city, stub, TroopState.WaitingInAssignment)) {
                    ReplyError(session, packet, Error.TroopChanged);
                    return;
                }

                Ioc.Kernel.Get<IDbManager>().Save(stub);

                int id;
                Error ret = session.Player.Tribesman.Tribe.CreateAssignment(stub, targetStructure.X, targetStructure.Y, targetCity, time, mode, out id);
                if (ret != 0) {
                    Procedure.TroopStubDelete(city, stub);
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            }
        }
 
        public void CmdAssignmentJoin(Session session, Packet packet) {
            uint cityId;
            int assignmentId;
            TroopStub stub;
            try
            {
                cityId = packet.GetUInt32();
                assignmentId = packet.GetInt32();
                stub = PacketHelper.ReadStub(packet, FormationType.Attack);
            }
            catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            Tribe tribe = session.Player.Tribesman.Tribe;
            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(session.Player, tribe)) {
                City city = session.Player.GetCity(cityId);
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                // Create stub
                if (!Procedure.TroopStubCreate(city, stub, TroopState.WaitingInAssignment))
                {
                    ReplyError(session, packet, Error.TroopChanged);
                    return;                        
                }

                Ioc.Kernel.Get<IDbManager>().Save(stub);

                Error error = tribe.JoinAssignment(assignmentId, stub);
                if (error != Error.Ok) {
                    Procedure.TroopStubDelete(city, stub);                    
                    ReplyError(session, packet, error);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            }
        }
    }
}
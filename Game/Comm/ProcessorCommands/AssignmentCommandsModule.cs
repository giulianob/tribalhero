#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Database;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class AssignmentCommandsModule : CommandModule
    {
        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.TribeAssignmentCreate, Create);
            processor.RegisterCommand(Command.TribeAssignmentJoin, Join);
        }

        private void Create(Session session, Packet packet) {
            uint cityId;
            uint targetCityId;
            uint targetObjectId;
            AttackMode mode;
            DateTime time;
            TroopStub stub;
            string description;
            try
            {
                mode = (AttackMode)packet.GetByte();
                cityId = packet.GetUInt32();
                targetCityId = packet.GetUInt32();
                targetObjectId = packet.GetUInt32();
                time = DateTime.UtcNow.AddSeconds(packet.GetInt32());
                stub = PacketHelper.ReadStub(packet, FormationType.Attack);
                description = packet.GetString();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            // First need to find all the objects that should be locked
            uint[] playerIds;
            Dictionary<uint, ICity> cities;
            using (Concurrency.Current.Lock(out cities, cityId, targetCityId))
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

                ICity targetCity = cities[targetCityId];

                // Make sure they are not in newbie protection
                if (Formula.Current.IsNewbieProtected(targetCity.Owner))
                {
                    ReplyError(session, packet, Error.PlayerNewbieProtection);
                    return;
                }

                playerIds = new[] {city.Owner.PlayerId, city.Owner.Tribesman.Tribe.Owner.PlayerId, targetCity.Owner.PlayerId};
            }

            Dictionary<uint, IPlayer> players;
            using (Concurrency.Current.Lock(out players, playerIds)) {
                ICity city;
                ICity targetCity;
                if (players == null || !World.Current.TryGetObjects(cityId, out city) || !World.Current.TryGetObjects(targetCityId, out targetCity))
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
                IStructure targetStructure;
                if (!targetCity.TryGetStructure(targetObjectId, out targetStructure))
                {
                    ReplyError(session, packet, Error.ObjectStructureNotFound);
                    return;
                }

                // Create troop stub                                
                if (!Procedure.Current.TroopStubCreate(city, stub, TroopState.WaitingInAssignment)) {
                    ReplyError(session, packet, Error.TroopChanged);
                    return;
                }

                DbPersistance.Current.Save(stub);

                int id;
                Error ret = session.Player.Tribesman.Tribe.CreateAssignment(stub, targetStructure.X, targetStructure.Y, targetCity, time, mode, description, out id);
                if (ret != 0) {
                    Procedure.Current.TroopStubDelete(city, stub);
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            }
        }
 
        private void Join(Session session, Packet packet) {
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

            ITribe tribe = session.Player.Tribesman.Tribe;
            using (Concurrency.Current.Lock(session.Player, tribe)) {
                ICity city = session.Player.GetCity(cityId);
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                // Create stub
                if (!Procedure.Current.TroopStubCreate(city, stub, TroopState.WaitingInAssignment))
                {
                    ReplyError(session, packet, Error.TroopChanged);
                    return;                        
                }

                DbPersistance.Current.Save(stub);

                Error error = tribe.JoinAssignment(assignmentId, stub);
                if (error != Error.Ok) {
                    Procedure.Current.TroopStubDelete(city, stub);                    
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
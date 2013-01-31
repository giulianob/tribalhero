#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class AssignmentCommandsModule : CommandModule
    {
        private readonly BattleProcedure battleProcedure;

        private readonly ICityManager cityManager;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        private readonly IStrongholdManager strongholdManager;

        public AssignmentCommandsModule(BattleProcedure battleProcedure,
                                        IGameObjectLocator gameObjectLocator,
                                        ILocker locker,
                                        IStrongholdManager strongholdManager,
                                        ICityManager cityManager)
        {
            this.battleProcedure = battleProcedure;
            this.gameObjectLocator = gameObjectLocator;
            this.locker = locker;
            this.strongholdManager = strongholdManager;
            this.cityManager = cityManager;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.TribeCityAssignmentCreate, CreateCityAssignment);
            processor.RegisterCommand(Command.TribeStrongholdAssignmentCreate, CreateStrongholdAssignment);
            processor.RegisterCommand(Command.TribeAssignmentJoin, Join);
        }

        private void CreateStrongholdAssignment(Session session, Packet packet)
        {
            uint cityId;
            uint strongholdId;
            AttackMode mode;
            DateTime time;
            ISimpleStub simpleStub;
            string description;
            bool isAttack;
            try
            {
                mode = (AttackMode)packet.GetByte();
                cityId = packet.GetUInt32();
                strongholdId = packet.GetUInt32();
                time = DateTime.UtcNow.AddSeconds(packet.GetInt32());
                isAttack = packet.GetByte() == 1;
                simpleStub = PacketHelper.ReadStub(packet, isAttack ? FormationType.Attack : FormationType.Defense);
                description = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(strongholdId, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            ICity city;
            if (!cityManager.TryGetCity(cityId, out city))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            // First need to find all the objects that should be locked
            uint[] playerIds;
            Dictionary<uint, ICity> cities;
            using (locker.Lock(city, stronghold))
            {
                if (city == null || stronghold == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                // Make sure city belongs to player and he is in a tribe
                if (city.Owner != session.Player || city.Owner.Tribesman == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                // Make sure this player is ranked high enough
                if (city.Owner.Tribesman == null ||
                    !city.Owner.Tribesman.Tribe.HasRight(city.Owner.PlayerId, "Assignment"))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                int id;
                Error ret = session.Player.Tribesman.Tribe.CreateAssignment(city,
                                                                            simpleStub,
                                                                            stronghold.X,
                                                                            stronghold.Y,
                                                                            stronghold,
                                                                            time,
                                                                            mode,
                                                                            description,
                                                                            isAttack,
                                                                            out id);
                ReplyWithResult(session, packet, ret);
            }
        }

        private void CreateCityAssignment(Session session, Packet packet)
        {
            uint cityId;
            uint targetCityId;
            uint targetObjectId;
            AttackMode mode;
            DateTime time;
            ISimpleStub simpleStub;
            string description;
            bool isAttack;
            try
            {
                mode = (AttackMode)packet.GetByte();
                cityId = packet.GetUInt32();
                targetCityId = packet.GetUInt32();
                targetObjectId = packet.GetUInt32();
                time = DateTime.UtcNow.AddSeconds(packet.GetInt32());
                isAttack = packet.GetByte() == 1;
                simpleStub = PacketHelper.ReadStub(packet, isAttack ? FormationType.Attack : FormationType.Defense);
                description = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            // First need to find all the objects that should be locked
            uint[] playerIds;
            Dictionary<uint, ICity> cities;
            using (locker.Lock(out cities, cityId, targetCityId))
            {
                if (cities == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var city = cities[cityId];

                // Make sure city belongs to player and he is in a tribe
                if (city.Owner != session.Player || city.Owner.Tribesman == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                ICity targetCity = cities[targetCityId];

                // Make sure they are not in newbie protection
                if (BattleProcedure.IsNewbieProtected(targetCity.Owner))
                {
                    ReplyError(session, packet, Error.PlayerNewbieProtection);
                    return;
                }

                playerIds = new[]
                {city.Owner.PlayerId, city.Owner.Tribesman.Tribe.Owner.PlayerId, targetCity.Owner.PlayerId};
            }

            Dictionary<uint, IPlayer> players;
            using (locker.Lock(out players, playerIds))
            {
                ICity city;
                ICity targetCity;
                if (players == null || !gameObjectLocator.TryGetObjects(cityId, out city) ||
                    !gameObjectLocator.TryGetObjects(targetCityId, out targetCity))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                // Get target structure
                IStructure targetStructure;
                if (!targetCity.TryGetStructure(targetObjectId, out targetStructure))
                {
                    ReplyError(session, packet, Error.ObjectStructureNotFound);
                    return;
                }

                int id;
                Error ret = session.Player.Tribesman.Tribe.CreateAssignment(city,
                                                                            simpleStub,
                                                                            targetStructure.X,
                                                                            targetStructure.Y,
                                                                            targetCity,
                                                                            time,
                                                                            mode,
                                                                            description,
                                                                            isAttack,
                                                                            out id);
                ReplyWithResult(session, packet, ret);
            }
        }

        private void Join(Session session, Packet packet)
        {
            uint cityId;
            int assignmentId;
            ISimpleStub stub;
            try
            {
                cityId = packet.GetUInt32();
                assignmentId = packet.GetInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            ITribe tribe = session.Player.Tribesman.Tribe;
            using (Concurrency.Current.Lock(session.Player, tribe))
            {
                ICity city = session.Player.GetCity(cityId);
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                // TODO: Clean this up
                Assignment assignment = tribe.Assignments.FirstOrDefault(x => x.Id == assignmentId);
                if (assignment == null)
                {
                    ReplyError(session, packet, Error.AssignmentDone);
                    return;
                }

                try
                {
                    stub = PacketHelper.ReadStub(packet,
                                                 assignment.IsAttack ? FormationType.Attack : FormationType.Defense);
                }
                catch(Exception)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                Error result = tribe.JoinAssignment(assignmentId, city, stub);

                ReplyWithResult(session, packet, result);
            }
        }
    }
}
#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stats;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class TroopCommandsModule : CommandModule
    {
        private readonly IActionFactory actionFactory;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly Formula formula;

        private readonly StructureFactory structureFactory;

        public TroopCommandsModule(IActionFactory actionFactory,
                                   StructureFactory structureFactory,
                                   IGameObjectLocator gameObjectLocator,
                                   Formula formula)
        {
            this.actionFactory = actionFactory;
            this.structureFactory = structureFactory;
            this.gameObjectLocator = gameObjectLocator;
            this.formula = formula;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.UnitTrain, TrainUnit);
            processor.RegisterCommand(Command.UnitUpgrade, UnitUpgrade);
            processor.RegisterCommand(Command.TroopInfo, GetTroopInfo);
            processor.RegisterCommand(Command.TroopAttackCity, AttackCity);
            processor.RegisterCommand(Command.TroopAttackStronghold, AttackStronghold);
            processor.RegisterCommand(Command.TroopDefendCity, DefendCity);
            processor.RegisterCommand(Command.TroopDefendStronghold, DefendStronghold);
            processor.RegisterCommand(Command.TroopRetreat, Retreat);
            processor.RegisterCommand(Command.TroopLocalSet, LocalTroopSet);
            processor.RegisterCommand(Command.TroopAttackBarbarianTribe, AttackBarbarianTribe);
            processor.RegisterCommand(Command.TroopModeSwitch, ModeSwitch);
        }

        private void ModeSwitch(Session session, Packet packet)
        {
            uint cityId;
            byte stubId;
            AttackMode mode;
            try
            {
                cityId = packet.GetUInt32();
                stubId = packet.GetByte();
                mode = (AttackMode)packet.GetByte();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            ICity city;
            IStation station;

            //we need to find out the stationed city first then reacquire local + stationed city locks            
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                ITroopStub stub;

                if (!city.Troops.TryGetStub(stubId, out stub) || stub.Station == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                station = stub.Station;
            }

            using (Concurrency.Current.Lock(city, station))
            {
                ITroopStub stub;

                if (!city.Troops.TryGetStub(stubId, out stub))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                if (stub.InitialCount == 0)
                {
                    ReplyError(session, packet, Error.NewFeature);
                    return;
                }

                if (stub.State != TroopState.Stationed)
                {
                    ReplyError(session, packet, Error.TroopNotStationed);
                    return;
                }

                stub.BeginUpdate();
                stub.RetreatCount = (ushort)formula.GetAttackModeTolerance(stub.InitialCount, mode);
                stub.AttackMode = mode;
                stub.EndUpdate();

                ReplySuccess(session, packet);
            }
        }

        private void GetTroopInfo(Session session, Packet packet)
        {
            ICity city;
            ITroopObject troop;

            uint cityId;
            uint objectId;

            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Concurrency.Current.Lock(cityId, objectId, out city, out troop))
            {
                if (city == null || troop == null || troop.Stub == null)
                {
                    ReplyError(session, packet, Error.ObjectNotFound);
                    return;
                }

                var reply = new Packet(packet);
                reply.AddByte(troop.Stub.TroopId);

                if (city.Owner == session.Player)
                {
                    reply.AddByte(troop.Stats.AttackRadius);
                    reply.AddFloat((float)troop.Stats.Speed);
                    reply.AddUInt32(troop.TargetX);
                    reply.AddUInt32(troop.TargetY);

                    var template = new UnitTemplate(city);

                    reply.AddByte(troop.Stub.FormationCount);
                    foreach (var formation in troop.Stub)
                    {
                        reply.AddByte((byte)formation.Type);
                        reply.AddByte((byte)formation.Count);
                        foreach (var kvp in formation)
                        {
                            reply.AddUInt16(kvp.Key);
                            reply.AddUInt16(kvp.Value);
                            template[kvp.Key] = city.Template[kvp.Key];
                        }
                    }

                    reply.AddUInt16((ushort)template.Size);
                    IEnumerator<KeyValuePair<ushort, IBaseUnitStats>> templateIter = template.GetEnumerator();
                    while (templateIter.MoveNext())
                    {
                        KeyValuePair<ushort, IBaseUnitStats> kvp = templateIter.Current;
                        reply.AddUInt16(kvp.Key);
                        reply.AddByte(kvp.Value.Lvl);
                    }
                }

                session.Write(reply);
            }
        }

        private void LocalTroopSet(Session session, Packet packet)
        {
            uint cityId;
            bool hideNewUnits;
            ISimpleStub stub;
            try
            {
                cityId = packet.GetUInt32();
                hideNewUnits = packet.GetByte() == 1;
                stub = PacketHelper.ReadStub(packet, FormationType.Normal, FormationType.Garrison);
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Concurrency.Current.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                // Set where new units should be sent to
                city.BeginUpdate();
                city.HideNewUnits = hideNewUnits;
                city.EndUpdate();

                // Move units
                if (stub.TotalCount > 0)
                {
                    var currentUnits = city.DefaultTroop.ToUnitList(FormationType.Normal, FormationType.Garrison);
                    var newUnits = stub.ToUnitList();

                    if (currentUnits.Count != newUnits.Count)
                    {
                        ReplyError(session, packet, Error.TroopChanged);
                        return;
                    }

                    // Units are ordered by their type so we can compare the array by indexes
                    if (
                            currentUnits.Where(
                                               (currentUnit, i) =>
                                               currentUnit.Type != newUnits[i].Type ||
                                               currentUnit.Count != newUnits[i].Count).Any())
                    {
                        ReplyError(session, packet, Error.TroopChanged);
                        return;
                    }

                    city.DefaultTroop.BeginUpdate();
                    city.DefaultTroop.RemoveAllUnits(FormationType.Normal, FormationType.Garrison);
                    city.DefaultTroop.Add(stub);
                    city.DefaultTroop.EndUpdate();
                }

                ReplySuccess(session, packet);
            }
        }

        private void UnitUpgrade(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            ushort type;

            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                type = packet.GetUInt16();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Concurrency.Current.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                IStructure barrack;
                if (!city.TryGetStructure(objectId, out barrack))
                {
                    ReplyError(session, packet, Error.Unexpected);
                }

                var upgradeAction = actionFactory.CreateUnitUpgradeActiveAction(cityId, objectId, type);
                Error ret = city.Worker.DoActive(structureFactory.GetActionWorkerType(barrack),
                                                 barrack,
                                                 upgradeAction,
                                                 barrack.Technologies);
                if (ret != 0)
                {
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            }
        }

        private void TrainUnit(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            ushort type;
            ushort count;

            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                type = packet.GetUInt16();
                count = packet.GetUInt16();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Concurrency.Current.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                IStructure barrack;
                if (!city.TryGetStructure(objectId, out barrack))
                {
                    ReplyError(session, packet, Error.Unexpected);
                }

                var trainAction = actionFactory.CreateUnitTrainActiveAction(cityId, objectId, type, count);
                Error ret = city.Worker.DoActive(structureFactory.GetActionWorkerType(barrack),
                                                 barrack,
                                                 trainAction,
                                                 barrack.Technologies);
                if (ret != 0)
                {
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            }
        }

        private void AttackStronghold(Session session, Packet packet)
        {
            uint cityId;
            uint targetStrongholdId;
            ISimpleStub simpleStub;
            AttackMode mode;

            try
            {
                mode = (AttackMode)packet.GetByte();
                cityId = packet.GetUInt32();
                targetStrongholdId = packet.GetUInt32();
                simpleStub = PacketHelper.ReadStub(packet, FormationType.Attack);
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            IStronghold stronghold;
            ICity city;

            using (Concurrency.Current.Lock(session.Player))
            {
                city = session.Player.GetCity(cityId);
                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                if (!gameObjectLocator.TryGetObjects(targetStrongholdId, out stronghold))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
            }

            using (Concurrency.Current.Lock(city, stronghold))
            {
                // Create troop object                
                ITroopObject troopObject;
                if (!Procedure.Current.TroopObjectCreateFromCity(city, simpleStub, city.X, city.Y, out troopObject))
                {
                    ReplyError(session, packet, Error.TroopChanged);
                    return;
                }

                var aa = actionFactory.CreateStrongholdAttackChainAction(cityId,
                                                                         troopObject.ObjectId,
                                                                         targetStrongholdId,
                                                                         mode);
                Error ret = city.Worker.DoPassive(city, aa, true);
                if (ret != 0)
                {
                    Procedure.Current.TroopObjectDelete(troopObject, true);
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            }
        }

        private void AttackBarbarianTribe(Session session, Packet packet)
        {
            uint cityId;
            uint targetBarbarianTribeId;
            ISimpleStub simpleStub;
            AttackMode mode;

            try
            {
                mode = (AttackMode)packet.GetByte();
                cityId = packet.GetUInt32();
                targetBarbarianTribeId = packet.GetUInt32();
                simpleStub = PacketHelper.ReadStub(packet, FormationType.Attack);
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            IBarbarianTribe barbarianTribe;
            ICity city;

            if (!gameObjectLocator.TryGetObjects(targetBarbarianTribeId, out barbarianTribe))
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Concurrency.Current.Lock(session.Player, barbarianTribe))
            {
                city = session.Player.GetCity(cityId);
                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                // Create troop object                
                ITroopObject troopObject;
                if (!Procedure.Current.TroopObjectCreateFromCity(city, simpleStub, city.X, city.Y, out troopObject))
                {
                    ReplyError(session, packet, Error.TroopChanged);
                    return;
                }

                var aa = actionFactory.CreateBarbarianTribeAttackChainAction(cityId, troopObject.ObjectId, targetBarbarianTribeId, mode);
                Error ret = city.Worker.DoPassive(city, aa, true);
                if (ret != 0)
                {
                    Procedure.Current.TroopObjectDelete(troopObject, true);
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            }
        }

        private void AttackCity(Session session, Packet packet)
        {
            uint cityId;
            uint targetCityId;
            uint targetObjectId;
            ISimpleStub simpleStub;
            AttackMode mode;

            try
            {
                mode = (AttackMode)packet.GetByte();
                cityId = packet.GetUInt32();
                targetCityId = packet.GetUInt32();
                targetObjectId = packet.GetUInt32();
                simpleStub = PacketHelper.ReadStub(packet, FormationType.Attack);
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
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

                ICity targetCity = cities[targetCityId];
                IStructure targetStructure;

                if (!targetCity.TryGetStructure(targetObjectId, out targetStructure))
                {
                    ReplyError(session, packet, Error.ObjectStructureNotFound);
                    return;
                }

                // Create troop object                
                ITroopObject troopObject;
                if (!Procedure.Current.TroopObjectCreateFromCity(city, simpleStub, city.X, city.Y, out troopObject))
                {
                    ReplyError(session, packet, Error.TroopChanged);
                    return;
                }

                var aa = actionFactory.CreateCityAttackChainAction(cityId,
                                                                   troopObject.ObjectId,
                                                                   targetCityId,
                                                                   targetObjectId,
                                                                   mode);
                Error ret = city.Worker.DoPassive(city, aa, true);
                if (ret != 0)
                {
                    Procedure.Current.TroopObjectDelete(troopObject, true);
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            }
        }

        private void DefendCity(Session session, Packet packet)
        {
            uint cityId;
            uint targetCityId;
            ISimpleStub simpleStub;
            AttackMode mode;

            try
            {
                cityId = packet.GetUInt32();
                targetCityId = packet.GetUInt32();
                simpleStub = PacketHelper.ReadStub(packet, FormationType.Defense);
                mode = (AttackMode)packet.GetByte();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (cityId == targetCityId)
            {
                ReplyError(session, packet, Error.DefendSelf);
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
                ICity city = cities[cityId];

                ITroopObject troopObject;
                if (!Procedure.Current.TroopObjectCreateFromCity(city, simpleStub, city.X, city.Y, out troopObject))
                {
                    ReplyError(session, packet, Error.ObjectNotFound);
                    return;
                }

                var da = actionFactory.CreateCityDefenseChainAction(cityId, troopObject.ObjectId, targetCityId, mode);
                Error ret = city.Worker.DoPassive(city, da, true);
                if (ret != 0)
                {
                    Procedure.Current.TroopObjectDelete(troopObject, true);
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            }
        }

        private void DefendStronghold(Session session, Packet packet)
        {
            uint cityId;
            uint targetStrongholdId;
            ISimpleStub simpleStub;
            AttackMode mode;

            try
            {
                cityId = packet.GetUInt32();
                targetStrongholdId = packet.GetUInt32();
                simpleStub = PacketHelper.ReadStub(packet, FormationType.Defense);
                mode = (AttackMode)packet.GetByte();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            IStronghold stronghold;
            ICity city;
            using (Concurrency.Current.Lock(session.Player))
            {
                city = session.Player.GetCity(cityId);
                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                if (!gameObjectLocator.TryGetObjects(targetStrongholdId, out stronghold))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
            }

            using (Concurrency.Current.Lock(city, stronghold))
            {
                ITroopObject troopObject;
                if (!Procedure.Current.TroopObjectCreateFromCity(city, simpleStub, city.X, city.Y, out troopObject))
                {
                    ReplyError(session, packet, Error.ObjectNotFound);
                    return;
                }

                var da = actionFactory.CreateStrongholdDefenseChainAction(cityId,
                                                                          troopObject.ObjectId,
                                                                          targetStrongholdId,
                                                                          mode);
                Error ret = city.Worker.DoPassive(city, da, true);
                if (ret != 0)
                {
                    Procedure.Current.TroopObjectDelete(troopObject, true);
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            }
        }

        private void Retreat(Session session, Packet packet)
        {
            uint cityId;
            byte troopId;

            try
            {
                cityId = packet.GetUInt32();
                troopId = packet.GetByte();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            ICity city;
            IStation station;

            //we need to find out the stationed city first then reacquire local + stationed city locks            
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                ITroopStub stub;

                if (!city.Troops.TryGetStub(troopId, out stub) || stub.Station == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                station = stub.Station;
            }

            using (Concurrency.Current.Lock(city, station))
            {
                ITroopStub stub;

                if (!city.Troops.TryGetStub(troopId, out stub))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                //Make sure that the person sending the retreat is either the guy who owns the troop or the guy who owns the stationed city
                if (city.Owner != session.Player && stub.Station != null &&
                    session.Player.GetCityList().All(x => x != stub.Station))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var ra = actionFactory.CreateRetreatChainAction(cityId, troopId);

                Error ret = city.Worker.DoPassive(city, ra, true);
                ReplyWithResult(session, packet, ret);
            }
        }
    }
}
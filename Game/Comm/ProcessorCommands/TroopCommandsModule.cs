#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class TroopCommandsModule : CommandModule
    {
        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.UnitTrain, TrainUnit);
            processor.RegisterCommand(Command.UnitUpgrade, UnitUpgrade);
            processor.RegisterCommand(Command.TroopInfo, GetTroopInfo);
            processor.RegisterCommand(Command.TroopAttack, Attack);
            processor.RegisterCommand(Command.TroopDefend, Defend);
            processor.RegisterCommand(Command.TroopRetreat, Retreat);
            processor.RegisterCommand(Command.TroopLocalSet, LocalTroopSet);
        }

        private void GetTroopInfo(Session session, Packet packet)
        {
            City city;
            TroopObject troop;

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
                    reply.AddByte(troop.Stats.Speed);
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
                    IEnumerator<KeyValuePair<ushort, BaseUnitStats>> templateIter = template.GetEnumerator();
                    while (templateIter.MoveNext())
                    {
                        KeyValuePair<ushort, BaseUnitStats> kvp = templateIter.Current;
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
            TroopStub stub;
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
                City city = session.Player.GetCity(cityId);

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
                    if (currentUnits.Where((currentUnit, i) => currentUnit.Type != newUnits[i].Type || currentUnit.Count != newUnits[i].Count).Any())
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
                City city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                Structure barrack;
                if (!city.TryGetStructure(objectId, out barrack))
                    ReplyError(session, packet, Error.Unexpected);

                var upgradeAction = new UnitUpgradeActiveAction(cityId, objectId, type);
                Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(barrack), barrack, upgradeAction, barrack.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
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
                City city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                Structure barrack;
                if (!city.TryGetStructure(objectId, out barrack))
                    ReplyError(session, packet, Error.Unexpected);

                var trainAction = new UnitTrainActiveAction(cityId, objectId, type, count);
                Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(barrack), barrack, trainAction, barrack.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
            }
        }

        private void Attack(Session session, Packet packet)
        {
            uint cityId;
            uint targetCityId;
            uint targetObjectId;
            TroopStub stub;
            AttackMode mode;

            try
            {
                mode = (AttackMode)packet.GetByte();
                cityId = packet.GetUInt32();
                targetCityId = packet.GetUInt32();
                targetObjectId = packet.GetUInt32();
                stub = PacketHelper.ReadStub(packet, FormationType.Attack);
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

            Dictionary<uint, City> cities;
            using (Concurrency.Current.Lock(out cities, cityId, targetCityId))
            {
                if (cities == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                City city = cities[cityId];

                City targetCity = cities[targetCityId];
                Structure targetStructure;

                if (city.Owner.PlayerId == targetCity.Owner.PlayerId)
                {
                    ReplyError(session, packet, Error.AttackSelf);
                    return;
                }

                if (!targetCity.TryGetStructure(targetObjectId, out targetStructure))
                {
                    ReplyError(session, packet, Error.ObjectStructureNotFound);
                    return;
                }

                // Create troop object                
                if (!Procedure.Current.TroopObjectCreateFromCity(city, stub, city.X, city.Y))
                {
                    ReplyError(session, packet, Error.TroopChanged);
                    return;
                }

                var aa = new AttackChainAction(cityId, stub.TroopId, targetCityId, targetObjectId, mode);
                Error ret = city.Worker.DoPassive(city, aa, true);
                if (ret != 0)
                {
                    Procedure.Current.TroopObjectDelete(stub.TroopObject, true);
                    ReplyError(session, packet, ret);
                }
                else
                    ReplySuccess(session, packet);
            }
        }

        private void Defend(Session session, Packet packet)
        {
            uint cityId;
            uint targetCityId;
            TroopStub stub;

            try
            {
                cityId = packet.GetUInt32();
                targetCityId = packet.GetUInt32();
                stub = PacketHelper.ReadStub(packet, FormationType.Defense);
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

            Dictionary<uint, City> cities;
            using (Concurrency.Current.Lock(out cities, cityId, targetCityId))
            {
                City city = cities[cityId];

                if (!Procedure.Current.TroopObjectCreateFromCity(city, stub, city.X, city.Y))
                {
                    ReplyError(session, packet, Error.ObjectNotFound);
                    return;
                }

                var da = new DefenseChainAction(cityId, stub.TroopId, targetCityId);
                Error ret = city.Worker.DoPassive(city, da, true);
                if (ret != 0)
                {
                    Procedure.Current.TroopObjectDelete(stub.TroopObject, true);
                    ReplyError(session, packet, ret);
                }
                else
                    ReplySuccess(session, packet);
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

            City city;
            City stationedCity;

            //we need to find out the stationed city first then reacquire local + stationed city locks            
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                TroopStub stub;

                if (!city.Troops.TryGetStub(troopId, out stub) || stub.StationedCity == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                stationedCity = stub.StationedCity;
            }

            using (Concurrency.Current.Lock(city, stationedCity))
            {
                TroopStub stub;

                if (!city.Troops.TryGetStub(troopId, out stub) || stub.StationedCity == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                //Make sure that the person sending the retreat is either the guy who owns the troop or the guy who owns the stationed city
                if (city.Owner != session.Player && stub.StationedCity.Owner != session.Player)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                if (stub.StationedCity.Battle != null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                stationedCity = stub.StationedCity;

                if (!Procedure.Current.TroopObjectCreateFromStation(stub, stub.StationedCity.X, stub.StationedCity.Y))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var ra = new RetreatChainAction(cityId, troopId);

                Error ret = city.Worker.DoPassive(city, ra, true);
                if (ret != 0)
                {
                    Procedure.Current.TroopObjectStation(stub.TroopObject, stationedCity);
                    ReplyError(session, packet, ret);
                }
                else
                    ReplySuccess(session, packet);
            }
        }
    }
}
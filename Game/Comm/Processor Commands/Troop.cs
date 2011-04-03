#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        public void CmdGetTroopInfo(Session session, Packet packet)
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

            using (new MultiObjectLock(cityId, objectId, out city, out troop))
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

        public void CmdLocalTroopSet(Session session, Packet packet)
        {
            uint cityId;
            byte formationCount;
            bool hideNewUnits;
            try
            {
                cityId = packet.GetUInt32();
                hideNewUnits = packet.GetByte() == 1;
                formationCount = packet.GetByte();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (new MultiObjectLock(session.Player))
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

                // Validate troop stub sent from player
                var stub = new TroopStub();

                if (formationCount != 2)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                for (int f = 0; f < formationCount; ++f)
                {
                    FormationType formationType;
                    byte unitCount;
                    try
                    {
                        formationType = (FormationType)packet.GetByte();

                        if ((f == 0 && formationType != FormationType.Normal) || (f == 1 && formationType != FormationType.Garrison))
                        {
                            // a bit dirty
                            ReplyError(session, packet, Error.Unexpected);
                            return;
                        }

                        unitCount = packet.GetByte();
                    }
                    catch(Exception)
                    {
                        ReplyError(session, packet, Error.Unexpected);
                        return;
                    }

                    stub.AddFormation(formationType);

                    for (int u = 0; u < unitCount; ++u)
                    {
                        ushort type;
                        ushort count;

                        try
                        {
                            type = packet.GetUInt16();
                            count = packet.GetUInt16();
                        }
                        catch(Exception)
                        {
                            ReplyError(session, packet, Error.Unexpected);
                            return;
                        }

                        stub.AddUnit(formationType, type, count);
                    }
                }

                if (stub.TotalCount > 0)
                {
                    stub.TroopId = 1;
                    if (!stub.Equal(city.DefaultTroop, FormationType.InBattle))
                    {
                        ReplyError(session, packet, Error.Unexpected);
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

        public void CmdUnitUpgrade(Session session, Packet packet)
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

            using (new MultiObjectLock(session.Player))
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
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(barrack), barrack, upgradeAction, barrack.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
            }
        }

        public void CmdTrainUnit(Session session, Packet packet)
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

            using (new MultiObjectLock(session.Player))
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
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(barrack), barrack, trainAction, barrack.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
            }
        }

        public void CmdTroopAttack(Session session, Packet packet)
        {
            uint cityId;
            uint targetCityId;
            uint targetObjectId;
            byte formationCount;
            AttackMode mode;

            try
            {
                mode = (AttackMode)packet.GetByte();
                cityId = packet.GetUInt32();
                targetCityId = packet.GetUInt32();
                targetObjectId = packet.GetUInt32();
                formationCount = packet.GetByte();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (cityId == targetCityId)
            {
                ReplyError(session, packet, Error.AttackSelf);
                return;
            }

            using (new MultiObjectLock(session.Player))
            {
                if (session.Player.GetCity(cityId) == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
            }

            Dictionary<uint, City> cities;
            using (new MultiObjectLock(out cities, cityId, targetCityId))
            {
                if (cities == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                City city = cities[cityId];

                City targetCity = cities[targetCityId];
                Structure targetStructure;

                if (!targetCity.TryGetStructure(targetObjectId, out targetStructure))
                {
                    ReplyError(session, packet, Error.ObjectStructureNotFound);
                    return;
                }

                var stub = new TroopStub();

                for (int f = 0; f < formationCount; ++f)
                {
                    FormationType formationType;
                    byte unitCount;
                    try
                    {
                        formationType = (FormationType)packet.GetByte();
                        unitCount = packet.GetByte();
                    }
                    catch(Exception)
                    {
                        ReplyError(session, packet, Error.Unexpected);
                        return;
                    }

                    stub.AddFormation(formationType);

                    for (int u = 0; u < unitCount; ++u)
                    {
                        ushort type;
                        ushort count;

                        try
                        {
                            type = packet.GetUInt16();
                            count = packet.GetUInt16();
                        }
                        catch(Exception)
                        {
                            ReplyError(session, packet, Error.Unexpected);
                            return;
                        }

                        stub.AddUnit(formationType, type, count);
                    }
                }

                // Create troop object                
                if (!Procedure.TroopObjectCreate(city, stub, city.X, city.Y))
                {
                    ReplyError(session, packet, Error.TroopChanged);
                    return;
                }

                var aa = new AttackChainAction(cityId, stub.TroopId, targetCityId, targetObjectId, mode);
                Error ret = city.Worker.DoPassive(city, aa, true);
                if (ret != 0)
                {
                    Procedure.TroopObjectDelete(stub.TroopObject, true);
                    ReplyError(session, packet, ret);
                }
                else
                    ReplySuccess(session, packet);
            }
        }

        public void CmdTroopDefend(Session session, Packet packet)
        {
            uint cityId;
            uint targetCityId;
            byte formationCount;

            try
            {
                cityId = packet.GetUInt32();
                targetCityId = packet.GetUInt32();
                formationCount = packet.GetByte();
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

            using (new MultiObjectLock(session.Player))
            {
                if (session.Player.GetCity(cityId) == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
            }

            Dictionary<uint, City> cities;
            using (new MultiObjectLock(out cities, cityId, targetCityId))
            {
                City city = cities[cityId];

                var stub = new TroopStub();

                for (int f = 0; f < formationCount; ++f)
                {
                    FormationType formationType;
                    byte unitCount;
                    try
                    {
                        formationType = (FormationType)packet.GetByte();
                        unitCount = packet.GetByte();
                    }
                    catch(Exception)
                    {
                        ReplyError(session, packet, Error.Unexpected);
                        return;
                    }

                    stub.AddFormation(formationType);

                    for (int u = 0; u < unitCount; ++u)
                    {
                        ushort type;
                        ushort count;

                        try
                        {
                            type = packet.GetUInt16();
                            count = packet.GetUInt16();
                        }
                        catch(Exception)
                        {
                            ReplyError(session, packet, Error.Unexpected);
                            return;
                        }

                        stub.AddUnit(formationType, type, count);
                    }
                }

                if (!Procedure.TroopObjectCreate(city, stub, city.X, city.Y))
                {
                    ReplyError(session, packet, Error.ObjectNotFound);
                    return;
                }

                var da = new DefenseChainAction(cityId, stub.TroopId, targetCityId);
                Error ret = city.Worker.DoPassive(city, da, true);
                if (ret != 0)
                {
                    Procedure.TroopObjectDelete(stub.TroopObject, true);
                    ReplyError(session, packet, ret);
                }
                else
                    ReplySuccess(session, packet);
            }
        }

        public void CmdTroopRetreat(Session session, Packet packet)
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
            using (new MultiObjectLock(cityId, out city))
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

            using (new MultiObjectLock(city, stationedCity))
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

                if (!Procedure.TroopObjectCreateFromStation(stub, stub.StationedCity.X, stub.StationedCity.Y))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var ra = new RetreatChainAction(cityId, troopId);

                Error ret = city.Worker.DoPassive(city, ra, true);
                if (ret != 0)
                {
                    Procedure.TroopObjectStation(stub.TroopObject, stationedCity);
                    ReplyError(session, packet, ret);
                }
                else
                    ReplySuccess(session, packet);
            }
        }
    }
}
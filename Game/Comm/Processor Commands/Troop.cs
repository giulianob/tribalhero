using System;
using System.Collections.Generic;
using System.Text;
using Game.Logic.Actions;
using Game.Logic;
using Game.Data;
using Game.Setup;
using Game.Fighting;
using Game.Database;
using Game.Util;
using Game.Logic.Procedures;

namespace Game.Comm {
    public partial class Processor {
        public void CmdLocalTroopSet(Session session, Packet packet) {
            uint cityId;
            byte formationCount;
            try {
                cityId = packet.getUInt32();
                formationCount = packet.getByte();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {

                City city = session.Player.getCity(cityId);

                if (city == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                TroopStub stub = new TroopStub();
                Dictionary<ushort, uint> holder = new Dictionary<ushort, uint>();

                if (formationCount != 2) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                for (int f = 0; f < formationCount; ++f) {
                    FormationType formationType;
                    byte unitCount;
                    try {
                        formationType = (FormationType)packet.getByte();

                        if ((f == 0 && formationType != FormationType.Normal) || (f == 1 && formationType != FormationType.Garrison)) { // a bit dirty
                            reply_error(session, packet, Error.UNEXPECTED);
                            return;
                        }

                        unitCount = packet.getByte();
                    }
                    catch (Exception) {
                        reply_error(session, packet, Error.UNEXPECTED);
                        return;
                    }

                    stub.addFormation(formationType);

                    for (int u = 0; u < unitCount; ++u) {
                        ushort type;
                        ushort count;

                        try {
                            type = packet.getUInt16();
                            count = packet.getUInt16();
                        }
                        catch (Exception) {
                            reply_error(session, packet, Error.UNEXPECTED);
                            return;
                        }

                        stub.addUnit(formationType, type, count);
                    }
                }
                stub.TroopId = 1;
                if (!stub.Equal(city.DefaultTroop)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                city.DefaultTroop.BeginUpdate();
                city.DefaultTroop.removeAllUnits();
                city.DefaultTroop.add(stub);
                city.DefaultTroop.EndUpdate();

                reply_success(session, packet);
            }
        }

        public void CmdUnitUpgrade(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            ushort type;

            try {
                cityId = packet.getUInt32();
                objectId = packet.getUInt32();
                type = packet.getUInt16();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.getCity(cityId);

                if (city == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                Structure barrack;
                if (!city.tryGetStructure(objectId, out barrack)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                }

                UnitUpgradeAction upgrade_action = new UnitUpgradeAction(cityId, objectId, type);
                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(barrack), barrack, upgrade_action, barrack.Technologies);
                if (ret != 0) {
                    reply_error(session, packet, ret);
                }
                else {
                    reply_success(session, packet);
                }
            }
        }

        public void CmdTrainUnit(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            ushort type;
            ushort count;

            try {
                cityId = packet.getUInt32();
                objectId = packet.getUInt32();
                type = packet.getUInt16();
                count = packet.getUInt16();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.getCity(cityId);

                if (city == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                Structure barrack;
                if (!city.tryGetStructure(objectId, out barrack)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                }

                UnitTrainAction train_action = new UnitTrainAction(cityId, objectId, type, count);
                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(barrack), barrack, train_action, barrack.Technologies);
                if (ret != 0) {
                    reply_error(session, packet, ret);
                }
                else {
                    reply_success(session, packet);
                }
            }
        }

        public void CmdTroopAttack(Session session, Packet packet) {
            uint cityId;
            uint targetCityId;
            uint targetObjectId;            
            byte formationCount;
            AttackMode mode;

            try {
                mode = (AttackMode)packet.getByte();
                cityId = packet.getUInt32();
                targetCityId = packet.getUInt32();
                targetObjectId = packet.getUInt32();
                formationCount = packet.getByte();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            if (cityId == targetCityId) {
                reply_error(session, packet, Error.ATTACK_SELF);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                if (session.Player.getCity(cityId) == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }
            }

            Dictionary<uint, City> cities;
            using (new MultiObjectLock(out cities, cityId, targetCityId)) {
                if (cities == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                City city = cities[cityId];

                if (city.Battle != null) {
                    reply_error(session, packet, Error.CITY_IN_BATTLE);
                    return;
                }

                City targetCity = cities[targetCityId];
                Structure targetStructure;

                if (!targetCity.tryGetStructure(targetObjectId, out targetStructure)) {
                    reply_error(session, packet, Error.OBJECT_STRUCTURE_NOT_FOUND);
                    return;
                }

                TroopStub stub = new TroopStub();

                for (int f = 0; f < formationCount; ++f) {
                    FormationType formationType;
                    byte unitCount;
                    try {
                        formationType = (FormationType)packet.getByte();
                        unitCount = packet.getByte();
                    }
                    catch (Exception) {
                        reply_error(session, packet, Error.UNEXPECTED);
                        return;
                    }

                    stub.addFormation(formationType);

                    for (int u = 0; u < unitCount; ++u) {
                        ushort type;
                        ushort count;

                        try {
                            type = packet.getUInt16();
                            count = packet.getUInt16();
                        }
                        catch (Exception) {
                            reply_error(session, packet, Error.UNEXPECTED);
                            return;
                        }

                        stub.addUnit(formationType, type, count);
                    }
                }

                if (!Procedure.TroopObjectCreate(city, stub, city.MainBuilding.X, city.MainBuilding.Y)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                AttackAction aa = new AttackAction(cityId, stub.TroopId, targetCityId, targetObjectId, mode);
                Error ret = city.Worker.doPassive(city, aa, true);
                if (ret != 0) {
                    reply_error(session, packet, ret);
                }
                else {
                    reply_success(session, packet);
                }
            }
        }

        public void CmdTroopDefend(Session session, Packet packet) {
            uint cityId;
            uint targetCityId;
            byte formationCount;

            try {
                cityId = packet.getUInt32();
                targetCityId = packet.getUInt32();
                formationCount = packet.getByte();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            if (cityId == targetCityId) {
                reply_error(session, packet, Error.DEFEND_SELF);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                if (session.Player.getCity(cityId) == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }
            }

            Dictionary<uint, City> cities;
            using (new MultiObjectLock(out cities, cityId, targetCityId)) {

                City city = cities[cityId];
                City targetCity = cities[targetCityId];

                TroopStub stub = new TroopStub();

                for (int f = 0; f < formationCount; ++f) {
                    FormationType formationType;
                    byte unitCount;
                    try {
                        formationType = (FormationType)packet.getByte();
                        unitCount = packet.getByte();
                    }
                    catch (Exception) {
                        reply_error(session, packet, Error.UNEXPECTED);
                        return;
                    }

                    stub.addFormation(formationType);

                    for (int u = 0; u < unitCount; ++u) {
                        ushort type;
                        ushort count;

                        try {
                            type = packet.getUInt16();
                            count = packet.getUInt16();
                        }
                        catch (Exception) {
                            reply_error(session, packet, Error.UNEXPECTED);
                            return;
                        }

                        stub.addUnit(formationType, type, count);
                    }
                }

                if (!Procedure.TroopObjectCreate(city, stub, city.MainBuilding.X, city.MainBuilding.Y)) {
                    reply_error(session, packet, Error.OBJECT_NOT_FOUND);
                    return;
                }

                DefenseAction da = new DefenseAction(cityId, stub.TroopId, targetCityId);
                Error ret = city.Worker.doPassive(city, da, true);
                if (ret != 0) {
                    reply_error(session, packet, ret);
                }
                else {
                    reply_success(session, packet);
                }
            }
        }

        public void CmdTroopRetreat(Session session, Packet packet) {
            uint cityId;
            byte troopId;            

            try {
                cityId = packet.getUInt32();
                troopId = packet.getByte();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            City city;
            City stationedCity;

            //we need to find out the stationed city first then reacquire local + stationed city locks            
            using (new MultiObjectLock(cityId, out city)) {                
                if (city == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }
                
                TroopStub stub;

                if (!city.Troops.TryGetStub(troopId, out stub) || stub.StationedCity == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                stationedCity = stub.StationedCity;
            }

            using (new MultiObjectLock(city, stationedCity)) {
                TroopStub stub;

                if (!city.Troops.TryGetStub(troopId, out stub) || stub.StationedCity == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                //Make sure that the person sending the retreat is either the guy who owns the troop or the guy who owns the stationed city
                if (city.Owner != session.Player && stub.StationedCity.Owner != session.Player) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (stub.StationedCity.Battle != null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (!Procedure.TroopObjectCreateFromStation(stub, stub.StationedCity.MainBuilding.X, stub.StationedCity.MainBuilding.Y)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                RetreatAction ra = new RetreatAction(cityId, troopId);                

                Error ret = city.Worker.doPassive(city, ra, true);
                if (ret != 0) {
                    reply_error(session, packet, ret);
                }
                else {
                    reply_success(session, packet);
                }
            }
        }


    }

}

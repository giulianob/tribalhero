#region

using System;
using System.Linq;
using Game.Data;
using Game.Logic.Actions;
using Game.Setup;
using Game.Util;
using System.Collections.Generic;
using Game.Logic;

#endregion

namespace Game.Comm {
    public partial class Processor {
        public void CmdGetStructureInfo(Session session, Packet packet) {
            City city;
            Structure structure;           

            uint cityId;
            uint objectId;

            try {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(cityId, objectId, out city, out structure)) {
                if (city == null || structure == null) {
                    ReplyError(session, packet, Error.OBJECT_NOT_FOUND);
                    return;
                }

                Packet reply = new Packet(packet);
                reply.AddByte(structure.Stats.Base.Lvl);
                if (session.Player == structure.City.Owner) {
                    reply.AddByte(structure.Stats.Labor);
                    reply.AddUInt16(structure.Stats.Hp);

                    foreach (Property prop in PropertyFactory.GetProperties(structure.Type)) {
                        switch (prop.Type) {
                            case DataType.BYTE:
                                reply.AddByte((byte) prop.GetValue(structure));
                                break;
                            case DataType.USHORT:
                                reply.AddUInt16((ushort) prop.GetValue(structure));
                                break;
                            case DataType.UINT:
                                reply.AddUInt32((uint) prop.GetValue(structure));
                                break;
                            case DataType.STRING:
                                reply.AddString((string) prop.GetValue(structure));
                                break;
                            case DataType.INT:
                                reply.AddInt32((int) prop.GetValue(structure));
                                break;
                        }
                    }

                    PacketHelper.AddToPacket(
                        new List<ReferenceStub>(structure.City.Worker.References.GetReferences(structure)), reply);
                }
                else {
                    foreach (Property prop in PropertyFactory.GetProperties(structure.Type, Visibility.PUBLIC)) {
                        switch (prop.Type) {
                            case DataType.BYTE:
                                reply.AddByte((byte)prop.GetValue(structure));
                                break;
                            case DataType.USHORT:
                                reply.AddUInt16((ushort)prop.GetValue(structure));
                                break;
                            case DataType.UINT:
                                reply.AddUInt32((uint)prop.GetValue(structure));
                                break;
                            case DataType.STRING:
                                reply.AddString((string)prop.GetValue(structure));
                                break;
                            case DataType.INT:
                                reply.AddInt32((int)prop.GetValue(structure));
                                break;
                        }
                    }                    
                }

                session.Write(reply);
            }
        }

        public void CmdGetForestInfo(Session session, Packet packet) {
            Packet reply = new Packet(packet);
            Forest forest;

            uint forestId;            

            try {
                forestId = packet.GetUInt32();                
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(Global.Forests)) {
                if (!Global.Forests.TryGetValue(forestId, out forest)) {
                    ReplyError(session, packet, Error.OBJECT_NOT_FOUND);
                    return;
                }
                
                reply.AddInt32((int)(forest.Rate / Config.seconds_per_unit));
                reply.AddInt32(forest.Labor);
                reply.AddUInt32(UnixDateTime.DateTimeToUnix(forest.DepleteTime.ToUniversalTime()));
                PacketHelper.AddToPacket(forest.Wood, reply);                
            }

            session.Write(reply);
        }

        public void CmdGetCityUsername(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            byte count;
            uint[] cityIds;
            try {
                count = packet.GetByte();
                cityIds = new uint[count];
                for (int i = 0; i < count; i++)
                    cityIds[i] = packet.GetUInt32();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            reply.AddByte(count);
            for (int i = 0; i < count; i++) {
                uint cityId = cityIds[i];
                City city;

                if (!Global.World.TryGetObjects(cityId, out city)) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                reply.AddUInt32(cityId);
                reply.AddString(city.Name);
            }

            session.Write(reply);
        }

        public void CmdLaborMove(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            byte count;
            Structure obj;
            City city;

            try {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                count = packet.GetByte();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                city = session.Player.GetCity(cityId);

                if (city == null || !city.TryGetStructure(objectId, out obj)) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                LaborMoveAction lma;
                if (obj.Stats.Labor < count) {
                    //move from city to obj
                    count = (byte) (count - obj.Stats.Labor);

                    if (city.Resource.Labor.Value < count) {
                        //not enough available in city
                        ReplyError(session, packet, Error.LABOR_NOT_ENOUGH);
                        return;
                    }
 
                    if (obj.Stats.Labor + count > obj.Stats.Base.MaxLabor) {
                        //adding too much to obj
                        ReplyError(session, packet, Error.LABOR_OVERFLOW);
                        return;
                    }
                    
                    lma = new LaborMoveAction(cityId, objectId, true, count);
                } else if (obj.Stats.Labor > count) {
                    //move from obj to city
                    count = (byte) (obj.Stats.Labor - count);
                    lma = new LaborMoveAction(cityId, objectId, false, count);
                } else {
                    ReplySuccess(session, packet);
                    return;
                }

                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, lma, obj.Technologies);

                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
            }
        }

        public void CmdTechnologyUpgrade(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            uint techId;

            try {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                techId = packet.GetUInt32();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.GetCity(cityId);

                Structure obj;

                if (city == null || !city.TryGetStructure(objectId, out obj)) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                TechnologyUpgradeAction upgradeAction = new TechnologyUpgradeAction(cityId, objectId, techId);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, upgradeAction,
                                                 obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
            }
        }

        public void CmdCancelAction(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            ushort actionId;

            try {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                actionId = packet.GetUInt16();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.GetCity(cityId);

                if (city == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                Error ret;
                if ((ret = city.Worker.Cancel(actionId)) != Error.OK)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);

                ReplyError(session, packet, Error.UNEXPECTED);
            }
        }

        public void CmdUpgradeStructure(Session session, Packet packet) {
            uint cityId;
            uint objectId;

            try {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.GetCity(cityId);

                if (city == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                StructureUpgradeAction upgradeAction = new StructureUpgradeAction(cityId, objectId);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, upgradeAction,
                                                 obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);

                return;
            }
        }

        public void CmdDowngradeStructure(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            uint targetId;

            try {
                cityId = packet.GetUInt32();
                targetId = objectId = packet.GetUInt32();
               // targetId = packet.GetUInt32();
            } catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.GetCity(cityId);

                if (city == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                StructureUserDowngradeAction downgradeAction = new StructureUserDowngradeAction(cityId, targetId);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, downgradeAction,
                                                 obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);

                return;
            }
        }

        public void CmdCreateStructure(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            uint x;
            uint y;
            ushort type;

            try {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                x = packet.GetUInt32();
                y = packet.GetUInt32();
                type = packet.GetUInt16();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            if (!Global.World.IsValidXandY(x, y))
            {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.GetCity(cityId);

                if (city == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                StructureBuildAction buildaction = new StructureBuildAction(cityId, type, x, y);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, buildaction,
                                                 obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }

        public void CmdChangeStructure(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            ushort structureType;
            byte structureLvl;

            try {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                structureType = packet.GetUInt16();
                structureLvl = packet.GetByte();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.GetCity(cityId);

                if (city == null) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                StructureChangeAction changeAction = new StructureChangeAction(cityId, objectId, structureType, structureLvl);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, changeAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }

        public void CmdCreateForestCamp(Session session, Packet packet) {
            uint cityId;            
            uint forestId;
            ushort type;
            byte labor;

            try {
                cityId = packet.GetUInt32();                
                forestId = packet.GetUInt32();
                type = packet.GetUInt16();
                labor = packet.GetByte();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            City city = session.Player.GetCity(cityId);
            if (city == null) {
                ReplyError(session, packet, Error.CITY_NOT_FOUND);
                return;
            }

            using (new CallbackLock(Global.Forests.CallbackLockHandler, new object[] { forestId }, city, Global.Forests)) {
                
                // Get the lumbermill
                Structure lumbermill = city.FirstOrDefault(structure => ObjectTypeFactory.IsStructureType("Wood", structure));
                
                if (lumbermill == null) {
                    ReplyError(session, packet, Error.LUMBERMILL_UNAVAILABLE);
                    return;
                }

                ForestCampBuildAction buildaction = new ForestCampBuildAction(cityId, lumbermill.ObjectId, forestId, type, labor);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(lumbermill), lumbermill, buildaction,
                                                 lumbermill.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }    
    }
}
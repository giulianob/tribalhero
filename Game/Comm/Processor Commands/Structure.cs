#region

using System;
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

                    foreach (Property prop in PropertyFactory.getProperties(structure.Type)) {
                        if (!structure.Properties.Contains(prop.name)) {
                            switch (prop.type) {
                                case DataType.Byte:
                                    reply.AddByte(Byte.MaxValue);
                                    break;
                                case DataType.UShort:
                                    reply.AddUInt16(UInt16.MinValue);
                                    break;
                                case DataType.UInt:
                                    reply.AddUInt32(UInt32.MinValue);
                                    break;
                                case DataType.String:
                                    reply.AddString("N/A");
                                    break;
                                case DataType.Int:
                                    reply.AddInt32(Int16.MaxValue);
                                    break;
                            }
                        } else {
                            switch (prop.type) {
                                case DataType.Byte:
                                    reply.AddByte((byte) prop.getValue(structure));
                                    break;
                                case DataType.UShort:
                                    reply.AddUInt16((ushort) prop.getValue(structure));
                                    break;
                                case DataType.UInt:
                                    reply.AddUInt32((uint) prop.getValue(structure));
                                    break;
                                case DataType.String:
                                    reply.AddString((string) prop.getValue(structure));
                                    break;
                                case DataType.Int:
                                    reply.AddInt32((int) prop.getValue(structure));
                                    break;
                            }
                        }
                    }

                    PacketHelper.AddToPacket(
                        new List<ReferenceStub>(structure.City.Worker.References.GetReferences(structure)), reply);
                }

                session.Write(reply);
            }
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

                Structure obj = city[objectId] as Structure;
                if (obj != null) {
                    Error ret;
                    if ((ret = city.Worker.Cancel(actionId)) != Error.OK)
                        ReplyError(session, packet, ret);
                    else
                        ReplySuccess(session, packet);
                }

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
    }
}
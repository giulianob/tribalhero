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
                cityId = packet.getUInt32();
                objectId = packet.getUInt32();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(cityId, objectId, out city, out structure)) {
                if (city == null || structure == null) {
                    reply_error(session, packet, Error.OBJECT_NOT_FOUND);
                    return;
                }

                Packet reply = new Packet(packet);
                reply.addByte(structure.Stats.Base.Lvl);
                if (session.Player == structure.City.Owner) {
                    reply.addByte(structure.Stats.Labor);
                    reply.addUInt16(structure.Stats.Hp);

                    foreach (Property prop in PropertyFactory.getProperties(structure.Type)) {
                        if (!structure.Properties.contains(prop.name)) {
                            switch (prop.type) {
                                case DataType.Byte:
                                    reply.addByte(Byte.MaxValue);
                                    break;
                                case DataType.UShort:
                                    reply.addUInt16(UInt16.MinValue);
                                    break;
                                case DataType.UInt:
                                    reply.addUInt32(UInt32.MinValue);
                                    break;
                                case DataType.String:
                                    reply.addString("N/A");
                                    break;
                                case DataType.Int:
                                    reply.addInt32(Int16.MaxValue);
                                    break;
                            }
                        } else {
                            switch (prop.type) {
                                case DataType.Byte:
                                    reply.addByte((byte) prop.getValue(structure));
                                    break;
                                case DataType.UShort:
                                    reply.addUInt16((ushort) prop.getValue(structure));
                                    break;
                                case DataType.UInt:
                                    reply.addUInt32((uint) prop.getValue(structure));
                                    break;
                                case DataType.String:
                                    reply.addString((string) prop.getValue(structure));
                                    break;
                                case DataType.Int:
                                    reply.addInt32((int) prop.getValue(structure));
                                    break;
                            }
                        }
                    }

                    PacketHelper.AddToPacket(
                        new List<ReferenceStub>(structure.City.Worker.References.getReferences(structure)), reply);
                }

                session.write(reply);
            }
        }

        public void CmdGetCityUsername(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            byte count;
            uint[] cityIds;
            try {
                count = packet.getByte();
                cityIds = new uint[count];
                for (int i = 0; i < count; i++)
                    cityIds[i] = packet.getUInt32();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            reply.addByte(count);
            for (int i = 0; i < count; i++) {
                uint cityId = cityIds[i];
                City city;

                if (!Global.World.TryGetObjects(cityId, out city)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                reply.addUInt32(cityId);
                reply.addString(city.Name);
            }

            session.write(reply);
        }

        public void CmdLaborMove(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            byte count;
            Structure obj;
            City city;

            try {
                cityId = packet.getUInt32();
                objectId = packet.getUInt32();
                count = packet.getByte();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                city = session.Player.getCity(cityId);

                if (city == null || !city.TryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                LaborMoveAction lma;
                if (obj.Stats.Labor < count) {
                    //move from city to obj
                    count = (byte) (count - obj.Stats.Labor);

                    if (city.Resource.Labor.Value < count) {
                        //not enough available in city
                        reply_error(session, packet, Error.LABOR_NOT_ENOUGH);
                        return;
                    }
 
                    if (obj.Stats.Labor + count > obj.Stats.Base.MaxLabor) {
                        //adding too much to obj
                        reply_error(session, packet, Error.LABOR_OVERFLOW);
                        return;
                    }
                    
                    lma = new LaborMoveAction(cityId, objectId, true, count);
                } else if (obj.Stats.Labor > count) {
                    //move from obj to city
                    count = (byte) (obj.Stats.Labor - count);
                    lma = new LaborMoveAction(cityId, objectId, false, count);
                } else {
                    reply_success(session, packet);
                    return;
                }

                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(obj), obj, lma, obj.Technologies);

                if (ret != 0)
                    reply_error(session, packet, ret);
                else
                    reply_success(session, packet);
            }
        }

        public void CmdTechnologyUpgrade(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            uint techId;

            try {
                cityId = packet.getUInt32();
                objectId = packet.getUInt32();
                techId = packet.getUInt32();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.getCity(cityId);

                Structure obj;

                if (city == null || !city.TryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                TechnologyUpgradeAction upgradeAction = new TechnologyUpgradeAction(cityId, objectId, techId);
                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(obj), obj, upgradeAction,
                                                 obj.Technologies);
                if (ret != 0)
                    reply_error(session, packet, ret);
                else
                    reply_success(session, packet);
            }
        }

        public void CmdCancelAction(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            ushort actionId;

            try {
                cityId = packet.getUInt32();
                objectId = packet.getUInt32();
                actionId = packet.getUInt16();
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

                Structure obj = city[objectId] as Structure;
                if (obj != null) {
                    Error ret;
                    if ((ret = city.Worker.Cancel(actionId)) != Error.OK)
                        reply_error(session, packet, ret);
                    else
                        reply_success(session, packet);
                }

                reply_error(session, packet, Error.UNEXPECTED);
            }
        }

        public void CmdUpgradeStructure(Session session, Packet packet) {
            uint cityId;
            uint objectId;

            try {
                cityId = packet.getUInt32();
                objectId = packet.getUInt32();
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

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                StructureUpgradeAction upgradeAction = new StructureUpgradeAction(cityId, objectId);
                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(obj), obj, upgradeAction,
                                                 obj.Technologies);
                if (ret != 0)
                    reply_error(session, packet, ret);
                else
                    reply_success(session, packet);

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
                cityId = packet.getUInt32();
                objectId = packet.getUInt32();
                x = packet.getUInt32();
                y = packet.getUInt32();
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

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                StructureBuildAction buildaction = new StructureBuildAction(cityId, type, x, y);
                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(obj), obj, buildaction,
                                                 obj.Technologies);
                if (ret != 0)
                    reply_error(session, packet, ret);
                else
                    reply_success(session, packet);
                return;
            }
        }

        public void CmdChangeStructure(Session session, Packet packet) {
            uint cityId;
            uint objectId;
            ushort structureType;
            byte structureLvl;

            try {
                cityId = packet.getUInt32();
                objectId = packet.getUInt32();
                structureType = packet.getUInt16();
                structureLvl = packet.getByte();
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

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                StructureChangeAction changeAction = new StructureChangeAction(cityId, objectId, structureType,
                                                                               structureLvl, false, false);
                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(obj), obj, changeAction,
                                                 obj.Technologies);
                if (ret != 0)
                    reply_error(session, packet, ret);
                else
                    reply_success(session, packet);
                return;
            }
        }
    }
}
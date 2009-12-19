using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Game.Data;
using Game.Setup;
using Game.Logic;
using Game.Fighting;
using Game.Util;
using Game.Data.Stats;

namespace Game.Comm {
    public partial class Processor {

        public void CmdNotificationLocate(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            uint srcCityId;
            uint cityId;
            ushort actionId;

            try {
                srcCityId = packet.getUInt32();
                cityId = packet.getUInt32();
                actionId = packet.getUInt16();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            //check to make sure that the city belongs to us
            using (new MultiObjectLock(session.Player)) {
                if (session.Player.getCity(cityId) == null && session.Player.getCity(srcCityId) == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }
            }

            Dictionary<uint, City> cities;
            using (new MultiObjectLock(out cities, srcCityId, cityId)) {
                if (cities == null) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                City srcCity = cities[srcCityId];
                City city = cities[cityId];

                Game.Logic.NotificationManager.Notification notification;
                if (!srcCity.Worker.Notifications.tryGetValue(city, actionId, out notification)) {
                    reply_error(session, packet, Error.ACTION_NOT_FOUND);
                    return;
                }

                reply.addUInt32(notification.GameObject.X);
                reply.addUInt32(notification.GameObject.Y);

                session.write(reply);
            }
        }

        public void CmdGetRegion(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            ushort regionId;

            byte regionSubscribeCount;
            try {
                regionSubscribeCount = packet.getByte();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            reply.addByte(regionSubscribeCount);

            for (uint i = 0; i < regionSubscribeCount; ++i) {
                try {
                    regionId = packet.getUInt16();
                }
                catch (Exception) {                    
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (regionId >= Setup.Config.regions_count) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                reply.addUInt16(regionId);
                reply.addBytes(Global.World.getRegion(regionId).getBytes());
                reply.addBytes(Global.World.getRegion(regionId).getObjectBytes());
                Global.World.subscribeRegion(session, regionId);
            }

            byte regionUnsubscribeCount;
            try {
                regionUnsubscribeCount = packet.getByte();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            for (uint i = 0; i < regionUnsubscribeCount; ++i) {
                try {
                    regionId = packet.getUInt16();
                }
                catch (Exception) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                Global.World.unsubscribeRegion(session, regionId);
            }

            session.write(reply);
        }

        public void CmdGetCityRegion(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            ushort regionId;

            byte regionSubscribeCount;
            try {
                regionSubscribeCount = packet.getByte();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            reply.addByte(regionSubscribeCount);

            for (uint i = 0; i < regionSubscribeCount; ++i) {
                try {
                    regionId = packet.getUInt16();
                }
                catch (Exception) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                if (regionId >= Setup.Config.regions_count) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }
                 
                reply.addUInt16(regionId);
                reply.addBytes(Global.World.getCityRegion(regionId).getCityBytes());
            }            

            session.write(reply);
        }

        public void CmdGetTroopInfo(Session session, Packet packet) {
            City city;
            TroopObject troop;

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

            using (new MultiObjectLock(cityId, objectId, out city, out troop)) {
                if (city == null || troop == null) {
                    reply_error(session, packet, Error.OBJECT_NOT_FOUND);
                    return;
                }

                Packet reply = new Packet(packet);

                if (city.Owner == session.Player) {
                    reply.addByte((byte)troop.Stats.AttackRadius);
                    reply.addByte((byte)troop.Stats.Speed);                   

                    UnitTemplate template = new UnitTemplate(city);

                    reply.addByte(troop.Stub.FormationCount);
                    foreach (KeyValuePair<FormationType, Formation> formation in troop.Stub as IEnumerable<KeyValuePair<FormationType, Formation>>) {
                        reply.addByte((byte)formation.Key);
                        reply.addByte((byte)formation.Value.Count);
                        foreach (KeyValuePair<ushort, ushort> kvp in formation.Value) {
                            reply.addUInt16(kvp.Key);
                            reply.addUInt16(kvp.Value);
                            template[kvp.Key] = city.Template[kvp.Key];
                        }
                    }

                    reply.addUInt16((ushort)template.Size);
                    IEnumerator<KeyValuePair<ushort, BaseUnitStats>> templateIter = template.GetEnumerator();
                    while (templateIter.MoveNext()) {
                        KeyValuePair<ushort, BaseUnitStats> kvp = templateIter.Current;
                        reply.addUInt16(kvp.Key);
                        reply.addByte(kvp.Value.Lvl);
                    }
                    
                    PacketHelper.AddToPacket(new List<ReferenceStub>(troop.City.Worker.References.getReferences(troop)), reply);
                }

                session.write(reply);
            }
        }

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
                                reply.addByte((byte)prop.getValue(structure));
                                break;
                            case DataType.UShort:
                                reply.addUInt16((ushort)prop.getValue(structure));
                                break;
                            case DataType.UInt:
                                reply.addUInt32((uint)prop.getValue(structure));
                                break;
                            case DataType.String:
                                reply.addString((string)prop.getValue(structure));
                                break;
                            case DataType.Int:
                                reply.addInt32((int)prop.getValue(structure));
                                break;
                        }
                    }
                }
                
                PacketHelper.AddToPacket(new List<ReferenceStub>(structure.City.Worker.References.getReferences(structure)), reply);
                //        PacketHelper.AddToPacket(obj.Worker, packet);
                session.write(reply);

            }
        }
    }
}
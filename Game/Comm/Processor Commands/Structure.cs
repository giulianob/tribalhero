#region

using System;
using System.Linq;
using Game.Data;
using Game.Logic.Actions;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        public void CmdGetStructureInfo(Session session, Packet packet)
        {
            City city;
            Structure structure;

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

            using (new MultiObjectLock(cityId, objectId, out city, out structure))
            {
                if (city == null || structure == null)
                {
                    ReplyError(session, packet, Error.ObjectNotFound);
                    return;
                }

                var reply = new Packet(packet);
                reply.AddUInt16(structure.Stats.Base.Type);
                reply.AddByte(structure.Stats.Base.Lvl);
                if (session.Player == structure.City.Owner)
                {
                    reply.AddUInt16(structure.Stats.Labor);
                    reply.AddUInt16(structure.Stats.Hp);

                    foreach (var prop in PropertyFactory.GetProperties(structure.Type))
                    {
                        switch(prop.Type)
                        {
                            case DataType.Byte:
                                reply.AddByte((byte)prop.GetValue(structure));
                                break;
                            case DataType.UShort:
                                reply.AddUInt16((ushort)prop.GetValue(structure));
                                break;
                            case DataType.UInt:
                                reply.AddUInt32((uint)prop.GetValue(structure));
                                break;
                            case DataType.String:
                                reply.AddString((string)prop.GetValue(structure));
                                break;
                            case DataType.Int:
                                reply.AddInt32((int)prop.GetValue(structure));
                                break;
                            case DataType.Float:
                                reply.AddFloat((float)prop.GetValue(structure));
                                break;
                        }
                    }
                }
                else
                {
                    foreach (var prop in PropertyFactory.GetProperties(structure.Type, Visibility.Public))
                    {
                        switch(prop.Type)
                        {
                            case DataType.Byte:
                                reply.AddByte((byte)prop.GetValue(structure));
                                break;
                            case DataType.UShort:
                                reply.AddUInt16((ushort)prop.GetValue(structure));
                                break;
                            case DataType.UInt:
                                reply.AddUInt32((uint)prop.GetValue(structure));
                                break;
                            case DataType.String:
                                reply.AddString((string)prop.GetValue(structure));
                                break;
                            case DataType.Int:
                                reply.AddInt32((int)prop.GetValue(structure));
                                break;
                            case DataType.Float:
                                reply.AddFloat((float)prop.GetValue(structure));
                                break;
                        }
                    }
                }

                session.Write(reply);
            }
        }

        public void CmdGetForestInfo(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            Forest forest;

            uint forestId;

            try
            {
                forestId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (new MultiObjectLock(Global.World.Forests))
            {
                if (!Global.World.Forests.TryGetValue(forestId, out forest))
                {
                    ReplyError(session, packet, Error.ObjectNotFound);
                    return;
                }

                reply.AddFloat((float)(forest.Rate/Config.seconds_per_unit));
                reply.AddInt32(forest.Labor);
                reply.AddUInt32(UnixDateTime.DateTimeToUnix(forest.DepleteTime.ToUniversalTime()));
                PacketHelper.AddToPacket(forest.Wood, reply);
            }

            session.Write(reply);
        }

        public void CmdGetCityUsername(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            byte count;
            uint[] cityIds;
            try
            {
                count = packet.GetByte();
                cityIds = new uint[count];
                for (int i = 0; i < count; i++)
                    cityIds[i] = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            reply.AddByte(count);
            for (int i = 0; i < count; i++)
            {
                uint cityId = cityIds[i];
                City city;

                if (!Global.World.TryGetObjects(cityId, out city))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt32(cityId);
                reply.AddString(city.Name);
            }

            session.Write(reply);
        }

        public void CmdLaborMove(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            ushort count;
            Structure obj;
            City city;

            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                count = packet.GetUInt16();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (new MultiObjectLock(session.Player))
            {
                city = session.Player.GetCity(cityId);

                if (city == null || !city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                LaborMoveActiveAction lma;
                if (obj.Stats.Labor < count)
                {
                    //move from city to obj
                    count = (byte)(count - obj.Stats.Labor);

                    if (city.Resource.Labor.Value < count)
                    {
                        //not enough available in city
                        ReplyError(session, packet, Error.LaborNotEnough);
                        return;
                    }

                    if (obj.Stats.Labor + count > obj.Stats.Base.MaxLabor)
                    {
                        //adding too much to obj
                        ReplyError(session, packet, Error.LaborOverflow);
                        return;
                    }

                    lma = new LaborMoveActiveAction(cityId, objectId, true, count);
                }
                else if (obj.Stats.Labor > count)
                {
                    //move from obj to city
                    count = (byte)(obj.Stats.Labor - count);
                    lma = new LaborMoveActiveAction(cityId, objectId, false, count);
                }
                else
                {
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

        public void CmdTechnologyUpgrade(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            uint techId;

            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                techId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (new MultiObjectLock(session.Player))
            {
                City city = session.Player.GetCity(cityId);

                Structure obj;

                if (city == null || !city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var upgradeAction = new TechnologyUpgradeActiveAction(cityId, objectId, techId);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, upgradeAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
            }
        }

        public void CmdCancelAction(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            ushort actionId;

            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                actionId = packet.GetUInt16();
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

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.ActionUncancelable);
                    return;
                }

                Error ret;
                if ((ret = city.Worker.Cancel(actionId)) != Error.Ok)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
            }
        }

        public void CmdUpgradeStructure(Session session, Packet packet)
        {
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

            using (new MultiObjectLock(session.Player))
            {
                City city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var upgradeAction = new StructureUpgradeActiveAction(cityId, objectId);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, upgradeAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);

                return;
            }
        }

        public void CmdDowngradeStructure(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            uint targetId;

            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                targetId = packet.GetUInt32();
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

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var downgradeAction = new StructureDowngradeActiveAction(cityId, targetId);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, downgradeAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);

                return;
            }
        }

        public void CmdCreateStructure(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            uint x;
            uint y;
            ushort type;
            byte level;

            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                x = packet.GetUInt32();
                y = packet.GetUInt32();
                type = packet.GetUInt16();
                level = packet.GetByte();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (!Global.World.IsValidXandY(x, y))
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

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var buildaction = new StructureBuildActiveAction(cityId, type, x, y, level);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, buildaction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }

        public void CmdChangeStructure(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            ushort structureType;
            byte structureLvl;

            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                structureType = packet.GetUInt16();
                structureLvl = packet.GetByte();
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

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var changeAction = new StructureChangeActiveAction(cityId, objectId, structureType, structureLvl);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, changeAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }

        public void CmdSelfDestroyStructure(Session session, Packet packet)
        {
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

            using (new MultiObjectLock(session.Player))
            {
                City city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var destroyAction = new StructureSelfDestroyActiveAction(cityId, objectId);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, destroyAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }
        public void CmdCreateForestCamp(Session session, Packet packet)
        {
            uint cityId;
            uint forestId;
            ushort type;
            byte labor;

            try
            {
                cityId = packet.GetUInt32();
                forestId = packet.GetUInt32();
                type = packet.GetUInt16();
                labor = packet.GetByte();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            City city = session.Player.GetCity(cityId);
            if (city == null)
            {
                ReplyError(session, packet, Error.CityNotFound);
                return;
            }

            using (new CallbackLock(Global.World.Forests.CallbackLockHandler, new object[] {forestId}, city, Global.World.Forests))
            {
                // Get the lumbermill
                Structure lumbermill = city.FirstOrDefault(structure => ObjectTypeFactory.IsStructureType("Wood", structure));

                if (lumbermill == null || lumbermill.Lvl == 0)
                {
                    ReplyError(session, packet, Error.LumbermillUnavailable);
                    return;
                }

                var buildaction = new ForestCampBuildActiveAction(cityId, lumbermill.ObjectId, forestId, type, labor);
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(lumbermill), lumbermill, buildaction, lumbermill.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }
    }
}
#region

using System;
using System.Linq;
using Game.Data;
using Game.Logic.Actions;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class StructureCommandsModule : CommandModule
    {
        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.StructureInfo, StructureInfo);
            processor.RegisterCommand(Command.ForestInfo, ForestInfo);
            processor.RegisterCommand(Command.ForestCampCreate, CreateForestCamp);
            processor.RegisterCommand(Command.StructureBuild, CreateStructure);
            processor.RegisterCommand(Command.StructureUpgrade, UpgradeStructure);
            processor.RegisterCommand(Command.StructureDowngrade, DowngradeStructure);
            processor.RegisterCommand(Command.StructureChange, ChangeStructure);
            processor.RegisterCommand(Command.StructureLaborMove, LaborMove);
            processor.RegisterCommand(Command.StructureSelfDestroy, SelfDestroyStructure);
            processor.RegisterCommand(Command.ActionCancel, CancelAction);
            processor.RegisterCommand(Command.TechUpgrade, TechnologyUpgrade);
            processor.RegisterCommand(Command.CityUsernameGet, GetCityUsername);
        }

        private void StructureInfo(Session session, Packet packet)
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

            using (Concurrency.Current.Lock(cityId, objectId, out city, out structure))
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

                    foreach (var prop in Ioc.Kernel.Get<PropertyFactory>().GetProperties(structure.Type))
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
                    foreach (var prop in Ioc.Kernel.Get<PropertyFactory>().GetProperties(structure.Type, Visibility.Public))
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

        private void ForestInfo(Session session, Packet packet)
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

            using (Concurrency.Current.Lock(World.Current.Forests))
            {
                if (!World.Current.Forests.TryGetValue(forestId, out forest))
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

        private void GetCityUsername(Session session, Packet packet)
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

                if (!World.Current.TryGetObjects(cityId, out city))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt32(cityId);
                reply.AddString(city.Name);
            }

            session.Write(reply);
        }

        private void LaborMove(Session session, Packet packet)
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

            using (Concurrency.Current.Lock(session.Player))
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
                    count = (ushort)(count - obj.Stats.Labor);

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
                    count = (ushort)(obj.Stats.Labor - count);
                    lma = new LaborMoveActiveAction(cityId, objectId, false, count);
                }
                else
                {
                    ReplySuccess(session, packet);
                    return;
                }

                Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(obj), obj, lma, obj.Technologies);

                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
            }
        }

        private void TechnologyUpgrade(Session session, Packet packet)
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

            using (Concurrency.Current.Lock(session.Player))
            {
                City city = session.Player.GetCity(cityId);

                Structure obj;

                if (city == null || !city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var upgradeAction = new TechnologyUpgradeActiveAction(cityId, objectId, techId);
                Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(obj), obj, upgradeAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
            }
        }

        private void CancelAction(Session session, Packet packet)
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

            using (Concurrency.Current.Lock(session.Player))
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

        private void UpgradeStructure(Session session, Packet packet)
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

            using (Concurrency.Current.Lock(session.Player))
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
                Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(obj), obj, upgradeAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);

                return;
            }
        }

        private void DowngradeStructure(Session session, Packet packet)
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

            using (Concurrency.Current.Lock(session.Player))
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
                Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(obj), obj, downgradeAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);

                return;
            }
        }

        private void CreateStructure(Session session, Packet packet)
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

            if (!World.Current.IsValidXandY(x, y))
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

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var buildaction = new StructureBuildActiveAction(cityId, type, x, y, level);
                Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(obj), obj, buildaction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }

        private void ChangeStructure(Session session, Packet packet)
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

            using (Concurrency.Current.Lock(session.Player))
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
                Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(obj), obj, changeAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }

        private void SelfDestroyStructure(Session session, Packet packet)
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

            using (Concurrency.Current.Lock(session.Player))
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
                Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(obj), obj, destroyAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }

        private void CreateForestCamp(Session session, Packet packet)
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

            using (Concurrency.Current.Lock(World.Current.Forests.CallbackLockHandler, new object[] {forestId}, city, World.Current.Forests))
            {
                // Get the lumbermill
                Structure lumbermill = city.FirstOrDefault(structure => Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("Wood", structure));

                if (lumbermill == null || lumbermill.Lvl == 0)
                {
                    ReplyError(session, packet, Error.LumbermillUnavailable);
                    return;
                }

                var buildaction = new ForestCampBuildActiveAction(cityId, lumbermill.ObjectId, forestId, type, labor);
                Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(lumbermill), lumbermill, buildaction, lumbermill.Technologies);
                if(ret== Error.ActionTotalMaxReached)
                    ReplyError(session, packet, Error.LumbermillBusy);
                else if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                
                return;
            }
        }
    }
}
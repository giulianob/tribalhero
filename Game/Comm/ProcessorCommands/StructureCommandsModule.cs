#region

using System;
using System.Linq;
using System.Threading;
using Game.Data;
using Game.Data.Forest;
using Game.Logic.Actions;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class StructureCommandsModule : CommandModule
    {
        private readonly IActionFactory actionFactory;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly PropertyFactory propertyFactory;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly IForestManager forestManager;

        private readonly IWorld world;

        private readonly ILocker locker;

        public StructureCommandsModule(IActionFactory actionFactory,
                                       IStructureCsvFactory structureCsvFactory,
                                       IObjectTypeFactory objectTypeFactory,
                                       PropertyFactory propertyFactory,
                                       IForestManager forestManager,
                                       IWorld world,
                                       ILocker locker)
        {
            this.actionFactory = actionFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.propertyFactory = propertyFactory;
            this.forestManager = forestManager;
            this.world = world;
            this.locker = locker;
        }

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
            processor.RegisterCommand(Command.CityHasApBonus, GetCityHasApBonus);
        }

        private void GetCityHasApBonus(Session session, Packet packet)
        {
            ICity city;

            uint cityId;

            try
            {
                cityId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (locker.Lock(cityId, out city))
            {
                if (city == null)
                {
                    ReplyError(session, packet, Error.ObjectNotFound);
                    return;
                }

                var reply = new Packet(packet);
                reply.AddByte((byte)(city.AlignmentPoint >= 75m ? 1 : 0));

                session.Write(reply);
            }
        }

        private void StructureInfo(Session session, Packet packet)
        {
            ICity city;
            IStructure structure;

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

            using (locker.Lock(cityId, objectId, out city, out structure))
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
                    reply.AddUInt16((ushort)structure.Stats.Hp);

                    foreach (var prop in propertyFactory.GetProperties(structure.Type))
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
                    foreach (var prop in propertyFactory.GetProperties(structure.Type, Visibility.Public))
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

            IForest forest;
            if (!forestManager.TryGetValue(forestId, out forest))
            {
                ReplyError(session, packet, Error.ObjectNotFound);
                return;
            }

            using (locker.Lock(forest))
            {                
                reply.AddFloat((float)(forest.Rate / Config.seconds_per_unit));
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
                {
                    cityIds[i] = packet.GetUInt32();
                }
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
                ICity city;

                if (!world.TryGetObjects(cityId, out city))
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

            using (locker.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                IStructure obj;
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

                    lma = actionFactory.CreateLaborMoveActiveAction(cityId, objectId, true, count);
                }
                else if (obj.Stats.Labor > count)
                {
                    //move from obj to city
                    count = (ushort)(obj.Stats.Labor - count);
                    lma = actionFactory.CreateLaborMoveActiveAction(cityId, objectId, false, count);
                }
                else
                {
                    ReplySuccess(session, packet);
                    return;
                }

                Error ret = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(obj), obj, lma, obj.Technologies);

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

            using (locker.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                IStructure obj;

                if (city == null || !city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var upgradeAction = actionFactory.CreateTechnologyUpgradeActiveAction(cityId, objectId, techId);
                Error ret = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(obj),
                                                 obj,
                                                 upgradeAction,
                                                 obj.Technologies);
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

            using (locker.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                IStructure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.ActionUncancelable);
                    return;
                }

                Error ret;
                if ((ret = city.Worker.Cancel(actionId)) != Error.Ok)
                {
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
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

            using (locker.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                IStructure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var upgradeAction = actionFactory.CreateStructureUpgradeActiveAction(cityId, objectId);
                Error ret = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(obj),
                                                 obj,
                                                 upgradeAction,
                                                 obj.Technologies);
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

            using (locker.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                IStructure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var downgradeAction = actionFactory.CreateStructureDowngradeActiveAction(cityId, targetId);
                Error ret = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(obj),
                                                 obj,
                                                 downgradeAction,
                                                 obj.Technologies);
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

            if (!world.Regions.IsValidXandY(x, y))
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (locker.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                IStructure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var buildaction = actionFactory.CreateStructureBuildActiveAction(cityId, type, x, y, level);
                Error ret = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(obj),
                                                 obj,
                                                 buildaction,
                                                 obj.Technologies);
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

            using (locker.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                IStructure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var changeAction = actionFactory.CreateStructureChangeActiveAction(cityId,
                                                                                   objectId,
                                                                                   structureType,
                                                                                   structureLvl);
                Error ret = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(obj),
                                                 obj,
                                                 changeAction,
                                                 obj.Technologies);
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

            using (locker.Lock(session.Player))
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                IStructure obj;
                if (!city.TryGetStructure(objectId, out obj))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var destroyAction = actionFactory.CreateStructureSelfDestroyActiveAction(cityId, objectId);
                Error ret = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(obj),
                                                 obj,
                                                 destroyAction,
                                                 obj.Technologies);
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

            ICity city = session.Player.GetCity(cityId);
            if (city == null)
            {
                ReplyError(session, packet, Error.CityNotFound);
                return;
            }

            using (locker.Lock(forestManager.CallbackLockHandler,
                                             new object[] {forestId},
                                             city))
            {
                // Get the lumbermill
                IStructure lumbermill =
                        city.FirstOrDefault(structure => objectTypeFactory.IsStructureType("Wood", structure));

                if (lumbermill == null || lumbermill.Lvl == 0)
                {
                    ReplyError(session, packet, Error.LumbermillUnavailable);
                    return;
                }

                var buildaction = actionFactory.CreateForestCampBuildActiveAction(cityId,
                                                                                  lumbermill.ObjectId,
                                                                                  forestId,
                                                                                  type,
                                                                                  labor);
                Error ret = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(lumbermill),
                                                 lumbermill,
                                                 buildaction,
                                                 lumbermill.Technologies);
                if (ret == Error.ActionTotalMaxReached)
                {
                    ReplyError(session, packet, Error.LumbermillBusy);
                }
                else
                {
                    ReplyWithResult(session, packet, ret);
                }
            }
        }
    }
}
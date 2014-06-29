#region

using System;
using Game.Data;
using Game.Logic.Actions;
using Game.Setup;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class MiscCommandsModule : CommandModule
    {
        private readonly IActionFactory actionFactory;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly ILocker locker;

        public MiscCommandsModule(IActionFactory actionFactory, IStructureCsvFactory structureCsvFactory, ILocker locker)
        {
            this.actionFactory = actionFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.locker = locker;
        }

        public override void RegisterCommands(IProcessor processor)
        {
            processor.RegisterCommand(Command.CityCreate, CreateCity);
            processor.RegisterCommand(Command.ResourceGather, GatherResource);
            processor.RegisterCommand(Command.CityResourceWithdraw, WithdrawResource);
        }

        private void WithdrawResource(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;
            int crop;
            int gold;
            int iron;
            int wood;
            try
            {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
                crop = packet.GetInt32();
                gold = packet.GetInt32();
                iron = packet.GetInt32();
                wood = packet.GetInt32();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            locker.Lock(session.Player).Do(() =>
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

                var gatherAction = actionFactory.CreateResourceWithdrawActiveAction(cityId, objectId, new Resource(crop,gold,iron,wood));
                Error ret = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(obj),
                                                 obj,
                                                 gatherAction,
                                                 obj.Technologies);
                if (ret != 0)
                {
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            });
        }

        private void CreateCity(Session session, Packet packet)
        {
            uint cityId;
            uint x;
            uint y;
            string cityName;

            try
            {
                cityId = packet.GetUInt32();
                x = packet.GetUInt32();
                y = packet.GetUInt32();
                cityName = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            locker.Lock(session.Player).Do(() =>
            {
                ICity city = session.Player.GetCity(cityId);

                if (city == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var cityCreateAction = actionFactory.CreateCityCreatePassiveAction(cityId, x, y, cityName);
                Error ret = city.Worker.DoPassive(city[1], cityCreateAction, true);
                if (ret != 0)
                {
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            });
        }

        private void GatherResource(Session session, Packet packet)
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

            locker.Lock(session.Player).Do(() =>
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

                var gatherAction = actionFactory.CreateResourceGatherActiveAction(cityId, objectId);
                Error ret = city.Worker.DoActive(structureCsvFactory.GetActionWorkerType(obj),
                                                 obj,
                                                 gatherAction,
                                                 obj.Technologies);
                if (ret != 0)
                {
                    ReplyError(session, packet, ret);
                }
                else
                {
                    ReplySuccess(session, packet);
                }
            });
        }
    }
}
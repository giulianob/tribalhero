#region

using System;
using Game.Data;
using Game.Logic.Actions;
using Game.Setup;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class MiscCommandsModule : CommandModule
    {
        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.CityCreate, CreateCity);
            processor.RegisterCommand(Command.ResourceGather, GatherResource);
        }

        private void CreateCity(Session session, Packet packet) {
            uint cityId;
            uint x;
            uint y;
            string cityName;

            try {
                cityId = packet.GetUInt32();
                x = packet.GetUInt32();
                y = packet.GetUInt32();
                cityName = packet.GetString();
            } catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Concurrency.Current.Lock(session.Player)) {
                ICity city = session.Player.GetCity(cityId);

                if (city == null) {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var cityCreateAction = new CityCreatePassiveAction(cityId, x, y, cityName);
                Error ret = city.Worker.DoPassive(city[1], cityCreateAction, true);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }

        private void GatherResource(Session session, Packet packet)
        {
            uint cityId;
            uint objectId;

            try {
                cityId = packet.GetUInt32();
                objectId = packet.GetUInt32();
            } catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Concurrency.Current.Lock(session.Player)) {
                ICity city = session.Player.GetCity(cityId);

                if (city == null) {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                Structure obj;
                if (!city.TryGetStructure(objectId, out obj)) {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                var gatherAction = new ResourceGatherActiveAction(cityId, objectId);
                Error ret = city.Worker.DoActive(Ioc.Kernel.Get<StructureFactory>().GetActionWorkerType(obj), obj, gatherAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }
    }
}

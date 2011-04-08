#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Actions;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        public void CmdCityCreate(Session session, Packet packet) {
            uint cityId;
            uint objectId;
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

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.GetCity(cityId);

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

        public void CmdResourceGather(Session session, Packet packet)
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

            using (new MultiObjectLock(session.Player)) {
                City city = session.Player.GetCity(cityId);

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
                Error ret = city.Worker.DoActive(StructureFactory.GetActionWorkerType(obj), obj, gatherAction, obj.Technologies);
                if (ret != 0)
                    ReplyError(session, packet, ret);
                else
                    ReplySuccess(session, packet);
                return;
            }
        }

    }
}

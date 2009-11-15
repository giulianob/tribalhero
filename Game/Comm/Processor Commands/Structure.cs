using System;
using System.Collections.Generic;
using System.Text;
using Game.Logic;
using Game.Logic.Actions;
using Game.Setup;
using Game.Data;
using Game.Database;
using Game.Util;

namespace Game.Comm {
    public partial class Processor {
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

                if (city == null || !city.tryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                LaborMoveAction lma = null;
                if (obj.Labor < count) { //move from city to obj
                    count = (byte)(count - obj.Labor);

                    if (city.Resource.Labor.Value < count) { //not enough available in city
                        reply_error(session, packet, Error.LABOR_NOT_ENOUGH);
                        return;
                    }
                    else if (obj.Labor + count > obj.Stats.MaxLabor) { //adding too much to obj
                        reply_error(session, packet, Error.LABOR_OVERFLOW);
                        return;
                    }
                    lma = new LaborMoveAction(cityId, objectId, true, count);
                }
                else if (obj.Labor > count) { //move from obj to city
                    count = (byte)(obj.Labor - count);
                    lma = new LaborMoveAction(cityId, objectId, false, count);
                }
                else {
                    reply_success(session, packet);
                    return;
                }
                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(obj), obj, lma, obj.Technologies);

                if (ret != 0) {
                    reply_error(session, packet, ret);
                }
                else {
                    reply_success(session, packet);
                }
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

                if (city == null || !city.tryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                TechnologyUpgradeAction upgradeAction = new TechnologyUpgradeAction(cityId, objectId, techId);
                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(obj), obj, upgradeAction, obj.Technologies);
                if (ret != 0) {
                    reply_error(session, packet, ret);
                }
                else {
                    reply_success(session, packet);
                }
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
                    if ((ret = city.Worker.Cancel(actionId)) != Error.OK) {
                        reply_error(session, packet, ret);
                    }
                    else {
                        reply_success(session, packet);
                    }
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
                if (!city.tryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                StructureUpgradeAction upgradeAction = new StructureUpgradeAction(cityId, objectId);
                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(obj), obj, upgradeAction, obj.Technologies);
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
                if (!city.tryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                StructureBuildAction buildaction = new StructureBuildAction(cityId, type, x, y);
                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(obj), obj, buildaction, obj.Technologies);
                if (ret != 0) {
                    reply_error(session, packet, ret);
                }
                else {
                    reply_success(session, packet);
                }
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
                if (!city.tryGetStructure(objectId, out obj)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                StructureChangeAction changeAction = new StructureChangeAction(cityId, objectId, structureType, structureLvl, false, false);
                Error ret = city.Worker.doActive(StructureFactory.getActionWorkerType(obj), obj, changeAction, obj.Technologies);
                if (ret != 0) {
                    reply_error(session, packet, ret);
                }
                else {
                    reply_success(session, packet);
                }
                return;
            }
        }

    }
}

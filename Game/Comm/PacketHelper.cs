#region

using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Fighting;
using Game.Logic;
using Game.Map;
using Game.Util;

#endregion

namespace Game.Comm {
    public class PacketHelper {
        public static void AddToPacket(UnitTemplate template, Packet packet) {
            packet.AddUInt16((ushort) template.Size);
            foreach (KeyValuePair<ushort, BaseUnitStats> kvp in template) {
                packet.AddUInt16(kvp.Key);
                packet.AddByte(kvp.Value.Lvl);
            }
        }

        public static void AddToPacket(NotificationManager.Notification notification, Packet packet) {
            packet.AddUInt32(notification.Action.WorkerObject.City.Id);
            packet.AddUInt32(notification.GameObject.ObjectId);
            packet.AddUInt32(notification.Action.ActionId);
            packet.AddUInt16((ushort) notification.Action.Type);

            if (notification.Action is IActionTime) {
                IActionTime actionTime = notification.Action as IActionTime;
                if (actionTime.BeginTime == DateTime.MinValue)
                    packet.AddUInt32(0);
                else
                    packet.AddUInt32(UnixDateTime.DateTimeToUnix(actionTime.BeginTime.ToUniversalTime()));

                if (actionTime.EndTime == DateTime.MinValue)
                    packet.AddUInt32(0);
                else
                    packet.AddUInt32(UnixDateTime.DateTimeToUnix(actionTime.EndTime.ToUniversalTime()));
            } else {
                packet.AddUInt32(0);
                packet.AddUInt32(0);
            }
        }

        //sendRegularObject defines whether to send the state and labor count. Basicaly the sendRegularObject should be true
        //when sending the object where the client will accept it as a regular StructureObject and should be false if the client
        //will be accepting it as a CityObject
        //
        //These add to packet methods might need to be broken up a bit since this one has too many "cases"
        public static void AddToPacket(SimpleGameObject obj, Packet packet, bool sendRegularObject) {
            packet.AddByte(obj.Lvl);
            packet.AddUInt16(obj.Type);

            GameObject gameObj = obj as GameObject;
            if (gameObj == null || gameObj.City == null) {
                packet.AddUInt32(0); //playerid
                packet.AddUInt32(0); //cityid
            } else {
                packet.AddUInt32(gameObj.City.Owner.PlayerId);
                packet.AddUInt32(gameObj.City.Id);
            }

            packet.AddUInt32(obj.ObjectId);
            packet.AddUInt16((ushort) (obj.RelX));
            packet.AddUInt16((ushort) (obj.RelY));

            if (sendRegularObject) {
                packet.AddByte((byte) obj.State.Type);
                foreach (object parameter in obj.State.Parameters) {
                    if (parameter is byte)
                        packet.AddByte((byte) parameter);
                    else if (parameter is short)
                        packet.AddInt16((short) parameter);
                    else if (parameter is int)
                        packet.AddInt32((int) parameter);
                    else if (parameter is ushort)
                        packet.AddUInt16((ushort) parameter);
                    else if (parameter is uint)
                        packet.AddUInt32((uint) parameter);
                    else if (parameter is string)
                        packet.AddString((string) parameter);
                }

                if (gameObj != null && gameObj.ObjectId == 1) //main building, send radius
                    packet.AddByte(gameObj.City.Radius);
            } else if (obj is Structure) {
                //if obj is a structure and we are sending it as CityObj we include the labor
                packet.AddByte((obj as Structure).Stats.Labor);
            }
        }

        public static void AddToPacket(Resource resource, Packet packet) {
            packet.AddUInt32((uint) resource.Crop);
            packet.AddUInt32((uint) resource.Gold);
            packet.AddUInt32((uint) resource.Iron);
            packet.AddUInt32((uint) resource.Wood);
        }

        public static void AddToPacket(LazyValue value, Packet packet) {
            packet.AddInt32(value.RawValue);
            packet.AddInt32(value.Rate);
            packet.AddInt32(value.Upkeep);
            packet.AddInt32(value.Limit);
            packet.AddUInt32(UnixDateTime.DateTimeToUnix(value.LastRealizeTime.ToUniversalTime()));                        
        }


        public static void AddToPacket(LazyResource resource, Packet packet) {
            packet.AddInt32(resource.Crop.RawValue);
            packet.AddInt32(resource.Crop.Rate);
            packet.AddInt32(resource.Crop.Upkeep);
            packet.AddInt32(resource.Crop.Limit);
            packet.AddUInt32(UnixDateTime.DateTimeToUnix(resource.Crop.LastRealizeTime.ToUniversalTime()));

            packet.AddInt32(resource.Iron.RawValue);
            packet.AddInt32(resource.Iron.Rate);
            packet.AddInt32(resource.Iron.Limit);
            packet.AddUInt32(UnixDateTime.DateTimeToUnix(resource.Iron.LastRealizeTime.ToUniversalTime()));

            packet.AddInt32(resource.Gold.RawValue);
            packet.AddInt32(resource.Gold.Rate);
            packet.AddInt32(resource.Gold.Limit);
            packet.AddUInt32(UnixDateTime.DateTimeToUnix(resource.Gold.LastRealizeTime.ToUniversalTime()));

            packet.AddInt32(resource.Wood.RawValue);
            packet.AddInt32(resource.Wood.Rate);
            packet.AddInt32(resource.Wood.Limit);
            packet.AddUInt32(UnixDateTime.DateTimeToUnix(resource.Wood.LastRealizeTime.ToUniversalTime()));

            packet.AddInt32(resource.Labor.RawValue);
            packet.AddInt32(resource.Labor.Rate);
            packet.AddInt32(resource.Labor.Limit);
            packet.AddUInt32(UnixDateTime.DateTimeToUnix(resource.Wood.LastRealizeTime.ToUniversalTime()));
        }

        public static void AddToPacket(List<ReferenceStub> references, Packet packet) {
            packet.AddByte((byte) references.Count);
            foreach (ReferenceStub reference in references) {
                packet.AddUInt32(reference.Action.WorkerObject.WorkerId);
                packet.AddUInt32(reference.Action.ActionId);
            }
        }

        public static void AddToPacket(List<GameAction> actions, Packet packet, bool includeWorkerId) {
            packet.AddByte((byte) actions.Count);
            foreach (GameAction actionStub in actions)
                AddToPacket(actionStub, packet, includeWorkerId);
        }

        internal static void AddToPacket(GameAction actionStub, Packet packet, bool includeWorkerId) {
            if (includeWorkerId)
                packet.AddUInt32(actionStub.WorkerObject.WorkerId);

            if (actionStub is PassiveAction) {
                packet.AddByte(0);
                packet.AddUInt32(actionStub.ActionId);
                packet.AddUInt16((ushort) actionStub.Type);
            } else {
                packet.AddByte(1);
                packet.AddUInt32(actionStub.ActionId);
                packet.AddByte(((ActiveAction) actionStub).WorkerIndex);
                packet.AddUInt16(((ActiveAction) actionStub).ActionCount);
            }

            if (actionStub is IActionTime) {
                IActionTime actionTime = actionStub as IActionTime;
                if (actionTime.BeginTime == DateTime.MinValue)
                    packet.AddUInt32(0);
                else
                    packet.AddUInt32(UnixDateTime.DateTimeToUnix(actionTime.BeginTime.ToUniversalTime()));

                if (actionTime.EndTime == DateTime.MinValue)
                    packet.AddUInt32(0);
                else
                    packet.AddUInt32(UnixDateTime.DateTimeToUnix(actionTime.EndTime.ToUniversalTime()));
            } else {
                packet.AddUInt32(0);
                packet.AddUInt32(0);
            }
        }

        internal static void AddToPacket(TroopStub stub, Packet packet) {
            packet.AddUInt32(stub.City.Owner.PlayerId);
            packet.AddUInt32(stub.City.Id);

            packet.AddByte(stub.TroopId);
            packet.AddByte((byte)stub.State);

            //Add troop template
            packet.AddByte(stub.Template.Count);
            foreach (
                KeyValuePair<ushort, BattleStats> stats in
                    stub.Template as IEnumerable<KeyValuePair<ushort, BattleStats>>) {
                packet.AddUInt16(stats.Key);
                packet.AddByte(stats.Value.Base.Lvl);

                packet.AddUInt16(stats.Value.MaxHp);
                packet.AddUInt16(stats.Value.Atk);
                packet.AddUInt16(stats.Value.Def);
                packet.AddByte(stats.Value.Rng);
                packet.AddByte(stats.Value.Spd);
                packet.AddByte(stats.Value.Stl);
            }

            //Add state specific variables
            switch (stub.State) {
                case TroopState.MOVING:
                    packet.AddUInt32(stub.TroopObject.ObjectId);
                    packet.AddUInt32(stub.TroopObject.X);
                    packet.AddUInt32(stub.TroopObject.Y);
                    break;
                case TroopState.BATTLE:
                    if (stub.TroopObject != null) {
                        packet.AddUInt32(stub.TroopObject.ObjectId);
                        packet.AddUInt32(stub.TroopObject.X);
                        packet.AddUInt32(stub.TroopObject.Y);
                    } else {
                        packet.AddUInt32(stub.City.MainBuilding.ObjectId);
                        packet.AddUInt32(stub.City.MainBuilding.X);
                        packet.AddUInt32(stub.City.MainBuilding.Y);
                    }
                    break;
                case TroopState.STATIONED:
                case TroopState.BATTLE_STATIONED:
                    packet.AddUInt32(stub.StationedCity.MainBuilding.ObjectId);
                    packet.AddUInt32(stub.StationedCity.MainBuilding.X);
                    packet.AddUInt32(stub.StationedCity.MainBuilding.Y);
                    break;
            }

            //Actual formation and unit counts
            packet.AddByte(stub.FormationCount);
            foreach (Formation formation in stub) {
                packet.AddByte((byte) formation.Type);
                packet.AddByte((byte) formation.Count);
                foreach (KeyValuePair<ushort, ushort> kvp in formation) {
                    packet.AddUInt16(kvp.Key);
                    packet.AddUInt16(kvp.Value);
                }
            }
        }

        internal static void AddToPacket(List<CombatObject> list, Packet packet) {
            packet.AddUInt16((ushort) list.Count);
            foreach (CombatObject obj in list) {
                packet.AddUInt32(obj.PlayerId);
                packet.AddUInt32(obj.City.Id);
                packet.AddUInt32(obj.Id);
                packet.AddByte((byte) obj.ClassType);
                if (obj.ClassType == BattleClass.UNIT)
                    packet.AddByte(((ICombatUnit) obj).TroopStub.TroopId);
                else
                    packet.AddByte(1);
                packet.AddUInt16(obj.Type);
                packet.AddByte(obj.Lvl);
                packet.AddUInt32(obj.Hp);
            }
        }

        public static void AddLoginToPacket(Session session, Packet packet) {
            //Cities
            List<City> list = session.Player.GetCityList();
            packet.AddByte((byte)list.Count);
            foreach (City city in list) {
                city.Subscribe(session);
                packet.AddUInt32(city.Id);
                packet.AddString(city.Name);
                AddToPacket(city.Resource, packet);
                packet.AddByte(city.Radius);

                //City Actions
                AddToPacket(new List<GameAction>(city.Worker.GetVisibleActions()), packet, true);

                //Notifications
                packet.AddUInt16(city.Worker.Notifications.Count);
                foreach (NotificationManager.Notification notification in city.Worker.Notifications)
                    AddToPacket(notification, packet);

                //Structures
                List<Structure> structs = new List<Structure>(city);
                packet.AddUInt16((ushort)structs.Count);
                foreach (Structure structure in structs) {
                    packet.AddUInt16(Region.GetRegionIndex(structure));
                    AddToPacket(structure, packet, false);

                    packet.AddUInt16((ushort)structure.Technologies.OwnedTechnologyCount);
                    foreach (Technology tech in structure.Technologies) {
                        if (tech.ownerLocation != EffectLocation.OBJECT)
                            continue;
                        packet.AddUInt32(tech.Type);
                        packet.AddByte(tech.Level);
                    }
                }

                //City Troops
                packet.AddByte(city.Troops.Size);
                foreach (TroopStub stub in city.Troops)
                    AddToPacket(stub, packet);

                //Unit Template
                AddToPacket(city.Template, packet);
            }
        }
    }
}
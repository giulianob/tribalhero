using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Logic;
using Game.Setup;
using Game.Util;
using Game.Fighting;
using Game.Battle;

namespace Game.Comm {
    public class PacketHelper {
        public static void AddToPacket(UnitTemplate template, Packet packet) {
            packet.addUInt16((ushort)template.Size);
            foreach (KeyValuePair<ushort, UnitStats> kvp in template) {
                packet.addUInt16(kvp.Key);
                packet.addByte(kvp.Value.lvl);
            }
        }

        public static void AddToPacket(Game.Logic.NotificationManager.Notification notification, Packet packet) {
            packet.addUInt32(notification.Action.WorkerObject.City.CityId);
            packet.addUInt32(notification.GameObject.ObjectID);
            packet.addUInt16(notification.Action.ActionId);
            packet.addUInt16((ushort)notification.Action.Type);

            if (notification.Action is IActionTime) {
                IActionTime actionTime = notification.Action as IActionTime;
                if (actionTime.BeginTime == DateTime.MinValue)
                    packet.addUInt32(0);
                else
                    packet.addUInt32(UnixDateTime.DateTimeToUnix(actionTime.BeginTime.ToUniversalTime()));

                if (actionTime.EndTime == DateTime.MinValue)
                    packet.addUInt32(0);
                else
                    packet.addUInt32(UnixDateTime.DateTimeToUnix(actionTime.EndTime.ToUniversalTime()));
            }
            else {
                packet.addUInt32(0);
                packet.addUInt32(0);
            }   
        }

        //sendRegularObject defines whether to send the state and labor count. Basicaly the sendRegularObject should be true
        //when sending the object where the client will accept it as a regular StructureObject and should be false if the client
        //will be accepting it as a CityObject
        //
        //These add to packet methods might need to be broken up a bit since this one has too many "cases"
        public static void AddToPacket(GameObject obj, Packet packet, bool sendRegularObject) {
            packet.addByte(obj.Lvl);
            packet.addUInt16(obj.Type);
            if (obj.City == null) {
                packet.addUInt32(0);//playerid
                packet.addUInt32(0);//cityid
            }
            else {
                packet.addUInt32(obj.City.Owner.PlayerId);
                packet.addUInt32(obj.City.CityId);
            }
            packet.addUInt32(obj.ObjectID);
            packet.addUInt16((ushort)(obj.RelX));
            packet.addUInt16((ushort)(obj.RelY));

            if (sendRegularObject) {
                packet.addByte((byte)obj.State.Type);
                foreach (object parameter in obj.State.Parameters) {
                    if (parameter is byte) packet.addByte((byte)parameter);
                    else if (parameter is short) packet.addInt16((short)parameter);
                    else if (parameter is int) packet.addInt32((int)parameter);
                    else if (parameter is ushort) packet.addUInt16((ushort)parameter);
                    else if (parameter is uint) packet.addUInt32((uint)parameter);
                    else if (parameter is string) packet.addString((string)parameter);
                }

                if (obj.ObjectID == 1) //main building, send radius
                    packet.addByte(obj.City.Radius);
            }
            else if (obj is Structure) { //if obj is a structure and we are sending it as CityObj we include the labor
                packet.addByte((obj as Structure).Labor);
            }
        }

        public static void AddToPacket(Resource resource, Packet packet) {
            packet.addUInt32((uint)resource.Crop);
            packet.addUInt32((uint)resource.Gold);
            packet.addUInt32((uint)resource.Iron);
            packet.addUInt32((uint)resource.Wood);
        }

        public static void AddToPacket(LazyResource resource, Packet packet) {
            packet.addInt32(resource.Crop.RawValue);
            packet.addInt32(resource.Crop.Rate);
            packet.addInt32(resource.Crop.Limit);
            packet.addUInt32(UnixDateTime.DateTimeToUnix(resource.Crop.LastRealizeTime.ToUniversalTime()));

            packet.addInt32(resource.Iron.RawValue);
            packet.addInt32(resource.Iron.Rate);
            packet.addInt32(resource.Iron.Limit);
            packet.addUInt32(UnixDateTime.DateTimeToUnix(resource.Iron.LastRealizeTime.ToUniversalTime()));

            packet.addInt32(resource.Gold.RawValue);
            packet.addInt32(resource.Gold.Rate);
            packet.addInt32(resource.Gold.Limit);
            packet.addUInt32(UnixDateTime.DateTimeToUnix(resource.Gold.LastRealizeTime.ToUniversalTime()));

            packet.addInt32(resource.Wood.RawValue);
            packet.addInt32(resource.Wood.Rate);
            packet.addInt32(resource.Wood.Limit);
            packet.addUInt32(UnixDateTime.DateTimeToUnix(resource.Wood.LastRealizeTime.ToUniversalTime()));

            packet.addInt32(resource.Labor.RawValue);
            packet.addInt32(resource.Labor.Rate);
            packet.addInt32(resource.Labor.Limit);
            packet.addUInt32(UnixDateTime.DateTimeToUnix(resource.Wood.LastRealizeTime.ToUniversalTime()));
        }

        public static void AddToPacket(List<ReferenceStub> references, Packet packet) {
            packet.addByte((byte)references.Count);
            foreach (ReferenceStub reference in references) {
                packet.addUInt32(reference.Action.WorkerObject.WorkerId);
                packet.addUInt16(reference.Action.ActionId);
            }
        }

        public static void AddToPacket(List<Game.Logic.Action> actions, Packet packet, bool includeWorkerId) {
            packet.addByte((byte)actions.Count);
            foreach (Game.Logic.Action actionStub in actions)
                AddToPacket(actionStub, packet, includeWorkerId);            
        }

        internal static void AddToPacket(Game.Logic.Action actionStub, Packet packet, bool includeWorkerId) {
            if (includeWorkerId)
                packet.addUInt32(actionStub.WorkerObject.WorkerId);

            if (actionStub is PassiveAction) {
                packet.addByte(0);
                packet.addUInt16(actionStub.ActionId);
                packet.addUInt16((ushort)actionStub.Type);
            }
            else {
                packet.addByte(1);
                packet.addUInt16(actionStub.ActionId);
                packet.addByte((actionStub as ActiveAction).WorkerIndex);
                packet.addUInt16((actionStub as ActiveAction).ActionCount);
            }

            if (actionStub is IActionTime) {
                IActionTime actionTime = actionStub as IActionTime;
                if (actionTime.BeginTime == DateTime.MinValue)
                    packet.addUInt32(0);
                else
                    packet.addUInt32(UnixDateTime.DateTimeToUnix(actionTime.BeginTime.ToUniversalTime()));

                if (actionTime.EndTime == DateTime.MinValue)
                    packet.addUInt32(0);
                else
                    packet.addUInt32(UnixDateTime.DateTimeToUnix(actionTime.EndTime.ToUniversalTime()));
            }
            else {
                packet.addUInt32(0);
                packet.addUInt32(0);
            }         
        }

        internal static void AddToPacket(TroopStub stub, Packet packet) {
            packet.addUInt32(stub.City.Owner.PlayerId);
            packet.addUInt32(stub.City.CityId);            

            packet.addByte(stub.TroopId);
            packet.addByte((byte)stub.State);

            if (stub.TroopId == 1)            
                packet.addInt32(stub.Upkeep);

            switch (stub.State) {
                case TroopStub.TroopState.MOVING:
                    packet.addUInt32(stub.TroopObject.ObjectID);
                    packet.addUInt32(stub.TroopObject.X);
                    packet.addUInt32(stub.TroopObject.Y);
                    break;
                case TroopStub.TroopState.BATTLE:
                    if (stub.TroopObject != null) {
                        packet.addUInt32(stub.TroopObject.ObjectID);
                        packet.addUInt32(stub.TroopObject.X);
                        packet.addUInt32(stub.TroopObject.Y);
                    }
                    else {
                        packet.addUInt32(stub.City.MainBuilding.ObjectID);
                        packet.addUInt32(stub.City.MainBuilding.X);
                        packet.addUInt32(stub.City.MainBuilding.Y);
                    }
                    break;
                case TroopStub.TroopState.STATIONED:
                case TroopStub.TroopState.BATTLE_STATIONED:
                    packet.addUInt32(stub.StationedCity.MainBuilding.ObjectID);
                    packet.addUInt32(stub.StationedCity.MainBuilding.X);
                    packet.addUInt32(stub.StationedCity.MainBuilding.Y);
                    break;              
            }
            
            packet.addByte(stub.FormationCount);
            foreach (KeyValuePair<FormationType, Formation> formation in stub as IEnumerable<KeyValuePair<FormationType, Formation>>) {
                packet.addByte((byte)formation.Key);
                packet.addByte((byte)formation.Value.Count);
                foreach (KeyValuePair<ushort, ushort> kvp in formation.Value) {
                    packet.addUInt16(kvp.Key);
                    packet.addUInt16(kvp.Value);
                }
            }
        }

        internal static void AddToPacket(List<CombatObject> list, Packet packet) {
            packet.addUInt16((ushort)list.Count);
            foreach (CombatObject obj in list) {                
                packet.addUInt32(obj.PlayerId);
                packet.addUInt32(obj.City.CityId);
                packet.addUInt32(obj.Id);
                packet.addByte((byte)obj.ClassType);
                if (obj.ClassType == BattleClass.Unit)
                    packet.addByte((obj as ICombatUnit).TroopStub.TroopId);
                else
                    packet.addByte(1);
                packet.addUInt16(obj.Type);
                packet.addByte(obj.Lvl);
                packet.addUInt32(obj.HP);
            }
        }
    }
}
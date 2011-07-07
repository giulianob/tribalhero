#region

using System;
using System.Collections;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;
using System.Linq;
using NDesk.Options;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        public void CmdAssignmentList(Session session, Packet packet) {
            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            Tribe tribe;
            using (new MultiObjectLock(session.Player.Tribesman.Tribe.Id, out tribe))
            {
                if (tribe == null)
                {
                    ReplyError(session, packet, Error.TribeIsNull);
                    return;
                }

                packet.AddInt32(tribe.AssignmentCount);
                foreach (var assignment in (IEnumerable<Assignment>)tribe)
                {
                    PacketHelper.AddToPacket(assignment,packet);
                }
            }
            ReplySuccess(session,packet);
        }

        public void CmdAssignmentCreate(Session session, Packet packet) {
            uint cityId;
            uint targetCityId;
            uint targetObjectId;
            byte formationCount;
            AttackMode mode;
            DateTime time;
            try
            {
                mode = (AttackMode)packet.GetByte();
                cityId = packet.GetUInt32();
                targetCityId = packet.GetUInt32();
                targetObjectId = packet.GetUInt32();
                formationCount = packet.GetByte();
                time = DateTime.UtcNow.AddMinutes(packet.GetInt32());
            }
            catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (new MultiObjectLock(session.Player)) {
                if (session.Player.GetCity(cityId) == null) {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
            }


            Dictionary<uint, City> cities;
            using (new MultiObjectLock(out cities, cityId, targetCityId)) {
                if (cities == null) {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                City city = cities[cityId];

                City targetCity = cities[targetCityId];
                Structure targetStructure;

                if (city.Owner.PlayerId == targetCity.Owner.PlayerId) {
                    ReplyError(session, packet, Error.AttackSelf);
                    return;
                }

                
                if (!targetCity.TryGetStructure(targetObjectId, out targetStructure))
                {
                    ReplyError(session, packet, Error.ObjectStructureNotFound);
                    return;
                }

                var stub = new TroopStub();

                for (int f = 0; f < formationCount; ++f) {
                    FormationType formationType;
                    byte unitCount;
                    try {
                        formationType = (FormationType)packet.GetByte();
                        unitCount = packet.GetByte();
                    } catch (Exception) {
                        ReplyError(session, packet, Error.Unexpected);
                        return;
                    }

                    stub.AddFormation(formationType);

                    for (int u = 0; u < unitCount; ++u) {
                        ushort type;
                        ushort count;

                        try {
                            type = packet.GetUInt16();
                            count = packet.GetUInt16();
                        } catch (Exception) {
                            ReplyError(session, packet, Error.Unexpected);
                            return;
                        }

                        stub.AddUnit(formationType, type, count);
                    }
                }

                // Create troop stub                
                if (!Procedure.TroopStubCreate(city, stub)) {
                    ReplyError(session, packet, Error.TroopChanged);
                    return;
                }
                Global.DbManager.Save(stub);

                int id;
                Error ret = session.Player.Tribesman.Tribe.CreateAssignment(stub, targetStructure.X, targetStructure.Y, time, mode, out id);
                if (ret != 0) {
                    Procedure.TroopStubDelete(city, stub);
                    Global.DbManager.Rollback();
                    ReplyError(session, packet, ret);
                } else
                    ReplySuccess(session, packet);
            }
        }
 
        public void CmdAssignmentJoin(Session session, Packet packet) {
            uint cityId;
            int assignmentId;
            byte formationCount;
            try
            {
                cityId = packet.GetUInt32();
                assignmentId = packet.GetInt32();
                formationCount = packet.GetByte();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            Tribe tribe = session.Player.Tribesman.Tribe;
            using (new MultiObjectLock(session.Player, tribe)) {
                City city = session.Player.GetCity(cityId);
                if( city==null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }
                var stub = new TroopStub();

                for (int f = 0; f < formationCount; ++f) {
                    FormationType formationType;
                    byte unitCount;
                    try {
                        formationType = (FormationType)packet.GetByte();
                        unitCount = packet.GetByte();
                    } catch (Exception) {
                        ReplyError(session, packet, Error.Unexpected);
                        return;
                    }

                    stub.AddFormation(formationType);

                    for (int u = 0; u < unitCount; ++u) {
                        ushort type;
                        ushort count;

                        try {
                            type = packet.GetUInt16();
                            count = packet.GetUInt16();
                        } catch (Exception) {
                            ReplyError(session, packet, Error.Unexpected);
                            return;
                        }

                        stub.AddUnit(formationType, type, count);
                    }
                } 
                
                Procedure.TroopStubCreate(city, stub);
                Global.DbManager.Save(stub);

                Error error = tribe.JoinAssignment(assignmentId, stub);
                if (error != Error.Ok) {
                    Procedure.TroopStubDelete(city, stub);
                    Global.DbManager.Rollback();
                    ReplyError(session, packet, error);
                } else
                    ReplySuccess(session, packet);

            }
        }


    }
}
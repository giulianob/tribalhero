#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Tribe;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using System.Linq;
using Ninject;
using Persistance;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {

        public void CmdTribeSetDescription(Session session, Packet packet)
        {
            string description;
            try
            {
                description = packet.GetString();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(session.Player))
            {
                if (session.Player.Tribesman == null)
                {
                    ReplyError(session, packet, Error.TribeIsNull);
                    return;
                }

                if (!session.Player.Tribesman.Tribe.IsOwner(session.Player))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                if (description.Length > Player.MAX_DESCRIPTION_LENGTH)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                session.Player.Tribesman.Tribe.Description = description;

                ReplySuccess(session, packet);
            }
        }

        public void CmdTribeName(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            byte count;
            uint[] tribeIds;
            try
            {
                count = packet.GetByte();
                tribeIds = new uint[count];
                for (int i = 0; i < count; i++)
                    tribeIds[i] = packet.GetUInt32();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            reply.AddByte(count);
            for (int i = 0; i < count; i++)
            {
                uint tribeId = tribeIds[i];
                Tribe tribe;

                if (!Global.Tribes.TryGetValue(tribeId, out tribe))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt32(tribeId);
                reply.AddString(tribe.Name);
            }

            session.Write(reply);
        }

        public void CmdTribeInfo(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }
            var tribe = session.Player.Tribesman.Tribe;

            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(tribe))
            {
                reply.AddUInt32(tribe.Id);
                reply.AddUInt32(tribe.Owner.PlayerId);
                reply.AddByte(tribe.Level);
                reply.AddString(tribe.Name);
                reply.AddString(tribe.Description);
                PacketHelper.AddToPacket(tribe.Resource, reply);

                reply.AddInt16((short)tribe.Count);
                foreach (var tribesman in tribe)
                {
                    reply.AddUInt32(tribesman.Player.PlayerId);
                    reply.AddString(tribesman.Player.Name);
                    reply.AddInt32(tribesman.Player.GetCityCount());
                    reply.AddByte(tribesman.Rank);
                    PacketHelper.AddToPacket(tribesman.Contribution, reply);
                }

                // Incoming List
                var incomingList = tribe.GetIncomingList().ToList();
                reply.AddInt16((short)incomingList.Count());
                foreach (var incoming in incomingList)
                {
                    // Target
                    City targetCity;
                    if (Global.World.TryGetObjects(incoming.Action.To, out targetCity))
                    {                        
                        reply.AddUInt32(targetCity.Owner.PlayerId);
                        reply.AddUInt32(targetCity.Id);
                        reply.AddString(targetCity.Owner.Name);
                        reply.AddString(targetCity.Name);
                    }
                    else
                    {
                        reply.AddUInt32(0);
                        reply.AddUInt32(0);
                        reply.AddString("N/A");
                        reply.AddString("N/A");                        
                    }

                    // Attacker
                    reply.AddUInt32(incoming.Action.WorkerObject.City.Owner.PlayerId);
                    reply.AddUInt32(incoming.Action.WorkerObject.City.Id);
                    reply.AddString(incoming.Action.WorkerObject.City.Owner.Name);
                    reply.AddString(incoming.Action.WorkerObject.City.Name);

                    reply.AddUInt32(UnixDateTime.DateTimeToUnix(incoming.Action.EndTime.ToUniversalTime()));
                }

                // Assignment List
                reply.AddInt16(tribe.AssignmentCount);
                foreach (var assignment in (IEnumerable<Assignment>)tribe)
                {
                    PacketHelper.AddToPacket(assignment, reply);
                }

                session.Write(reply);
            }
        }

        public void CmdTribePublicInfo(Session session, Packet packet) {
            var reply = new Packet(packet);
            uint id;
            try {
                id = packet.GetUInt32();
            } catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            Tribe tribe;

            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(id,out tribe)) {
                reply.AddInt16((short)tribe.Count);
                foreach (var tribesman in tribe) {
                    reply.AddUInt32(tribesman.Player.PlayerId);
                    reply.AddString(tribesman.Player.Name);
                    reply.AddInt32(tribesman.Player.GetCityCount());
                    reply.AddByte(tribesman.Rank);
                }

                session.Write(reply);
            }
        }

        public void CmdTribeCreate(Session session, Packet packet)
        {
            string name;
            try
            {
                name = packet.GetString();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(session.Player))
            {
                if (session.Player.Tribesman != null)
                {
                    ReplyError(session, packet, Error.TribesmanAlreadyInTribe);
                    return;
                }

                if (Global.World.TribeNameTaken(name))
                {
                    ReplyError(session, packet, Error.TribeAlreadyExists);
                    return;
                }

                if (!Tribe.IsNameValid(name))
                {
                    ReplyError(session, packet, Error.TribeNameInvalid);
                    return;
                }

                if (session.Player.GetCityList().Count(city => city.Lvl >= 10) < 1)
                {
                    ReplyError(session, packet, Error.EffectRequirementNotMet);
                    return;
                }

                Tribe tribe = new Tribe(session.Player, name);
                Global.Tribes.Add(tribe.Id, tribe);
                Ioc.Kernel.Get<IDbManager>().Save(tribe);

                Tribesman tribesman = new Tribesman(tribe, session.Player, 0);
                tribe.AddTribesman(tribesman);
                ReplySuccess(session, packet);
            }
        }

        public void CmdTribeDelete(Session session, Packet packet)
        {
            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            Tribe tribe = session.Player.Tribesman.Tribe;
            using (Ioc.Kernel.Get<CallbackLock>().Lock(custom => ((IEnumerable<Tribesman>)tribe).ToArray(), new object[] { }, tribe))
            {
                if (!session.Player.Tribesman.Tribe.IsOwner(session.Player))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                if( tribe.AssignmentCount>0 )
                {
                    ReplyError(session, packet, Error.TribeHasAssignment);
                    return;                    
                }

                foreach (var tribesman in new List<Tribesman>(tribe))
                    tribe.RemoveTribesman(tribesman.Player.PlayerId);
                
                Global.Tribes.Remove(tribe.Id);
                Ioc.Kernel.Get<IDbManager>().Delete(tribe);
            }

            ReplySuccess(session, packet);
        }

        public void CmdTribeUpgrade(Session session, Packet packet)
        {
            if (!session.Player.Tribesman.Tribe.IsOwner(session.Player))
            {
                ReplyError(session, packet, Error.TribesmanNotAuthorized);
                return;
            }

            Tribe tribe = session.Player.Tribesman.Tribe;
            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(tribe))
            {
                if(tribe.Level>=20)
                {
                    ReplyError(session, packet, Error.TribeMaxLevel);
                    return;
                }
                Resource cost = Formula.GetTribeUpgradeCost(tribe.Level);
                if (!tribe.Resource.HasEnough(cost))
                {
                    ReplyError(session, packet, Error.ResourceNotEnough);
                    return;
                }

                tribe.Upgrade();
            }

            ReplySuccess(session, packet);
        }

        public void CmdTribeIncomingList(Session session, Packet packet)
        {
            /* Tribe t;
             List<NotificationManager.Notification> notifications;
             //t.Where(x => x.Player.GetCityList().Where(y => y.Worker.Notifications.Where(z => z.Action is AttackChainAction && z.Subscriptions.Any(city => city == y))));
             foreach (var city in t.SelectMany(tribesman => tribesman.Player.GetCityList()))
             {
                 notifications = new List<NotificationManager.Notification>(city.Worker.Notifications.Where(x => x.Action is AttackChainAction && x.Subscriptions.Any(y => y == city)));
             }*/
        }

    }
}
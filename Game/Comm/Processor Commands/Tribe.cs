#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Tribe;
using Game.Logic;
using Game.Logic.Actions;
using Game.Setup;
using Game.Util;
using System.Linq;
#endregion

namespace Game.Comm {
    public partial class Processor {
        public void CmdTribeInfo(Session session, Packet packet) {
            var reply = new Packet(packet);
            if (session.Player.Tribesman == null) {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }
            var tribe = session.Player.Tribesman.Tribe;

            using (new MultiObjectLock(tribe)) {
                reply.AddUInt32(tribe.Id);
                reply.AddUInt32(tribe.Owner.PlayerId);
                reply.AddByte(tribe.Level);
                reply.AddString(tribe.Name);
                reply.AddString(tribe.Desc);
                PacketHelper.AddToPacket(tribe.Resource, reply);

                reply.AddInt32(tribe.Count);
                foreach (var tribesman in tribe) {
                    reply.AddUInt32(tribesman.Player.PlayerId);
                    reply.AddInt32(tribesman.Player.GetCityCount());
                    reply.AddByte(tribesman.Rank);
                    PacketHelper.AddToPacket(tribesman.Contribution, reply);
                }
                session.Write(reply);
            }
        }

        public void CmdTribeCreate(Session session, Packet packet) {
            string name;
            try {
                name = packet.GetString();
            } catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (new MultiObjectLock(session.Player))
            {
                if (session.Player.Tribesman != null)
                {
                    ReplyError(session, packet, Error.TribesmanAlreadyInTribe);
                    return;
                }

                if (Global.Tribes.Any(x => x.Value.Name.Equals(name)))
                {
                    ReplyError(session, packet, Error.TribesmanAlreadyExists);
                    return;
                }

                if (Global.Tribes.ContainsKey(session.Player.PlayerId))
                {
                    ReplyError(session, packet, Error.TribeAlreadyExists);
                    return;
                }
                // deduct resource

                Tribe tribe = new Tribe(session.Player,name);
                Global.Tribes.Add(tribe.Id, tribe);
                Global.DbManager.Save(tribe);

                Tribesman tribesman = new Tribesman(tribe, session.Player, 0);
                tribe.AddTribesman(tribesman);
                ReplySuccess(session, packet);
            }
        }

        public void CmdTribeDelete(Session session, Packet packet) {
            if( session.Player.Tribesman==null )
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }
            if (!session.Player.Tribesman.Tribe.IsOwner(session.Player)) {
                ReplyError(session, packet, Error.TribesmanNotAuthorized);
                return;
            }

            Tribe tribe = session.Player.Tribesman.Tribe;
            using (new CallbackLock(custom => tribe.ToArray(), new object[] { }, tribe)) {
                foreach (var tribesman in new List<Tribesman>(tribe)) {
                    tribe.RemoveTribesman(tribesman.Player.PlayerId);
                }
                Global.Tribes.Remove(tribe.Id);
                Global.DbManager.Delete(tribe);
            }
        }

        public void CmdTribeUpdate(Session session, Packet packet) {
            string desc;
            try {
                desc = packet.GetString();
            } catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (!session.Player.Tribesman.Tribe.IsOwner(session.Player))
            {
                ReplyError(session, packet, Error.TribesmanNotAuthorized);
                return;
            }

            using (new MultiObjectLock(session.Player.Tribesman.Tribe)) {
                session.Player.Tribesman.Tribe.Desc = desc;
                Global.DbManager.Save(session.Player.Tribesman.Tribe);
            }
            ReplySuccess(session,packet);
        }

        public void CmdTribeUpgrade(Session session, Packet packet) {
            if (!session.Player.Tribesman.Tribe.IsOwner(session.Player)) {
                ReplyError(session, packet, Error.TribesmanNotAuthorized);
                return;
            }

            using (new MultiObjectLock(session.Player.Tribesman.Tribe)) {
                // deduct resource
                ++session.Player.Tribesman.Tribe.Level;
                Global.DbManager.Save(session.Player.Tribesman.Tribe);
            }
            ReplySuccess(session, packet);     
        }

        public void CmdTribeAssignmentList(Session session, Packet packet) {

        }
        public void CmdTribeAssignmentCreate(Session session, Packet packet)
        {
            
        }
        public void CmdTribeAssignmentJoin(Session session, Packet packet) {

        }

        public void CmdTribeIncomingList(Session session, Packet packet) {
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
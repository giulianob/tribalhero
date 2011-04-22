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
            var tribe = session.Player.Tribe;
            if (tribe == null) {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

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
                if (session.Player.Tribe != null)
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

                Tribe tribe = new Tribe(session.Player) { Name = name };
                Tribesman tribesman = new Tribesman(tribe,session.Player,0);
                session.Player.Tribe = tribe;
                tribe.AddTribesman(tribesman);
                Global.Tribes.Add(tribe.Id, tribe);
                Global.DbManager.Save(tribe, tribesman);
                ReplySuccess(session, packet);
            }
        }

        public void CmdTribeDelete(Session session, Packet packet) {

        }
        public void CmdTribeUpdate(Session session, Packet packet) {


        }
        public void CmdTribeUpgrade(Session session, Packet packet) {
        
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
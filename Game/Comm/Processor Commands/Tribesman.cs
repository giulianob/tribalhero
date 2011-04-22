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
        public void CmdTribesmanRequest(Session session, Packet packet)
        {
            uint playerId;
            try {
                playerId = packet.GetUInt32();
            } catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (session.Player.Tribe == null) {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            Dictionary<uint, Player> players;
            Tribe tribe = session.Player.Tribe;
            using (new MultiObjectLock(out players, playerId, tribe.Owner.PlayerId)) {
                if (!tribe.HasRight(session.Player.PlayerId, "Request"))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }
                if( players[playerId].Tribe!=null )
                {
                    ReplyError(session, packet, Error.TribesmanAlreadyInTribe);
                    return;
                }
                if (players[playerId].TribeRequest != 0)
                {
                    ReplyError(session, packet, Error.TribesmanPendingRequest);
                    return;
                }

                players[playerId].TribeRequest = tribe.Id;
                Global.DbManager.Save(players[playerId]);
                ReplySuccess(session, packet);
            }

        }

        public void CmdTribesmanConfirm(Session session, Packet packet) {
            bool isAccepting;
            try {
                isAccepting = packet.GetByte() == 0 ? false : true;
            } catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            Tribe tribe;

            using (new MultiObjectLock(session.Player))
            {
                if (session.Player.TribeRequest == 0)
                {
                    ReplyError(session, packet, Error.TribesmanNoRequest);
                    return;
                }

                if(!isAccepting)
                {
                    session.Player.TribeRequest = 0;
                    Global.DbManager.Save(session.Player);
                    ReplySuccess(session,packet);
                    return;
                }

                if (!Global.Tribes.TryGetValue(session.Player.TribeRequest, out tribe))
                {
                    ReplyError(session, packet, Error.TribeNotFound);
                    return;
                }
            }

            using (new MultiObjectLock(session.Player,tribe)) {
                Tribesman tribesman = new Tribesman(tribe, session.Player, 2);
                session.Player.Tribe = tribe;
                tribe.AddTribesman(tribesman);
                Global.DbManager.Save(tribesman);
                ReplySuccess(session, packet);
            }
        }

        public void CmdTribesmanAdd(Session session, Packet packet) {
            uint playerId;
            try {
                playerId = packet.GetUInt32();
            } catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if(session.Player.Tribe==null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            Dictionary<uint,Player> players;
            using (new MultiObjectLock(out players,playerId,session.Player.Tribe.Owner.PlayerId)) {
                Tribesman tribesman = new Tribesman(session.Player.Tribe, players[playerId],2);
                players[playerId].Tribe = session.Player.Tribe;
                session.Player.Tribe.AddTribesman(tribesman);
                ReplySuccess(session, packet);
            }
        }
        public void CmdTribesmanRemove(Session session, Packet packet) {
            uint playerId;
            try {
                playerId = packet.GetUInt32();
            } catch (Exception) {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (session.Player.Tribe == null) {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            Dictionary<uint, Player> players;
            using (new MultiObjectLock(out players, playerId, session.Player.Tribe.Owner.PlayerId)) {
                session.Player.Tribe.RemoveTribesman(playerId);
                ReplySuccess(session, packet);
            }
        }
        public void CmdTribesmanUpdate(Session session, Packet packet) {

        }

    }
}
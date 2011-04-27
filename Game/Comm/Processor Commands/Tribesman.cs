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

namespace Game.Comm
{
    public partial class Processor
    {
        public void CmdTribesmanRequest(Session session, Packet packet)
        {
            uint playerId;
            try
            {
                playerId = packet.GetUInt32();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            Dictionary<uint, Player> players;
            Tribe tribe = session.Player.Tribesman.Tribe;
            using (new MultiObjectLock(out players, playerId, tribe.Owner.PlayerId))
            {
                if (!tribe.HasRight(session.Player.PlayerId, "Request"))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }
                if (players[playerId].Tribesman.Tribe != null)
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

        public void CmdTribesmanConfirm(Session session, Packet packet)
        {
            bool isAccepting;
            try
            {
                isAccepting = packet.GetByte() == 0 ? false : true;
            }
            catch (Exception)
            {
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

                session.Player.TribeRequest = 0;
                Global.DbManager.Save(session.Player);

                if (!isAccepting)
                {
                    ReplySuccess(session, packet);
                    return;
                }

                if (!Global.Tribes.TryGetValue(session.Player.TribeRequest, out tribe))
                {
                    ReplyError(session, packet, Error.TribeNotFound);
                    return;
                }
            }

            using (new MultiObjectLock(session.Player, tribe))
            {
                Tribesman tribesman = new Tribesman(tribe, session.Player, 2);
                tribe.AddTribesman(tribesman);
                ReplySuccess(session, packet);
            }
        }

        public void CmdTribesmanAdd(Session session, Packet packet)
        {
            uint playerId;
            try
            {
                playerId = packet.GetUInt32();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            Dictionary<uint, Player> players;
            using (new MultiObjectLock(out players, playerId, session.Player.Tribesman.Tribe.Owner.PlayerId))
            {
                Tribesman tribesman = new Tribesman(session.Player.Tribesman.Tribe, players[playerId], 2);
                session.Player.Tribesman.Tribe.AddTribesman(tribesman);
                ReplySuccess(session, packet);
            }
        }
        public void CmdTribesmanRemove(Session session, Packet packet)
        {
            uint playerId;
            try
            {
                playerId = packet.GetUInt32();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            Dictionary<uint, Player> players;
            using (new MultiObjectLock(out players, playerId, session.Player.Tribesman.Tribe.Owner.PlayerId))
            {
                Tribe tribe = session.Player.Tribesman.Tribe;
                if (!tribe.HasRight(session.Player.PlayerId, "Kick"))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }
                if (tribe.IsOwner(session.Player))
                {
                    ReplyError(session, packet, Error.TribesmanIsOwner);
                    return;
                }
                session.Player.Tribesman.Tribe.RemoveTribesman(playerId);
                ReplySuccess(session, packet);
            }
        }
        public void CmdTribesmanUpdate(Session session, Packet packet)
        {
        }
        public void CmdTribesmanLeave(Session session, Packet packet)
        {
            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            using (new MultiObjectLock(session.Player.Tribesman.Tribe, session.Player))
            {
                Tribe tribe = session.Player.Tribesman.Tribe;

                if (tribe.IsOwner(session.Player))
                {
                    ReplyError(session, packet, Error.TribesmanIsOwner);
                    return;
                }
                tribe.RemoveTribesman(session.Player.PlayerId);
                ReplySuccess(session, packet);
            }
        }

    }
}
#region

using System;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Comm {
    public partial class Processor {
        public void CmdGetUsername(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            byte count;
            uint[] playerIds;
            try {
                count = packet.GetByte();
                playerIds = new uint[count];
                for (int i = 0; i < count; i++)
                    playerIds[i] = packet.GetUInt32();
            }
            catch (Exception) {
                ReplyError(session, packet, Error.UNEXPECTED);
                return;
            }

            reply.AddByte(count);
            foreach (uint playerId in playerIds) {
                Player player;
                if (!Global.Players.TryGetValue(playerId, out player)) {
                    ReplyError(session, packet, Error.UNEXPECTED);
                    return;
                }

                reply.AddUInt32(player.PlayerId);
                reply.AddString(player.Name);
            }

            session.write(reply);
        }
    }
}
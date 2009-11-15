using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Game.Data;
using Game.Setup;
using Game.Logic;
using Game.Fighting;

namespace Game.Comm {
    public partial class Processor {
        public void CmdGetUsername(Session session, Packet packet) {
            Packet reply = new Packet(packet);

            byte count;
            uint[] playerIds;
            try {
                count = packet.getByte();
                playerIds = new uint[count];
                for (int i = 0;i < count; i++)
                    playerIds[i] = packet.getUInt32();
            }
            catch (Exception) {
                reply_error(session, packet, Error.UNEXPECTED);
                return;
            }

            reply.addByte(count);   
            foreach (uint playerId in playerIds) {
                Player player;
                if (!Global.Players.TryGetValue(playerId, out player)) {
                    reply_error(session, packet, Error.UNEXPECTED);
                    return;
                }

                reply.addUInt32(player.PlayerId);
                reply.addString(player.Name);
            }

            session.write(reply);
        }
    }
}
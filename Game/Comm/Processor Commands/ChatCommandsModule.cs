using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

namespace Game.Comm.Processor_Commands
{
    class ChatCommandsModule : CommandModule
    {
        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.Chat, Chat);
        }

        public void Chat(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            byte type;
            string message;

            try
            {
                type = packet.GetByte();
                message = packet.GetString().Trim();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (string.IsNullOrEmpty(message) || message.Length > 500)
            {
                ReplyError(session, packet, Error.ChatMessageTooLong);
                return;                
            }

            Packet chatPacket;

            using (Concurrency.Current.Lock(session.Player))
            {
                if (SystemClock.Now.Subtract(session.Player.ChatFloodTime).TotalSeconds >= 5)
                {
                    session.Player.ChatFloodTime = SystemClock.Now;
                    session.Player.ChatFloodCount = 1;
                }
                else
                {
                    session.Player.ChatFloodCount++;

                    if (session.Player.ChatFloodCount >= 5)
                    {
                        ReplyError(session, packet, Error.ChatFloodWarning);
                        return;
                    }
                }

                chatPacket = new Packet(Command.Chat);
                chatPacket.AddByte(1);
                chatPacket.AddUInt32(session.Player.PlayerId);
                chatPacket.AddString(session.Player.Name);
                chatPacket.AddString(message);

                ReplySuccess(session, packet);
            }

            Global.Channel.Post("/GLOBAL", chatPacket);
        }
    }
}

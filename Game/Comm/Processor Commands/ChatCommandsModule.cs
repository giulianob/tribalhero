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
        enum ChatType
        {
            GLOBAL = 0,
            TRIBE = 1
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.Chat, Chat);
        }

        public void Chat(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            ChatType type;
            string message;

            try
            {
                type = (ChatType)packet.GetByte();
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

            string channel;

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

                

                switch (type)
                {
                    case ChatType.TRIBE:
                        if (session.Player.Tribesman == null)
                        {
                            ReplyError(session, packet, Error.TribesmanNotPartOfTribe);
                            return;
                        }
                        channel = string.Format("/TRIBE/{0}", session.Player.Tribesman.Tribe.Id);
                        break;
                    default:
                        channel = "/GLOBAL";
                        break;
                }

                chatPacket = new Packet(Command.Chat);
                chatPacket.AddByte((byte)type);
                chatPacket.AddUInt32(session.Player.PlayerId);
                chatPacket.AddString(session.Player.Name);
                chatPacket.AddString(message);

                ReplySuccess(session, packet);
            }

            Global.Channel.Post(channel, chatPacket);
        }
    }
}

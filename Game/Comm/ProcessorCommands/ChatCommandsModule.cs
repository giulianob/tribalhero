using System;
using System.IO;
using Game.Data;
using Game.Module;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

namespace Game.Comm.ProcessorCommands
{
    class ChatCommandsModule : CommandModule
    {
        private readonly StreamWriter writer;

        public ChatCommandsModule(StreamWriter writer)
        {
            this.writer = writer;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.Chat, Chat);
        }

        public void Chat(Session session, Packet packet)
        {
            Chat.ChatType type;
            string message;

            try
            {
                type = (Chat.ChatType)packet.GetByte();
                message = packet.GetString().Trim();
            }
            catch(Exception)
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
                switch(type)
                {
                    case Module.Chat.ChatType.Tribe:
                        if (session.Player.Tribesman == null)
                        {
                            ReplyError(session, packet, Error.TribesmanNotPartOfTribe);
                            return;
                        }
                        channel = string.Format("/TRIBE/{0}", session.Player.Tribesman.Tribe.Id);
                        break;

                    case Module.Chat.ChatType.Global:
                    case Module.Chat.ChatType.Offtopic:
                        // If player is muted then dont let him talk in global
                        if (session.Player.Muted)
                        {
                            ReplyError(session, packet, Error.ChatMuted);
                            return;
                        }

                        // Flood chat protection
                        int secondsFromLastMessage =
                                (int)SystemClock.Now.Subtract(session.Player.ChatLastMessage).TotalSeconds;

                        if (secondsFromLastMessage <= 10)
                        {
                            session.Player.ChatFloodCount++;
                        }
                        else if (secondsFromLastMessage > 120)
                        {
                            session.Player.ChatFloodCount = 0;
                        }
                        else if (secondsFromLastMessage > 15)
                        {
                            session.Player.ChatFloodCount = Math.Max(0, session.Player.ChatFloodCount - 2);
                        }

                        if (session.Player.ChatFloodCount >= 15)
                        {
                            ReplyError(session, packet, Error.ChatFloodWarning);
                            return;
                        }

                        session.Player.ChatLastMessage = SystemClock.Now;

                        channel = "/GLOBAL";
                        break;

                    default:
                        return;
                }

                writer.WriteLine(string.Format("[{0} {1}] {2}:{3}",
                                               SystemClock.Now,
                                               channel,
                                               session.Player.Name,
                                               message));

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
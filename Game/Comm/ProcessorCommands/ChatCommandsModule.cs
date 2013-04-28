using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Game.Data;
using Game.Module;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using log4net;

namespace Game.Comm.ProcessorCommands
{
    class ChatCommandsModule : CommandModule
    {
        private readonly Chat chat;

        public ChatCommandsModule(Chat chat)
        {
            this.chat = chat;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.Chat, Chat);
        }

        private static string RemoveDiacritics(string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder(stIn.Length);

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
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

            string channel;
            var chatState = session.Player.ChatState;

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

                    default:
                        // If player is muted then dont let him talk
                        if (session.Player.Muted > SystemClock.Now)
                        {
                            ReplyError(session, packet, Error.ChatMuted);
                            return;
                        }

                        // Minimum chat privilege for controlling when chat goes out of control
                        if (session.Player.Rights < Config.chat_min_level)
                        {
                            ReplyError(session, packet, Error.ChatDisabled);
                            return;
                        }

                        // Flood chat protection
                        int secondsFromLastMessage =
                                (int)SystemClock.Now.Subtract(chatState.ChatLastMessage).TotalSeconds;

                        if (secondsFromLastMessage <= 10)
                        {
                            chatState.ChatFloodCount++;
                        }
                        else if (secondsFromLastMessage > 120)
                        {
                            chatState.ChatFloodCount = 0;
                        }
                        else if (secondsFromLastMessage > 15)
                        {
                            chatState.ChatFloodCount = Math.Max(0, chatState.ChatFloodCount - 2);
                        }

                        if (chatState.ChatFloodCount >= 15)
                        {
                            ReplyError(session, packet, Error.ChatFloodWarning);
                            return;
                        }

                        chatState.ChatLastMessage = SystemClock.Now;

                        channel = "/GLOBAL";
                        break;
                }

                ReplySuccess(session, packet);
            }

            chat.SendChat(channel, type, session.Player.PlayerId, session.Player.Name, chatState.Distinguish, message);
        }
    }
}
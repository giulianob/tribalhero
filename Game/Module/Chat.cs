using Game.Comm;
using Game.Util;
using Ninject.Extensions.Logging;

namespace Game.Module
{
    public class Chat
    {
        public enum ChatType
        {
            Global = 0,

            Tribe = 1,

            HelpDesk = 2,

            Offtopic = 3,
        }

        private readonly Channel channel;

        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        public Chat(Channel channel)
        {
            this.channel = channel;
        }

        public void SendChat(string channelName, ChatType type, uint playerId, string playerName, bool distinguish, string message)
        {
            logger.Info("[{0} {1}] {3} {2}:{4}", SystemClock.Now, channel, playerName, playerId, message);

            var chatPacket = new Packet(Command.Chat);
            chatPacket.AddByte((byte)type);
            chatPacket.AddByte((byte)(distinguish ? 1 : 0));
            chatPacket.AddUInt32(playerId);
            chatPacket.AddString(playerName);
            chatPacket.AddString(message);

            channel.Post(channelName, chatPacket);
        }

        public void SendSystemChat(string messageId, params string[] messageArgs)
        {
            Packet chatPacket = new Packet(Command.SystemChat);
            chatPacket.AddString(messageId);
            chatPacket.AddByte((byte)messageArgs.Length);
            foreach (var messageArg in messageArgs)
            {
                chatPacket.AddString(messageArg);
            }

            channel.Post("/GLOBAL", chatPacket);
        }
    }
}
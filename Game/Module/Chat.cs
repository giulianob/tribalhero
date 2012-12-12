using Game.Comm;
using Game.Util;

namespace Game.Module
{
    public class Chat
    {
        public enum ChatType
        {
            Global = 0,

            Tribe = 1,

            Offtopic = 2,
        }

        private readonly Channel channel;

        public Chat(Channel channel)
        {
            this.channel = channel;
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
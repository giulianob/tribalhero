using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Comm;
using Game.Comm.ProcessorCommands;
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

        private Channel channel;

        public Chat(Channel channel)
        {
            this.channel = channel;
        }

        public void SendSystemChat(string messageId, params string[] messageArgs)
        {
            Packet chatPacket = new Packet(Command.SystemChat);
            chatPacket.AddString(messageId);
            foreach (var messageArg in messageArgs)
            {
                chatPacket.AddString(messageArg);
            }

            channel.Post("/GLOBAL", chatPacket);
        }
    }
}

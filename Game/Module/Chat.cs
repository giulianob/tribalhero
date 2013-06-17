using System.Collections.Generic;
using System.Linq;
using Game.Comm;
using Game.Data;
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

        private readonly IChannel channel;

        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        public Chat(IChannel channel)
        {
            this.channel = channel;
        }

        public void SendChat(string channelName,
                             ChatType type,
                             uint playerId,
                             string playerName,
                             IDictionary<AchievementTier, byte> achievements,
                             bool distinguish,
                             string message)
        {
            logger.Info("[{0} {1}] {3} {2}:{4}", SystemClock.Now, channel, playerName, playerId, message);

            byte goldAchievements;
            achievements.TryGetValue(AchievementTier.Gold, out goldAchievements);

            byte silverAchievements;
            achievements.TryGetValue(AchievementTier.Silver, out silverAchievements);

            byte bronzeAchievements;
            achievements.TryGetValue(AchievementTier.Bronze, out bronzeAchievements);

            var chatPacket = new Packet(Command.Chat);
            chatPacket.AddByte((byte)type);
            chatPacket.AddByte(goldAchievements);
            chatPacket.AddByte(silverAchievements);
            chatPacket.AddByte(bronzeAchievements);
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

        public void SendSystemChat(IChannelListener session, string messageId, params string[] messageArgs)
        {
            if (session == null)
            {
                return;
            }

            Packet chatPacket = new Packet(Command.SystemChat);
            chatPacket.AddString(messageId);
            chatPacket.AddByte((byte)messageArgs.Length);
            foreach (var messageArg in messageArgs)
            {
                chatPacket.AddString(messageArg);
            }

            session.OnPost(chatPacket);
        }
    }
}
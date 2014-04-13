using System;
using Game.Comm;

namespace Game.Util
{
    public interface IChannel
    {
        int SubscriberCount(string channelId);

        void Post(string channelId, Packet message);

        void Post(string channelId, Func<Packet> message);

        void Subscribe(IChannelListener session, string channelId);

        bool Unsubscribe(IChannelListener session, string channelId);

        int SubscriptionCount(IChannelListener session = null);

        bool Unsubscribe(IChannelListener session);

        bool Unsubscribe(string channelId);
    }
}
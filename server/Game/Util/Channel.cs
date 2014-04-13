#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Comm;

#endregion

namespace Game.Util
{
    public class Channel : IChannel
    {
        #region Structs

        private class Subscriber
        {
            public Subscriber(IChannelListener session)
            {
                Session = session;
                Channels = new List<string>();
            }

            public IChannelListener Session { get; private set; }

            public List<string> Channels { get; private set; }
        }

        #endregion

        #region Members

        private readonly ReaderWriterLockSlim channelLock = new ReaderWriterLockSlim();

        private readonly Dictionary<string, List<Subscriber>> subscribersByChannel = new Dictionary<string, List<Subscriber>>();

        private readonly Dictionary<IChannelListener, Subscriber> subscribersBySession = new Dictionary<IChannelListener, Subscriber>();

        #endregion

        #region Events

        public delegate void OnPost(IChannelListener session, object custom);

        #endregion

        #region Methods

        public int SubscriberCount(string channelId)
        {
            channelLock.EnterReadLock();
            try
            {
                List<Subscriber> subscribers;
                if (!subscribersByChannel.TryGetValue(channelId, out subscribers))
                {
                    return 0;
                }

                return subscribers.Count;
            }
            finally
            {
                channelLock.ExitReadLock();
            }
        }

        public void Post(string channelId, Packet message)
        {
            Post(channelId, () => message);
        }

        public void Post(string channelId, Func<Packet> message)
        {
			IChannelListener[] sessionsToPost;
            
            channelLock.EnterReadLock();
            try
            {
                List<Subscriber> subscribers;
                if (!subscribersByChannel.TryGetValue(channelId, out subscribers))
                {
                    return;
                }

				sessionsToPost = subscribersByChannel[channelId].Select(s => s.Session).ToArray();
            }
            finally
            {
                channelLock.ExitReadLock();
            }
			
			var hasPacket = false;
			Packet packet = null;
            foreach (var session in sessionsToPost)
            {
                if (!hasPacket)
                {
                    hasPacket = true;
                    packet = message();
                }			
				
                session.OnPost(packet);
            }			
        }

        public void Subscribe(IChannelListener session, string channelId)
        {
            channelLock.EnterWriteLock();
            try
            {
                Subscriber sub;
                List<Subscriber> sublist;
                // Check if the channel list already exists. To keep memory down, we don't keep around subscription lists
                // for channels that have no subscribers
                if (!subscribersByChannel.TryGetValue(channelId, out sublist))
                {
                    sublist = new List<Subscriber>();
                    subscribersByChannel.Add(channelId, sublist);
                }

                if (subscribersBySession.TryGetValue(session, out sub))
                {
                    if (sublist.Contains(sub))
                    {
                        return;
                    }

                    sub.Channels.Add(channelId);
                }
                else
                {
                    sub = new Subscriber(session);
                    sub.Channels.Add(channelId);
                    subscribersBySession.Add(session, sub);
                }

                sublist.Add(sub);
            }
            finally
            {
                channelLock.ExitWriteLock();
            }
        }

        public bool Unsubscribe(IChannelListener session, string channelId)
        {
            channelLock.EnterWriteLock();
            try
            {
                Subscriber sub;
                if (subscribersBySession.TryGetValue(session, out sub))
                {
                    sub.Channels.Remove(channelId);
                    if (sub.Channels.Count == 0)
                    {
                        subscribersBySession.Remove(session);
                    }

                    List<Subscriber> sublist;
                    if (subscribersByChannel.TryGetValue(channelId, out sublist))
                    {
                        sublist.Remove(sub);

                        if (sublist.Count == 0)
                        {
                            subscribersByChannel.Remove(channelId);
                        }
                    }

                    return true;
                }
            }
            finally
            {
                channelLock.ExitWriteLock();
            }
            return false;
        }

        public int SubscriptionCount(IChannelListener session = null)
        {
            channelLock.EnterReadLock();
            try
            {
                if (session != null)
                {
                    Subscriber sub;
                    if (subscribersBySession.TryGetValue(session, out sub))
                    {
                        return sub.Channels.Count;
                    }
                }
                else
                {
                    return subscribersByChannel.Values.Sum(subscription => subscription.Count);
                }
            }
            finally
            {
                channelLock.ExitReadLock();
            }
            return 0;
        }

        public bool Unsubscribe(IChannelListener session)
        {
            channelLock.EnterWriteLock();
            try
            {
                Subscriber sub;
                if (subscribersBySession.TryGetValue(session, out sub))
                {
                    foreach (var id in sub.Channels)
                    {
                        List<Subscriber> sublist;

                        if (!subscribersByChannel.TryGetValue(id, out sublist))
                        {
                            continue;
                        }

                        sublist.Remove(sub);

                        if (sublist.Count == 0)
                        {
                            subscribersByChannel.Remove(id);
                        }
                    }

                    sub.Channels.Clear();
                    subscribersBySession.Remove(session);
                    return true;
                }
            }
            finally
            {
                channelLock.ExitWriteLock();
            }

            return false;
        }

        public bool Unsubscribe(string channelId)
        {
            channelLock.EnterWriteLock();
            try
            {
                List<Subscriber> subs;
                if (subscribersByChannel.TryGetValue(channelId, out subs))
                {
                    foreach (var sub in subs)
                    {
                        sub.Channels.Remove(channelId);
                        if (sub.Channels.Count == 0)
                        {
                            subscribersBySession.Remove(sub.Session);
                        }
                    }
                    subscribersByChannel.Remove(channelId);
                    return true;
                }
            }
            finally
            {
                channelLock.ExitWriteLock();
            }

            return false;
        }

        #endregion
    }
}
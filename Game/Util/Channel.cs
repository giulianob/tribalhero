#region

using System;
using System.Collections.Generic;

#endregion

namespace Game.Util {
    public class Channel {
        #region Structs

        private struct Subscriber {
            public readonly IChannel session;
            public List<string> channels;

            public Subscriber(IChannel session) {
                this.session = session;
                channels = new List<string>();
            }
        }

        #endregion

        #region Members

        private readonly Dictionary<IChannel, Subscriber> subscribersBySession = new Dictionary<IChannel, Subscriber>();
        private readonly Dictionary<string, List<Subscriber>> subscribersByChannel = new Dictionary<string, List<Subscriber>>();
        private readonly object channelLock = new Object();

        #endregion

        #region Events

        public delegate void OnPost(IChannel session, object custom);

        #endregion

        #region Methods

        public void Post(string channelId, object message) {
            lock (channelLock) {
                if (!subscribersByChannel.ContainsKey(channelId))
                    return;
                foreach (Subscriber sub in subscribersByChannel[channelId])
                    sub.session.OnPost(message);
            }
        }

        public void Subscribe(IChannel session, string channelId) {
            lock (channelLock) {
                Subscriber sub;
                List<Subscriber> sublist;
                if (!subscribersByChannel.TryGetValue(channelId, out sublist)) {
                    sublist = new List<Subscriber>();
                    subscribersByChannel.Add(channelId, sublist);
                }
                if (subscribersBySession.TryGetValue(session, out sub))
                    sub.channels.Add(channelId);
                else {
                    sub = new Subscriber(session);
                    sub.channels.Add(channelId);
                    subscribersBySession.Add(session, sub);
                }
                sublist.Add(sub);
            }
            return;
        }

        public bool Unsubscribe(IChannel session, string channelId) {
            lock (channelLock) {
                Subscriber sub;
                if (subscribersBySession.TryGetValue(session, out sub)) {
                    sub.channels.Remove(channelId);
                    if (sub.channels.Count == 0)
                        subscribersBySession.Remove(session);

                    List<Subscriber> sublist;
                    if (subscribersByChannel.TryGetValue(channelId, out sublist)) {
                        sublist.Remove(sub);

                        if (sublist.Count == 0)
                            subscribersByChannel.Remove(channelId);
                    }

                    return true;
                }
            }
            return false;
        }

        public bool Unsubscribe(IChannel session) {
            lock (channelLock) {
                Subscriber sub;
                if (subscribersBySession.TryGetValue(session, out sub)) {
                    foreach (string id in sub.channels) {
                        List<Subscriber> sublist;

                        if (!subscribersByChannel.TryGetValue(id, out sublist))
                            continue;

                        sublist.Remove(sub);

                        if (sublist.Count == 0)
                            subscribersByChannel.Remove(id);
                    }

                    sub.channels = new List<string>();
                    subscribersBySession.Remove(session);
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
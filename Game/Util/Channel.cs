#region

using System;
using System.Collections.Generic;

#endregion

namespace Game.Util {
    public class Channel {
        #region Structs

        private struct Subscriber {
            public IChannel session;
            public List<string> channels;

            public Subscriber(IChannel session) {
                this.session = session;
                channels = new List<string>();
            }
        }

        #endregion

        #region Members

        private Dictionary<IChannel, Subscriber> subscribers_by_session = new Dictionary<IChannel, Subscriber>();
        private Dictionary<string, List<Subscriber>> subscribers_by_channel = new Dictionary<string, List<Subscriber>>();
        private object channelLock = new Object();

        #endregion

        #region Events

        public delegate void OnPost(IChannel session, object custom);

        #endregion

        #region Methods

        public void Post(string channel_id, object message) {
            lock (channelLock) {
                if (!subscribers_by_channel.ContainsKey(channel_id))
                    return;
                foreach (Subscriber sub in subscribers_by_channel[channel_id])
                    sub.session.OnPost(message);
            }
        }

        public void Subscribe(IChannel session, string channel_id) {
            lock (channelLock) {
                Subscriber sub;
                List<Subscriber> sublist;
                if (!subscribers_by_channel.TryGetValue(channel_id, out sublist)) {
                    sublist = new List<Subscriber>();
                    subscribers_by_channel.Add(channel_id, sublist);
                }
                if (subscribers_by_session.TryGetValue(session, out sub))
                    sub.channels.Add(channel_id);
                else {
                    sub = new Subscriber(session);
                    sub.channels.Add(channel_id);
                    subscribers_by_session.Add(session, sub);
                }
                sublist.Add(sub);
            }
            return;
        }

        public bool Unsubscribe(IChannel session, string channel_id) {
            lock (channelLock) {
                Subscriber sub;
                if (subscribers_by_session.TryGetValue(session, out sub)) {
                    sub.channels.Remove(channel_id);
                    if (sub.channels.Count == 0)
                        subscribers_by_session.Remove(session);

                    List<Subscriber> sublist;
                    if (subscribers_by_channel.TryGetValue(channel_id, out sublist)) {
                        sublist.Remove(sub);

                        if (sublist.Count == 0)
                            subscribers_by_channel.Remove(channel_id);
                    }

                    return true;
                }
            }
            return false;
        }

        public bool Unsubscribe(IChannel session) {
            lock (channelLock) {
                Subscriber sub;
                if (subscribers_by_session.TryGetValue(session, out sub)) {
                    foreach (string id in sub.channels) {
                        List<Subscriber> sublist;
                        if (subscribers_by_channel.TryGetValue(id, out sublist)) {
                            sublist.Remove(sub);

                            if (sublist.Count == 0)
                                subscribers_by_channel.Remove(id);
                        }
                    }

                    sub.channels = new List<string>();
                    subscribers_by_session.Remove(session);
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
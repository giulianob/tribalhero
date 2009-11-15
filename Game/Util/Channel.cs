using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Util {

    public class Channel {
        #region Structs
        struct Subscriber {
            public IChannel session;
            public List<int> channels;
            public Subscriber(IChannel session) {
                this.session = session;
                channels = new List<int>();
            }
        }
        #endregion

        #region Members
        Dictionary<IChannel, Subscriber> subscribers_by_session = new Dictionary<IChannel, Subscriber>();
        Dictionary<int, List<Subscriber>> subscribers_by_channel = new Dictionary<int, List<Subscriber>>();
        object channelLock = new Object();
        #endregion

        #region Events
        public delegate void OnPost(IChannel session, object custom);
        #endregion

        #region Methods
        public void post(object message) {
            post(0, message);
        }

        public void post(int channel_id, object message) {
            lock (channelLock) {
                if (!subscribers_by_channel.ContainsKey(channel_id)) return;
                foreach (Subscriber sub in subscribers_by_channel[channel_id]) {
                    sub.session.OnPost(message);
                }
            }
        }

        public void subscribe(IChannel session) {
            subscribe(session, 0);
        }

        public void subscribe(IChannel session, int channel_id) {
            lock (channelLock) {
                Subscriber sub;
                List<Subscriber> sublist;
                if (!subscribers_by_channel.TryGetValue(channel_id, out sublist)) {
                    sublist = new List<Subscriber>();
                    subscribers_by_channel.Add(channel_id, sublist);
                }
                if (subscribers_by_session.TryGetValue(session, out sub)) {
                    sub.channels.Add(channel_id);
                }
                else {
                    sub = new Subscriber(session);
                    sub.channels.Add(channel_id);
                    subscribers_by_session.Add(session, sub);
                }
                sublist.Add(sub);
            }
            return;
        }

        public bool unsubscribe(IChannel session) {
            return unsubscribe(session, 0);
        }

        public bool unsubscribe(IChannel session, int channel_id) {
            lock (channelLock) {
                Subscriber sub;
                if (subscribers_by_session.TryGetValue(session, out sub)) {                    
                    sub.channels.Remove(channel_id);
                    if (sub.channels.Count == 0) subscribers_by_session.Remove(session);

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

        public bool unsubscribeAll(IChannel session) {
            lock (channelLock) {
                
                Subscriber sub;
                if (subscribers_by_session.TryGetValue(session, out sub)) {
                    foreach (int id in sub.channels) {
                        subscribers_by_channel.Remove(id);
                    }
                    subscribers_by_session.Remove(session);
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}

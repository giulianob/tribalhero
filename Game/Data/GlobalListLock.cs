using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Game.Data {
    public class GlobalLockList<TKey, TValue> {
        Dictionary<TKey, TValue> data = new Dictionary<TKey, TValue>();
        
        /*
        public void Add(TKey key, TValue value) {
            if (Global.ListLock.IsReaderLockHeld) throw new Exception("Deadlock may occur");

            Global.ListLock.AcquireWriterLock(Timeout.Infinite);
            data.Add(key, value);
            Global.ListLock.ReleaseWriterLock();
        }

        public void Remove(TKey key, TValue value) {
            if (Global.ListLock.IsReaderLockHeld) throw new Exception("Deadlock may occur");

            Global.ListLock.AcquireWriterLock(Timeout.Infinite);
            data.Add(key, value);
            Global.ListLock.ReleaseWriterLock();
        }
        
        public bool TryGetValue(TKey key, out TValue value) {
            return data.TryGetValue(key, out value);
        }
        
        public bool ContainsKey(TKey key) {
            return data.ContainsKey(key);
        }

        public TValue this[TKey key] {
            get {
                return data[key];
            }
            set {
                data[key] = value;
            }
        }
         */
    }
}
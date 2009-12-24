using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using System.Collections;
using System.Threading;

namespace Game.Map {

    class AllObjectEnum : IEnumerator {
        #region Members
        bool isNew = true;
        private ObjectList _objectList;
        private Dictionary<int, List<GameObject>>.Enumerator itr;
        private List<GameObject>.Enumerator list_itr;
        #endregion

        #region Constructors
        public AllObjectEnum(ObjectList objectList) {
            _objectList = objectList;
            itr = _objectList.dict.GetEnumerator();
        }
        #endregion

        #region IEnumerator Members

        public void Reset() {
            itr = _objectList.dict.GetEnumerator();
        }

        public object Current {
            get {
                return list_itr.Current;
            }
        }

        public bool MoveNext() {
            if (isNew == true) {
                isNew = false;
                if (itr.MoveNext()) {
                    list_itr = itr.Current.Value.GetEnumerator();
                    
                    return list_itr.MoveNext();
                }
                else {
                    return false;
                }
            }
            else {
                if (list_itr.MoveNext()) {
                    return true;
                }
                else {
                    if (itr.MoveNext()) {
                        list_itr = itr.Current.Value.GetEnumerator();

                        return list_itr.MoveNext();
                    }
                    else {
                        return false;
                    }
                }
            }
        }

        #endregion
    }

    public class ObjectList : IEnumerable {
        #region Members
        internal Dictionary<int, List<GameObject>> dict = new Dictionary<int, List<GameObject>>();
        ushort count;
        #endregion

        #region Properties
        public ushort Count {
            get { return count; }
        }
        #endregion

        #region Methods
        internal void addGameObject(GameObject obj) {
            add(obj, Region.getTileIndex(obj.X, obj.Y));
        }

        void add(GameObject obj, int index) {
            List<GameObject> list;

            if (dict.TryGetValue(index, out list)) {
                list.Add(obj);
            }
            else {
                list = new List<GameObject>();
                list.Add(obj);
                dict[index] = list;
            }

            ++count;
        }

        internal bool remove(GameObject obj) {
            return remove(obj, obj.X, obj.Y);
        }

        internal bool remove(GameObject obj, uint origX, uint origY) {
            List<GameObject> list;
            int index = Region.getTileIndex(origX, origY);

            if (dict.TryGetValue(index, out list)) {
                --count;
                bool ret = list.Remove(obj);

                if (list.Count == 0)
                    dict.Remove(index); //Remove list if it is empty

                return ret;
            }

            return true;
        }

        internal List<GameObject> get(uint x, uint y) {
            List<GameObject> list;

            int index = Region.getTileIndex(x, y);

            if (dict.TryGetValue(index, out list)) {                
                return new List<GameObject>(list);
            }
            else
                return new List<GameObject>();
        }
        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator() {
            return new AllObjectEnum(this);
        }

        #endregion
    }
}
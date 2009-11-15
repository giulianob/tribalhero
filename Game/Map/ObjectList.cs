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
        private Dictionary<int, List<object>>.Enumerator itr;
        private List<object>.Enumerator list_itr;
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

                    if (list_itr.Current is ObjectLink)
                        return MoveNext();
                    else
                        return list_itr.MoveNext();
                }
                else {
                    return false;
                }
            }
            else {
                if (list_itr.MoveNext()) {
                    if (list_itr.Current is ObjectLink)
                        return MoveNext();
                    else
                        return true;
                }
                else {
                    if (itr.MoveNext()) {
                        list_itr = itr.Current.Value.GetEnumerator();

                        if (list_itr.Current is ObjectLink)
                            return MoveNext();
                        else
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
        internal Dictionary<int, List<object>> dict = new Dictionary<int, List<object>>();
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

        internal void addGameObjectLink(GameObject obj, uint x, uint y) {
            add(new ObjectLink(obj), Region.getTileIndex(x, y));
        }

        void add(object obj, int index) {
            List<object> list;

            if (dict.TryGetValue(index, out list)) {
                list.Add(obj);
                dict[index] = list;
            }
            else {
                list = new List<object>();
                list.Add(obj);
                dict[index] = list;
            }

            ++count;
        }

        internal bool remove(GameObject obj) {
            return remove(obj, obj.X, obj.Y);
        }

        internal bool remove(object obj, uint origX, uint origY) {
            List<object> list;
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
            List<object> list;

            int index = Region.getTileIndex(x, y);

            if (dict.TryGetValue(index, out list)) {
                List<GameObject> objList = new List<GameObject>();
                foreach (object obj in list) {
                    GameObject gameObj;
                    if (obj is GameObject) {
                        gameObj = (obj as GameObject);
                    }
                    else if (obj is ObjectLink) {
                        gameObj = (obj as ObjectLink).Value;
                    }
                    else
                        continue;

                    if (objList.Contains(gameObj))
                        continue;

                    objList.Add(gameObj);
                }

                return objList;
            }
            else
                return new List<GameObject>();
        }
        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator() {
            return (IEnumerator)new AllObjectEnum(this);
        }

        #endregion
    }
}
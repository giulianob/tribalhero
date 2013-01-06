#region

using System;
using System.Collections;
using System.Collections.Generic;
using Game.Data;

#endregion

namespace Game.Map
{
    public partial class ObjectList : IEnumerable<ISimpleGameObject>
    {
        #region Members

        internal readonly Dictionary<int, List<ISimpleGameObject>> Dict = new Dictionary<int, List<ISimpleGameObject>>();

        #endregion

        #region Properties

        public ushort Count { get; private set; }

        #endregion

        #region Methods

        internal void AddGameObject(ISimpleGameObject obj)
        {
            Add(obj, Region.GetTileIndex(obj.X, obj.Y));
        }

        private void Add(ISimpleGameObject obj, int index)
        {
            List<ISimpleGameObject> list;

            if (Dict.TryGetValue(index, out list))
            {
                if (list.Contains(obj))
                {
                    throw new Exception("WTF");
                }
                list.Add(obj);
            }
            else
            {
                list = new List<ISimpleGameObject> {obj};
                Dict[index] = list;
            }

            ++Count;
        }

        internal bool Remove(ISimpleGameObject obj)
        {
            return Remove(obj, obj.X, obj.Y);
        }

        internal bool Remove(ISimpleGameObject obj, uint origX, uint origY)
        {
            List<ISimpleGameObject> list;
            int index = Region.GetTileIndex(origX, origY);

            if (Dict.TryGetValue(index, out list))
            {
                --Count;
                bool ret = list.Remove(obj);

                if (list.Count == 0)
                {
                    Dict.Remove(index); //Remove list if it is empty
                }

                return ret;
            }

            return true;
        }

        internal List<ISimpleGameObject> Get(uint x, uint y)
        {
            List<ISimpleGameObject> list;

            int index = Region.GetTileIndex(x, y);

            if (Dict.TryGetValue(index, out list))
            {
                return new List<ISimpleGameObject>(list);
            }

            return new List<ISimpleGameObject>();
        }

        #endregion

        public IEnumerator<ISimpleGameObject> GetEnumerator()
        {
            return new AllObjectEnum(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new AllObjectEnum(this);
        }
    }
}
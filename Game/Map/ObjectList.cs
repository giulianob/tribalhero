#region

using System;
using System.Collections;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using Game.Util;
using Ninject;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Map
{
    public partial class ObjectList : IEnumerable<ISimpleGameObject>
    {
        #region Members

        private readonly Dictionary<int, List<ISimpleGameObject>> objects = new Dictionary<int, List<ISimpleGameObject>>();

        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

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

            if (objects.TryGetValue(index, out list))
            {
                if (list.Contains(obj))
                {
                    throw new Exception("Trying to add obj that already exists");
                }

                list.Add(obj);
            }
            else
            {
                list = new List<ISimpleGameObject> {obj};
                objects[index] = list;
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

            if (objects.TryGetValue(index, out list))
            {                
                bool ret = list.Remove(obj);

                if (ret)
                {
                    --Count;
                }
                else
                {
                    logger.Warn("Tried to remove obj that wasnt in region {0} {1}", obj.ToString(), Environment.StackTrace);
                }

                if (list.Count == 0)
                {
                    objects.Remove(index); //Remove list if it is empty
                }

                return ret;
            }

            return true;
        }

        internal List<ISimpleGameObject> Get(uint x, uint y)
        {
            List<ISimpleGameObject> list;

            int index = Region.GetTileIndex(x, y);

            return objects.TryGetValue(index, out list) ? new List<ISimpleGameObject>(list) : new List<ISimpleGameObject>();
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
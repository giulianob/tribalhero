#region

using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Game.Data;
using Game.Util;

#endregion

namespace Game.Map
{
    public partial class RegionObjectList : IEnumerable<ISimpleGameObject>
    {
        private readonly IRegionLocator regionLocator;

        public RegionObjectList(IRegionLocator regionLocator)
        {
            this.regionLocator = regionLocator;
        }

        #region Members

        private readonly Dictionary<int, List<ISimpleGameObject>> objects = new Dictionary<int, List<ISimpleGameObject>>();

        private readonly ILogger logger = LoggerFactory.Current.GetLogger<RegionObjectList>();

        #endregion

        #region Properties

        public ushort Count { get; private set; }

        #endregion

        #region Methods

        public void Add(ISimpleGameObject obj, uint x, uint y)
        {
            int index = regionLocator.GetTileIndex(x, y);

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
        
        public bool Remove(ISimpleGameObject obj, uint x, uint y)
        {
            List<ISimpleGameObject> list;
            int index = regionLocator.GetTileIndex(x, y);

            if (objects.TryGetValue(index, out list))
            {                
                bool ret = list.Remove(obj);

                if (ret)
                {
                    --Count;
                }
                else
                {
                    logger.Warn("Tried to remove obj that wasnt in region {0} inWorld[{1}] tileX[{2}] tileY[{3}] Stacktrace: {4}", 
                        obj.ToString(), 
                        obj.InWorld, 
                        x,
                        y,
                        Environment.StackTrace);

                    throw new Exception("Tried to remove obj that wasnt in region");
                }

                //Remove list if it is empty to conserve memory
                if (list.Count == 0)
                {
                    objects.Remove(index);
                }
            }

            return true;
        }

        public List<ISimpleGameObject> Get(uint x, uint y)
        {
            List<ISimpleGameObject> list;

            int index = regionLocator.GetTileIndex(x, y);

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
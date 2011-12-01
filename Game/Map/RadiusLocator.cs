using System;
using Game.Data;

namespace Game.Map
{
    public class RadiusLocator
    {
        #region Delegates

        public delegate bool DoWork(uint origX, uint origY, uint x, uint y, object custom);

        #endregion

        public static void ForeachObject(uint ox, uint oy, byte radius, bool doSelf, DoWork work, object custom)
        {
            TileLocator.ForeachObject(ox,
                                      oy,
                                      radius,
                                      doSelf,
                                      (x, y, u, u1, c) =>
                                          {
                                              if (SimpleGameObject.RadiusDistance(x, y, u, u1) <= radius)
                                              {
                                                  return work(x, y, u, u1, custom);
                                              }
                                              return true;
                                          },
                                      null);
        }
    }
}
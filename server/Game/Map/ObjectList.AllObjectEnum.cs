using System.Collections.Generic;
using Game.Data;

namespace Game.Map
{
    partial class RegionObjectList
    {
        private class AllObjectEnum : IEnumerator<ISimpleGameObject>
        {
            #region Members

            private readonly RegionObjectList objectList;

            private bool isNew = true;

            private Dictionary<int, List<ISimpleGameObject>>.Enumerator itr;

            private List<ISimpleGameObject>.Enumerator listItr;

            #endregion

            #region Constructors

            public AllObjectEnum(RegionObjectList objectList)
            {
                this.objectList = objectList;
                itr = this.objectList.objects.GetEnumerator();
            }

            #endregion

            #region IEnumerator Members

            public void Reset()
            {
                itr = objectList.objects.GetEnumerator();
            }

            ISimpleGameObject IEnumerator<ISimpleGameObject>.Current
            {
                get
                {
                    return listItr.Current;
                }
            }

            public object Current
            {
                get
                {
                    return listItr.Current;
                }
            }

            public bool MoveNext()
            {
                if (isNew)
                {
                    isNew = false;
                    if (itr.MoveNext())
                    {
                        listItr = itr.Current.Value.GetEnumerator();

                        return listItr.MoveNext();
                    }

                    return false;
                }

                if (listItr.MoveNext())
                {
                    return true;
                }

                if (itr.MoveNext())
                {
                    listItr = itr.Current.Value.GetEnumerator();

                    return listItr.MoveNext();
                }

                return false;
            }

            #endregion

            public void Dispose()
            {
                listItr.Dispose();
            }
        }
    }
}
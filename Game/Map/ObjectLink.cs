#region

using Game.Data;

#endregion

namespace Game.Map
{
    class ObjectLink
    {
        private readonly GameObject value;

        public ObjectLink(GameObject gameObj)
        {
            value = gameObj;
        }

        public GameObject Value
        {
            get
            {
                return value;
            }
        }
    }
}
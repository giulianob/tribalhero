#region

using Game.Data;

#endregion

namespace Game.Map
{
    class ObjectLink
    {
        private readonly IGameObject value;

        public ObjectLink(IGameObject gameObj)
        {
            value = gameObj;
        }

        public IGameObject Value
        {
            get
            {
                return value;
            }
        }
    }
}
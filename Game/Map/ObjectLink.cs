#region

using Game.Data;

#endregion

namespace Game.Map {
    class ObjectLink {
        private GameObject value;

        public GameObject Value {
            get { return value; }
        }

        public ObjectLink(GameObject gameObj) {
            value = gameObj;
        }
    }
}
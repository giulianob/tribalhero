#region

using System.Collections.Generic;

#endregion

namespace Game.Data {
    public enum ObjectState {
        NORMAL = 0,
        BATTLE = 1
    }

    public class GameObjectState {
        private List<object> parameters = new List<object>();

        private ObjectState type;

        public ObjectState Type {
            get { return type; }
            set { type = value; }
        }

        public List<object> Parameters {
            get { return parameters; }
        }

        private GameObjectState(ObjectState type, params object[] parms) {
            this.type = type;
            parameters.AddRange(parms);
        }

        public static GameObjectState NormalState() {
            return new GameObjectState(ObjectState.NORMAL);
        }

        public static GameObjectState BattleState(uint cityid) {
            return new GameObjectState(ObjectState.BATTLE, cityid);
        }
    }
}
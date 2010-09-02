#region

using System.Collections.Generic;

#endregion

namespace Game.Data {
    public enum ObjectState {
        NORMAL = 0,
        BATTLE = 1,
        MOVING = 2
    }

    public class GameObjectState {
        private readonly List<object> parameters = new List<object>();

        public ObjectState Type { get; set; }

        public List<object> Parameters {
            get { return parameters; }
        }

        private GameObjectState(ObjectState type, params object[] parms) {
            Type = type;
            parameters.AddRange(parms);
        }

        public static GameObjectState NormalState() {
            return new GameObjectState(ObjectState.NORMAL);
        }

        public static GameObjectState BattleState(uint cityid) {
            return new GameObjectState(ObjectState.BATTLE, cityid);
        }

        public static GameObjectState Movingstate(uint x, uint y) {
            return new GameObjectState(ObjectState.MOVING, x, y);
        }
    }
}
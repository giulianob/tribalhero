using System;
using System.Collections.Generic;
using System.Text;
using Game.Battle;

namespace Game.Data
{
    public enum ObjectState {
        NORMAL = 0,
        BATTLE = 1
    }
    
    public class GameObjectState
    {
        List<object> parameters = new List<object>();
        
        ObjectState type;
        public ObjectState Type {
            get { return type; }
            set { type = value; }
        }

        public List<object> Parameters {
            get { return parameters; }
        }

        GameObjectState(ObjectState type, params object[] parms) {
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

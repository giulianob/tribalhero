#region

using System.Collections.Generic;

#endregion

namespace Game.Data
{
    public enum ObjectState
    {
        Normal = 0,

        Battle = 1,

        Moving = 2
    }

    public class GameObjectState
    {
        private readonly List<object> parameters = new List<object>();

        private GameObjectState(ObjectState type, params object[] parms)
        {
            Type = type;
            parameters.AddRange(parms);
        }

        public ObjectState Type { get; set; }

        public List<object> Parameters
        {
            get
            {
                return parameters;
            }
        }

        public static GameObjectState NormalState()
        {
            return new GameObjectState(ObjectState.Normal);
        }

        public static GameObjectState BattleState(uint battleId)
        {
            return new GameObjectState(ObjectState.Battle, battleId);
        }

        public static GameObjectState MovingState()
        {
            return new GameObjectState(ObjectState.Moving);
        }
    }
}
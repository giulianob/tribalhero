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

    public class GameObjectStateFactory
    {
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

    public class GameObjectState
    {
        protected GameObjectState()
        {            
        }

        public GameObjectState(ObjectState type, params object[] parms)
        {
            Type = type;
            Parameters = new List<object>(parms);
        }

        public virtual ObjectState Type { get; set; }

        public virtual IEnumerable<object> Parameters { get; set; }
    }
}
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Map;

namespace Game.Module.Remover
{
    public class AllButYouSelector: IPlayerSelector
    {
        private readonly IPlayer you;

        private readonly IWorld world;

        public AllButYouSelector(IPlayer you, IWorld world)
        {
            this.you = you;
            this.world = world;
        }

        public IEnumerable<uint> GetPlayerIds()
        {
            var list = world.Players.Keys.ToList();
            list.Remove(you.PlayerId);
            return list;
        }
    }
}

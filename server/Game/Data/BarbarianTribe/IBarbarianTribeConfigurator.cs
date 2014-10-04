using System.Collections.Generic;
using Game.Map;

namespace Game.Data.BarbarianTribe
{
    public interface IBarbarianTribeConfigurator
    {
        IEnumerable<BarbarianTribeConfiguration> Create(IEnumerable<IBarbarianTribe> existingBarbarianTribes, int existingBarbarianTribeCount);

        bool IsLocationAvailable(Position position);
    }
}

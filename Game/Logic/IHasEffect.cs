#region

using System.Collections.Generic;
using Game.Data;

#endregion

namespace Game.Logic
{
    public interface IHasEffect
    {
        IEnumerable<Effect> GetAllEffects(EffectInheritance inherit = EffectInheritance.All);
    }
}
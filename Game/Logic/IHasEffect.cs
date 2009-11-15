using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;

namespace Game.Logic {
    public interface IHasEffect {

        IEnumerable<Effect> GetAllEffects(EffectInheritance inherit);
        //   IEnumerable<Effect> GetEffects(EffectCode effect_code, EffectInheritance inherit);

    }
}

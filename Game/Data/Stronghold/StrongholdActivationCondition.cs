using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Data.Stronghold
{
    class StrongholdActivationCondition : IStrongholdActivationCondition
    {
        #region Implementation of IStrongholdActivationCondition

        public bool ShouldActivate(IStronghold stronghold)
        {
            return true;
        }

        #endregion
    }
}

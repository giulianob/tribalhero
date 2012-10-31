using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data.Troop;
using Game.Logic.Actions;

namespace Game.Data.Tribe
{ 
    public interface IAssignmentFactory
    {
        Assignment CreateAssignment(ITribe tribe, uint x, uint y, ILocation target, AttackMode mode, DateTime targetTime, string description, bool isAttack);
        Assignment CreateAssignmentFromDb(int id, ITribe tribe, uint x, uint y, ILocation target, AttackMode mode, DateTime targetTime, uint dispatchCount, string description, bool isAttack);
    }
}

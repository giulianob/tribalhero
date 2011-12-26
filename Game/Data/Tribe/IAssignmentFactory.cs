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
        Assignment CreateAssignment(ITribe tribe, uint x, uint y, ICity targetCity, AttackMode mode, DateTime targetTime, ITroopStub stub);

        Assignment CreateAssignmentFromDb(int id, ITribe tribe, uint x, uint y, ICity targetCity, AttackMode mode, DateTime targetTime, uint dispatchCount);
    }
}

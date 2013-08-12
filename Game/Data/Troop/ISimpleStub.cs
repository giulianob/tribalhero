using System.Collections.Generic;

namespace Game.Data.Troop
{
    public interface ISimpleStub : IEnumerable<IFormation>
    {
        byte FormationCount { get; }

        ushort TotalCount { get; }

        void AddUnit(FormationType formationType, ushort type, ushort count);

        List<Unit> ToUnitList(params FormationType[] formations);

        bool RemoveFromFormation(FormationType sourceFormationType, ISimpleStub unitsToRemove);

        IFormation this[FormationType type] { get; }

        bool HasFormation(FormationType formation);
    }
}
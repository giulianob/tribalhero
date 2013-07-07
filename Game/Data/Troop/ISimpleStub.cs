using System.Collections.Generic;

namespace Game.Data.Troop
{
    public interface ISimpleStub : IEnumerable<Formation>
    {
        byte FormationCount { get; }

        ushort TotalCount { get; }

        void AddUnit(FormationType formationType, ushort type, ushort count);

        /// <summary>
        ///     Returns a list of units for specified formations.
        ///     If formation is empty, will return all units.
        /// </summary>
        /// <param name="formations"></param>
        /// <returns></returns>
        List<Unit> ToUnitList(params FormationType[] formations);
    }
}
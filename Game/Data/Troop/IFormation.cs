using System.Collections.Generic;

namespace Game.Data.Troop
{
    public interface IFormation : IDictionary<ushort, ushort>
    {
        event Formation.UnitUpdated OnUnitUpdated;
        
        FormationType Type { get; }
        
        ushort Remove(ushort type, ushort count);

        void Add(IFormation formation);
    }
}
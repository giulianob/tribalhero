using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game.Data.Troop
{
    public class SimpleStub : ISimpleStub
    {
        protected readonly Dictionary<FormationType, IFormation> Data = new Dictionary<FormationType, IFormation>();

        #region Implementation of IEnumerable

        public virtual void AddUnit(FormationType formationType, ushort type, ushort count)
        {
            if (count <= 0)
            {
                return;
            }

            IFormation formation;
            if (!Data.TryGetValue(formationType, out formation))
            {
                formation = new Formation(formationType);
                Data.Add(formationType, formation);
            }

            formation.Add(type, count);
        }

        public virtual bool RemoveFromFormation(FormationType sourceFormationType, ISimpleStub unitsToRemove)
        {
            if (!HasFormation(sourceFormationType))
            {
                return false;
            }

            // Make sure there are enough units
            var sourceFormation = Data[sourceFormationType];
            var allUnitsToRemove = unitsToRemove.ToUnitList();
            foreach (var unit in allUnitsToRemove)
            {
                ushort count;                
                if (!sourceFormation.TryGetValue(unit.Type, out count) || count < unit.Count)
                {
                    return false;
                }
            }

            foreach (var unit in unitsToRemove.SelectMany(formation => formation))
            {
                Data[sourceFormationType].Remove(unit.Key, unit.Value);
            }

            return true;
        }

        public byte FormationCount
        {
            get
            {
                return (byte)Data.Count;
            }
        }

        public ushort TotalCount
        {
            get
            {
                return Data.Values.Aggregate<IFormation, ushort>(0,
                                                                (current, formation) =>
                                                                (ushort)(current + (ushort)formation.Sum(x => x.Value)));
            }
        }

        public bool HasFormation(FormationType formation)
        {
            return Data.ContainsKey(formation);
        }

        /// <summary>
        ///     Returns a list of units for specified formations.
        ///     If formation is empty, will return all units.
        /// </summary>
        /// <param name="formations"></param>
        /// <returns></returns>
        public List<Unit> ToUnitList(params FormationType[] formations)
        {
            var allUnits = from formation in Data.Values
                           from unit in formation
                           where (formations.Length == 0 || formations.Contains(formation.Type))
                           orderby unit.Key
                           group unit by unit.Key
                           into unitGroups select new Unit(unitGroups.Key, (ushort)unitGroups.Sum(x => x.Value));

            return allUnits.ToList();
        }

        public IEnumerator<IFormation> GetEnumerator()
        {
            return Data.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IFormation this[FormationType type]
        {
            get
            {
                return Data[type];
            }
        }

        #endregion
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game.Data.Troop
{
    public class SimpleStub : ISimpleStub
    {
        protected Dictionary<FormationType, Formation> data = new Dictionary<FormationType, Formation>();

        #region Implementation of IEnumerable

        public virtual void AddUnit(FormationType formationType, ushort type, ushort count)
        {
            if (count <= 0)
            {
                return;
            }

            Formation formation;
            if (!data.TryGetValue(formationType, out formation))
            {
                formation = new Formation(formationType);
                data.Add(formationType, formation);
            }

            formation.Add(type, count);
        }

        public byte FormationCount
        {
            get
            {
                return (byte)data.Count;
            }
        }

        public ushort TotalCount
        {
            get
            {
                return data.Values.Aggregate<Formation, ushort>(0,
                                                                (current, formation) =>
                                                                (ushort)(current + (ushort)formation.Sum(x => x.Value)));
            }
        }

        /// <summary>
        ///     Returns a list of units for specified formations.
        ///     If formation is empty, will return all units.
        /// </summary>
        /// <param name="formations"></param>
        /// <returns></returns>
        public List<Unit> ToUnitList(params FormationType[] formations)
        {
            var allUnits = from formation in data.Values
                           from unit in formation
                           where (formations.Length == 0 || formations.Contains(formation.Type))
                           orderby unit.Key
                           group unit by unit.Key
                           into unitGroups select new Unit(unitGroups.Key, (ushort)unitGroups.Sum(x => x.Value));

            return allUnits.ToList();
        }

        public IEnumerator<Formation> GetEnumerator()
        {
            return data.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
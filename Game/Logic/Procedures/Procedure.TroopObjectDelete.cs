#region

using System.Linq;
using Game.Data.Troop;
using Game.Map;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public virtual bool TroopObjectDelete(ITroopObject troop, bool addBackToNormal)
        {
            if (addBackToNormal)
            {
                AddToNormal(troop.Stub, troop.City.DefaultTroop);

                troop.City.BeginUpdate();
                troop.City.Resource.Add(troop.Stats.Loot);
                troop.City.LootStolen += (uint)troop.Stats.Loot.Total;
                troop.City.EndUpdate();
            }

            troop.City.Troops.Remove(troop.Stub.TroopId);

            troop.BeginUpdate();
            world.Regions.Remove(troop);
            troop.City.ScheduleRemove(troop, false);
            troop.Stub = null;
            troop.EndUpdate();

            return true;
        }

        private void AddToNormal(ITroopStub source, ITroopStub target)
        {
            target.BeginUpdate();
            foreach (var unit in source.SelectMany(formation => formation))
            {
                target.AddUnit(FormationType.Normal, unit.Key, unit.Value);
            }
            target.EndUpdate();
        }
    }
}
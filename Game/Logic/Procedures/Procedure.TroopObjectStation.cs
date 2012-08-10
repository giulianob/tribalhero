#region

using Game.Data;
using Game.Data.Troop;
using Game.Map;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public virtual void TroopObjectStation(ITroopObject troop, ICity target)
        {
            target.Troops.AddStationed(troop.Stub);

            troop.BeginUpdate();
            World.Current.Remove(troop);
            troop.City.ScheduleRemove(troop, false);
            troop.EndUpdate();
        }
    }
}
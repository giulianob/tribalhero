#region

using Game.Data;
using Game.Data.Troop;
using Game.Map;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public virtual void TroopObjectStation(TroopObject troop, City target)
        {
            troop.Stub.BeginUpdate();
            target.Troops.AddStationed(troop.Stub);
            troop.Stub.EndUpdate();

            troop.BeginUpdate();
            World.Current.Remove(troop);
            troop.City.ScheduleRemove(troop, false);
            troop.EndUpdate();
        }
    }
}
#region

using Game.Data;
using Game.Data.Troop;
using Game.Map;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public virtual void TroopObjectStation(ITroopObject troop, IStation station)
        {
            station.Troops.AddStationed(troop.Stub);

            troop.BeginUpdate();
            regions.Remove(troop);
            troop.City.ScheduleRemove(troop, false);
            troop.EndUpdate();
        }
    }
}
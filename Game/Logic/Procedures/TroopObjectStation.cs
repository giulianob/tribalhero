#region

using Game.Data;
using Game.Data.Troop;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public static void TroopObjectStation(TroopObject troop, City target)
        {
            troop.Stub.BeginUpdate();
            target.Troops.AddStationed(troop.Stub);
            troop.Stub.EndUpdate();

            troop.BeginUpdate();
            Global.World.Remove(troop);
            troop.City.ScheduleRemove(troop, false);
            troop.EndUpdate();
        }
    }
}
#region

using Game.Data;

#endregion

namespace Game.Logic.Procedures {
    public partial class Procedure {
        public static void TroopObjectStation(TroopObject troop, City target) {
            troop.Stub.BeginUpdate();
            target.Troops.AddStationed(troop.Stub);
            troop.Stub.EndUpdate();

            Global.World.Remove(troop);
            troop.City.Remove(troop);
        }
    }
}
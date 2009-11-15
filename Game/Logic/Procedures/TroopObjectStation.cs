using System;
using System.Collections.Generic;
using System.Text;
using Game.Database;
using Game.Util;

namespace Game.Logic.Procedures {
    public partial class Procedure {
        internal static void TroopObjectStation(Game.Data.TroopObject troop, City target) {
            troop.Stub.BeginUpdate();
            target.Troops.AddStationed(troop.Stub);
            troop.Stub.EndUpdate();

            Global.World.remove(troop);
            troop.City.remove(troop);
        }
    }
}
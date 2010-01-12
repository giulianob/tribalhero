#region

using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Fighting;

#endregion

namespace Game.Logic.Procedures {
    public partial class Procedure {
        public static void MoveUnitFormation(TroopStub stub, FormationType source, FormationType target) {
            stub[target].Add(stub[source]);
            stub[source].Clear();
        }

        public static void AddLocalToBattle(BattleManager bm, City city, ReportState state) {
            List<TroopStub> list = new List<TroopStub>(1);

            list.Add(city.DefaultTroop);

            city.DefaultTroop.BeginUpdate();
            city.DefaultTroop.State = TroopStub.TroopState.BATTLE;
            city.DefaultTroop.Template.LoadStats();
            MoveUnitFormation(city.DefaultTroop, FormationType.Normal, FormationType.InBattle);
            city.DefaultTroop.EndUpdate();

            bm.AddToLocal(list, state);
        }
    }
}
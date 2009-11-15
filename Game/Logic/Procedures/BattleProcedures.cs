using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Fighting;
using Game.Database;
using Game.Battle;
using Game.Setup;

namespace Game.Logic.Procedures {
    public partial class Procedure {

        public static void MoveUnitFormation(TroopStub stub, FormationType source, FormationType target) {
            stub.BeginUpdate();
            stub[target].Add(stub[source]);
            stub[source].Clear();
            stub.EndUpdate();
        }

        public static void AddLocalToBattle(BattleManager bm, City city, ReportState state) {
            List<TroopStub> list = new List<TroopStub>(1);

            list.Add(city.DefaultTroop);

            bm.AddToLocal(list, state);

            city.DefaultTroop.BeginUpdate();
            city.DefaultTroop.State = TroopStub.TroopState.BATTLE;
            Procedure.MoveUnitFormation(city.DefaultTroop, FormationType.Normal, FormationType.InBattle);
            city.DefaultTroop.EndUpdate();
        }

    }
}

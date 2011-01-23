#region

using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Data.Troop;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public static void MoveUnitFormation(TroopStub stub, FormationType source, FormationType target)
        {
            stub[target].Add(stub[source]);
            stub[source].Clear();
        }

        public static void AddLocalToBattle(BattleManager bm, City city, ReportState state)
        {
            if (city.DefaultTroop[FormationType.Normal].Count == 0)
                return;

            var list = new List<TroopStub>(1) {city.DefaultTroop};

            city.DefaultTroop.BeginUpdate();
            city.DefaultTroop.State = TroopState.Battle;
            city.DefaultTroop.Template.LoadStats(TroopBattleGroup.Local);
            bm.AddToLocal(list, state);
            MoveUnitFormation(city.DefaultTroop, FormationType.Normal, FormationType.InBattle);
            city.DefaultTroop.EndUpdate();
        }
    }
}
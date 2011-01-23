#region

using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Data.Troop;
using System.Linq;
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

        internal static void SenseOfUrgency(City city, uint maxHP ) {
            ushort restore = (ushort)(maxHP * city.Technologies.GetEffects(EffectCode.CountEffect, EffectInheritance.ALL).Sum(x => (int)x.value[0] == 21103 ? (int)x.value[1] : 0) / 100);
            foreach (Structure structure in city) {
                structure.BeginUpdate();
                structure.Stats.Hp += restore;
                structure.EndUpdate();
            }
        }
    }
}
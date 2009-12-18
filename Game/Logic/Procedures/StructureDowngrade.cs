using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;

namespace Game.Logic.Procedures {
    public partial class Procedure {
        public static bool StructureDowngrade(Structure structure) {
            structure.BeginUpdate();
            structure.City.Worker.Remove(structure, ActionInterrupt.CANCEL);
            byte oldLabor = structure.Stats.Labor;
            StructureFactory.getStructure(structure, structure.Type, (byte)(structure.Lvl - 1), true);
            structure.Stats.Hp = structure.Stats.Battle.MaxHp;
            structure.Stats.Labor = Math.Min(oldLabor, structure.Stats.MaxLabor);
            //structure["Efficiency"] = Formula.GetNewEfficiency((ushort)structure["Efficiency"], structure.Labor, oldLabor, structure.Stats.MaxLabor);
            InitFactory.initGameObject(InitCondition.ON_DOWNGRADE, structure, structure.Type, structure.Lvl);
            structure.EndUpdate();

            return true;
        }
    }
}

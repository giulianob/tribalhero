#region

using System;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic.Procedures {
    public partial class Procedure {
        /// <summary>
        /// Must call BeginUpdate on structure and the city
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
        public static bool StructureDowngrade(Structure structure) {
            structure.City.Worker.Remove(structure, ActionInterrupt.CANCEL);
            byte oldLabor = structure.Stats.Labor;
            StructureFactory.getStructure(structure, structure.Type, (byte) (structure.Lvl - 1), true);
            structure.Stats.Hp = structure.Stats.Base.Battle.MaxHp;
            structure.Stats.Labor = Math.Min(oldLabor, structure.Stats.Base.MaxLabor);
            InitFactory.initGameObject(InitCondition.ON_DOWNGRADE, structure, structure.Type, structure.Lvl);
            Formula.ResourceCap(structure.City);
            return true;
        }
    }
}
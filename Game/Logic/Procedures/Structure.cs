using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Setup;
using Ninject;

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        /// <summary>
        /// Changes a structure to a new type. Must call beginupdate on structure beforehand.  All technologies belong to the structure will be removed.
        /// </summary>
        /// <param name="structure">Current structure obj</param>
        /// <param name="newType">New type</param>
        /// <param name="newLvl">New lvl</param>
        public static void StructureChange(Structure structure, ushort newType, byte newLvl)
        {
            Ioc.Kernel.Get<StructureFactory>().GetUpgradedStructure(structure, newType, newLvl);
            structure.Technologies.BeginUpdate();
            structure.Technologies.Parent = structure.City.Technologies;
            structure.Technologies.Clear();  
            structure.Technologies.EndUpdate();
            structure.IsBlocked = false;
            Ioc.Kernel.Get<InitFactory>().InitGameObject(InitCondition.OnConvert, structure, structure.Type, structure.Lvl);
            
            OnStructureUpgradeDowngrade(structure);
        }
    }
}

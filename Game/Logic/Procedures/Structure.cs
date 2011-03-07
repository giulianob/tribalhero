using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Setup;

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        /// <summary>
        /// Changes a structure to a new type. Must call beginupdate on structure beforehand
        /// </summary>
        /// <param name="structure">Current structure obj</param>
        /// <param name="newType">New type</param>
        /// <param name="newLvl">New lvl</param>
        public static void StructureChange(Structure structure, ushort newType, byte newLvl)
        {
            StructureFactory.GetUpgradedStructure(structure, newType, newLvl);
            structure.Technologies.Parent = structure.City.Technologies;
            InitFactory.InitGameObject(InitCondition.OnConvert, structure, structure.Type, structure.Lvl);
            structure.IsBlocked = false;
        }
    }
}

using Game.Data;
using Game.Setup;
using Ninject;

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        /// <summary>
        ///     Changes a structure to a new type.
        /// </summary>
        public virtual void StructureChange(IStructure structure, ushort newType, byte newLvl, CallbackProcedure callbackProcedure, IStructureCsvFactory structureCsvFactory)
        {
            structure.City.BeginUpdate();
            structure.BeginUpdate();
            structure.IsBlocked = 0;
            structureCsvFactory.GetUpgradedStructure(structure, newType, newLvl);
            structure.Technologies.BeginUpdate();
            structure.Technologies.Parent = structure.City.Technologies;
            structure.Technologies.Clear();

            OnStructureUpgradeDowngrade(structure);

            structure.Technologies.EndUpdate();
            structure.EndUpdate();
            structure.City.EndUpdate();

            callbackProcedure.OnStructureConvert(structure);
        }
    }
}
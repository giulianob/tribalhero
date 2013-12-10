#region

using System;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Logic.Actions;

#endregion

namespace Game.Logic.Formulas
{
    public partial class Formula
    {
        public virtual byte GetTroopRadius(ITroopStub stub, ITechnologyManager em)
        {
            // The radius is based on the sum of levels of the structures in the city, from 0 to 4
            return (byte)Math.Min(stub.City.Value / 40, 4);
        }

        public virtual decimal GetTroopSpeed(ITroopStub stub)
        {
            if (stub.TotalCount == 0)
            {
                return 0;
            }

            int count = 0;
            decimal totalSpeed = 0;
            int machineSpeed = int.MaxValue;

            foreach (var formation in stub)
            {
                foreach (var kvp in formation)
                {
                    IBaseUnitStats stats = stub.City.Template[kvp.Key];
                    // Use the slowest machine speed if available.
                    if (stats.Battle.Armor == ArmorType.Machine)
                    {
                        machineSpeed = Math.Min(stats.Battle.Speed, machineSpeed);
                    }
                    else
                    {
                        count += (kvp.Value * stats.Upkeep);
                        totalSpeed += (kvp.Value * stats.Upkeep * stats.Battle.Speed);
                    }
                }
            }

            return Math.Round(machineSpeed == int.MaxValue ? totalSpeed / count : machineSpeed, 1);
        }

        public virtual int GetAttackModeTolerance(int totalCount, AttackMode mode)
        {
            switch(mode)
            {
                case AttackMode.Weak:
                    return (ushort)(totalCount * 2 / 3);
                case AttackMode.Normal:
                    return (ushort)(totalCount / 3);
                default:
                    return 0;
            }
        }
    }
}
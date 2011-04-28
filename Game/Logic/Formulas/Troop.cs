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
        public static byte GetTroopRadius(TroopStub stub, TechnologyManager em)
        {
            return (byte)Math.Min((int)Math.Ceiling((decimal)stub.Upkeep/100), 5);
        }

        public static byte GetTroopSpeed(TroopStub stub)
        {
            int count = 0;
            int totalSpeed = 0;
            int machineSpeed = int.MaxValue;

            foreach (var formation in stub)
            {
                foreach (var kvp in formation)
                {
                    BaseUnitStats stats = stub.City.Template[kvp.Key];
                    // Use the slowest machine speed if available.
                    if (stats.Battle.Armor == ArmorType.Machine) 
                    {
                        machineSpeed = Math.Min(stats.Battle.Spd,machineSpeed);
                    }
                    else
                    {
                        count += (kvp.Value*stats.Upkeep);
                        totalSpeed += (kvp.Value*stats.Upkeep*stats.Battle.Spd);
                    }
                }
            }

            return (byte)(machineSpeed == int.MaxValue ? totalSpeed / count : machineSpeed);
        }

        public static int GetAttackModeTolerance(int totalCount, AttackMode mode)
        {
            switch(mode)
            {
                case AttackMode.Weak:
                    return (ushort)(totalCount*2/3);
                case AttackMode.Normal:
                    return (ushort)(totalCount/3);
                case AttackMode.Strong:
                    return 0;
            }
            return 0;
        }
    }
}
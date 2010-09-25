#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Fighting;
using Game.Logic.Actions;

#endregion

namespace Game.Logic
{
    public partial class Formula
    {

        public static byte GetTroopRadius(TroopStub stub, TechnologyManager em)
        {
            return (byte)Math.Min((int)Math.Ceiling((decimal)stub.Upkeep / 100), 5);
        }

        public static byte GetTroopSpeed(TroopStub stub)
        {
            int count = 0;
            int totalSpeed = 0;

            foreach (Formation formation in stub)
            {
                foreach (KeyValuePair<ushort, ushort> kvp in formation)
                {
                    BaseUnitStats stats = stub.City.Template[kvp.Key];
                    count += (kvp.Value * stats.Upkeep);
                    totalSpeed += (kvp.Value * stats.Upkeep * stats.Battle.Spd);
                }
            }

            return (byte)(totalSpeed / count);
        }

        public static int GetAttackModeTolerance(int totalCount, AttackMode mode)
        {
            switch (mode)
            {
                case AttackMode.WEAK:
                    return (ushort)(totalCount * 2 / 3);
                case AttackMode.NORMAL:
                    return (ushort)(totalCount / 3);
                case AttackMode.STRONG:
                    return 0;
            }
            return 0;
        }

        public static ushort GetRewardPoint(Resource resource, ushort hp)
        {
            if (hp == 0) return 1;

            ushort total = (ushort)(resource.Crop + resource.Gold + resource.Wood + resource.Iron * 2);            
            return (ushort)Math.Max(total / hp, 1);
        }
    }
}
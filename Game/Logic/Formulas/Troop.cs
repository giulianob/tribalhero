#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Fighting;
using Game.Logic.Actions;
using Game.Module;

#endregion

namespace Game.Logic {
    public partial class Formula {

        public static byte GetTroopRadius(TroopStub stub, TechnologyManager em)
        {
            int count = 0;
            foreach (Formation formation in stub)
            {
                if (formation.Type == FormationType.SCOUT)
                    continue;
                foreach (KeyValuePair<ushort, ushort> kvp in formation)
                    count += kvp.Value;
            }

            return (byte)Math.Min((int)Math.Ceiling((decimal)count / 100), 5);
        }

        public static byte GetTroopSpeed(TroopStub stub, object p)
        {
            int count = 0;
            foreach (Formation formation in stub)
            {
                if (formation.Type == FormationType.SCOUT)
                    continue;

                foreach (KeyValuePair<ushort, ushort> kvp in formation)
                    count += kvp.Value;
            }
            return (byte)Math.Min(15, (10 - Math.Max(count / 100, 5)));
            //limiting it to max of 15 because Formula.MoveTime will return negative if greater than 20
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
            ushort total = (ushort)(resource.Crop + resource.Gold + resource.Wood + resource.Iron * 2);
            return (ushort)Math.Max(total / hp, 1);
        }
    }
}
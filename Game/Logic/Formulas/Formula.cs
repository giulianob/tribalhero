#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Fighting;
using Game.Logic.Actions;
using Game.Setup;

#endregion

namespace Game.Logic {
    public partial class Formula {
        public static int ResourceRate(TechnologyManager em) {
            // return em.Sum(EffectCode.ResourceRate);
            return 0;
        }

        public static bool HarvestSurprise(TechnologyManager em) {
            int min = em.Min(EffectCode.HarvestSurprise, EffectInheritance.SelfAll, 0);
            if (min == int.MaxValue)
                return false;
            return Config.Random.Next(0, 100) < em.Min(EffectCode.HarvestSurprise, EffectInheritance.SelfAll, 0);
        }

        public static bool OverDigging(TechnologyManager em) {
            int max = em.Max(EffectCode.OverDigging, EffectInheritance.SelfAll, 0);
            if (max == int.MinValue)
                return false;
            return Config.Random.Next(0, 1000) < em.Min(EffectCode.OverDigging, EffectInheritance.SelfAll, 0);
        }

        internal static byte GetTroopRadius(TroopStub stub, TechnologyManager em) {
            int count = 0;
            foreach (Formation formation in stub) {
                if (formation.Type == FormationType.SCOUT)
                    continue;
                foreach (KeyValuePair<ushort, ushort> kvp in formation)
                    count += kvp.Value;
            }

            return (byte) Math.Min((int) Math.Ceiling((decimal) count/100), 5);
        }

        internal static byte GetTroopSpeed(TroopStub stub, object p) {
            int count = 0;
            foreach (Formation formation in stub) {
                if (formation.Type == FormationType.SCOUT)
                    continue;
                foreach (KeyValuePair<ushort, ushort> kvp in formation)
                    count += kvp.Value;
            }
            return (byte) Math.Min(15, (10 - Math.Max(count/100, 5)));
                //limiting it to max of 15 because Formula.MoveTime will return negative if greater than 20
        }

        internal static Resource GetMillResource(byte millLvl, TechnologyManager em) {
            Resource res = new Resource(millLvl*5, millLvl*4, millLvl*2, millLvl*3, 0);
            foreach (Effect effect in em.GetEffects(EffectCode.CountEffect, EffectInheritance.Self)) {
                switch ((int) effect.value[0]) {
                    case 21051:
                        res.Crop *= (int) effect.value[1];
                        break;
                    case 21052:
                        res.Gold *= (int) effect.value[1];
                        break;
                    case 21053:
                        res.Wood *= (int) effect.value[1];
                        break;
                    case 21054:
                        res.Iron *= (int) effect.value[1];
                        break;
                }
            }
            return res;
        }

        internal static byte GetRadius(uint totalLabor) {
            return (byte) Math.Min(6, (int) (totalLabor/100));
        }

        internal static int GetAttackModeTolerance(int totalCount, AttackMode mode) {
            switch (mode) {
                case AttackMode.WEAK:
                    return (ushort) (totalCount*2/3);
                case AttackMode.NORMAL:
                    return (ushort) (totalCount/3);
                case AttackMode.STRONG:
                    return 0;
            }
            return 0;
        }

        internal static int GetResource(byte lvl, ushort labor) {
            if (labor == 0)
                return 0;
            //return lvl*labor*(100+labor/lvl)/100;
            return labor;
        }

        internal static ushort GetRewardPoint(Resource resource, ushort hp) {
            ushort total = (ushort) (resource.Crop + resource.Gold + resource.Wood + resource.Iron*2);
            return (ushort) Math.Max(total/hp, 1);
        }

        internal static double TradeTime(Structure structure) {
            return 50 + 50/structure.Lvl;
        }

        internal static double MarketTax(Structure structure) {
            switch (structure.Lvl) {
                case 1:
                    return 0.30;
                case 2:
                    return 0.20;
                case 3:
                    return 0.10;
                default:
                    return 0.05;
            }
        }

        internal static void ResourceCap(City city) {
            if (Config.resource_cap) {
                int limit = 500 + city.MainBuilding.Lvl*city.MainBuilding.Lvl*100;
                city.Resource.SetLimits(limit, 0, limit, limit, 0);
            } else
                city.Resource.SetLimits(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
        }

        internal static byte GetCityActionMax(City owner) {
            return (byte) (owner.MainBuilding.Lvl >= 10 ? 2 : 1);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Fighting;
using Game.Setup;
namespace Game.Logic {
    public partial class Formula {

        public static int ResourceRate(TechnologyManager em) {
            // return em.Sum(EffectCode.ResourceRate);
            return 0;
        }
        public static bool HarvestSurprise(TechnologyManager em) {
            int min = em.Min(EffectCode.HarvestSurprise, EffectInheritance.SelfAll, 0);
            if (min == int.MaxValue) return false;
            return Config.Random.Next(0, 100) < em.Min(EffectCode.HarvestSurprise, EffectInheritance.SelfAll, 0);
        }
        public static bool OverDigging(TechnologyManager em) {
            int max = em.Max(EffectCode.OverDigging, EffectInheritance.SelfAll, 0);
            if (max == int.MinValue) return false;
            return Config.Random.Next(0, 1000) < em.Min(EffectCode.OverDigging, EffectInheritance.SelfAll, 0);
        }

        internal static byte GetTroopRadius(TroopStub stub, TechnologyManager em) {
            int count = 0;
            foreach (KeyValuePair<FormationType, Formation> formation in stub as IEnumerable<KeyValuePair<FormationType, Formation>>) {
                if (formation.Key == FormationType.Scout) continue;
                foreach (KeyValuePair<ushort, ushort> kvp in formation.Value) {
                    count += kvp.Value;
                }
            }

            return (byte)Math.Min((int)Math.Ceiling((decimal)count / 100), 5);
        }

        internal static byte GetTroopSpeed(TroopStub stub, object p) {
            int count = 0;
            foreach (KeyValuePair<FormationType, Formation> formation in stub as IEnumerable<KeyValuePair<FormationType, Formation>>) {
                if (formation.Key == FormationType.Scout) continue;
                foreach (KeyValuePair<ushort, ushort> kvp in formation.Value) {
                    count += kvp.Value;
                }
            }
            return (byte)Math.Min(15, (10 - Math.Max(count / 100, 5))); //limiting it to max of 15 because Formula.MoveTime will return negative if greater than 20
        }


        internal static Resource GetMillResource(byte mill_lvl, TechnologyManager em) {
            Resource res = new Resource(mill_lvl * 5, mill_lvl * 4, mill_lvl * 2, mill_lvl * 3, 0);
            foreach (Effect effect in em.GetEffects(EffectCode.CountEffect, EffectInheritance.Self)) {
                switch ((int)effect.value[0]) {
                    case 21051:
                        res.Crop *= (int)effect.value[1];
                        break;
                    case 21052:
                        res.Gold *= (int)effect.value[1];
                        break;
                    case 21053:
                        res.Wood *= (int)effect.value[1];
                        break;
                    case 21054:
                        res.Iron *= (int)effect.value[1];
                        break;
                }
            }
            return res;
        }

        internal static ushort GetIncreasedEfficiency(ushort cur_efficiency, byte cur, byte max) {
            ushort max_efficency = (ushort)(cur * 120);
            if (cur_efficiency >= max_efficency) return cur_efficiency;
            switch ((10 - cur_efficiency * 10 / max_efficency) / 3) {
                case 0:
                    return (ushort)(cur_efficiency + 1);
                case 1:
                    return (ushort)(cur_efficiency + cur);
                case 2:
                    return (ushort)(cur_efficiency + cur * 3 / 2);
                case 3:
                    return (ushort)(cur_efficiency + cur * 2);
            }
            throw new Exception("Should never reach here");
        }

        internal static byte GetRadius(uint total_labor) {
            return (byte)Math.Min(6, (int)(total_labor / 100));
        }

        internal static int GetAttackModeTolerance(int totalCount, Game.Logic.Actions.AttackMode mode) {
            switch (mode) {
                case Game.Logic.Actions.AttackMode.WEAK:
                    return (ushort)(totalCount * 2 / 3);
                case Game.Logic.Actions.AttackMode.NORMAL:
                    return (ushort)(totalCount / 3);
                case Game.Logic.Actions.AttackMode.STRONG:
                    return 0;
            }
            return 0;
        }

        internal static int GetResource(byte lvl, ushort labor) {
            if (labor == 0) return 0;
            //return lvl*labor*(100+labor/lvl)/100;
            return labor;
        }

        internal static ushort GetNewEfficiency(ushort cur_efficiency, int old_count, int new_count, byte max_count) {
            if (old_count < new_count) {
                return cur_efficiency;
            }
            else if (old_count == 0 || new_count == 0) {
                return 0;
            }
            else {
                return (byte)(cur_efficiency / old_count * new_count);
            }
        }

        internal static ushort GetRewardPoint(Resource resource, ushort hp) {
            ushort total = (ushort)(resource.Crop + resource.Gold + resource.Wood + resource.Iron * 2);
            return (ushort)Math.Max(total / hp, 1);
        }

        internal static double TradeTime(Structure structure) {
            return 50 + 50 / structure.Lvl;
        }

        internal static double MarketTax(Structure structure) {
            if (structure.Lvl == 1)
                return 0.30;
            else if (structure.Lvl == 2)
                return 0.20;
            else if (structure.Lvl == 3)
                return 0.10;
            else
                return 0.05;
        }

        internal static void ResourceCap(City city) {
            if (Config.resource_cap) {
                if (city.Owner.PlayerId != 123) {
                    int limit = 500 + city.MainBuilding.Lvl * city.MainBuilding.Lvl * 100;
                    city.Resource.SetLimits(limit, 0, limit, limit, 0);
                }
            }
            else {
                city.Resource.SetLimits(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
            }
        }

        internal static byte GetCityActionMax(City owner) {
            return (byte)(owner.MainBuilding.Lvl >= 10 ? 2 : 1);
        }
    }
}

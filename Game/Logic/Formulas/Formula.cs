#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Fighting;
using Game.Logic.Actions;
using Game.Setup;

#endregion

namespace Game.Logic {
    public partial class Formula {

        /// <summary>
        /// Applies the specified effects to the specified radius. This is used by AwayFromLayout for building validation.
        /// </summary>
        /// <param name="effects"></param>
        /// <param name="radius"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetAwayFromRadius(IEnumerable<Effect> effects, byte radius, ushort type)
        {
            return radius + effects.Sum(x => (x.id == EffectCode.AwayFromStructureMod && (int)x.value[0] == type) ? (int)x.value[1] : 0);
        }        
        
        /// <summary>
        /// Returns the crop cap based on the main building level
        /// </summary>
        /// <param name="lvl"></param>
        /// <returns></returns>
        public static int ResourceCropCap(byte lvl) {
            int[] cap = { 700, 700, 1100, 1600, 2500, 3800, 5700, 8700, 13200, 20100, 30000 };
            return cap[lvl];
        }
        
        /// <summary>
        /// /// Returns the wood cap based on the main building level
        /// </summary>
        /// <param name="lvl"></param>
        /// <returns></returns>
        public static int ResourceWoodCap(byte lvl) {
            int[] cap = { 700, 700, 1100, 1600, 2500, 3800, 5700, 8700, 13200, 20100, 30000 };
            return cap[lvl];
        }

        /// <summary>
        /// Returns the iron cap based on the main building level
        /// </summary>
        /// <param name="lvl"></param>
        /// <returns></returns>
        public static int ResourceIronCap(byte lvl) {
            int[] cap = { 200, 200, 300, 500, 800, 1200, 1800, 2800, 4400, 6800, 10000 };
            return cap[lvl];
        }

        public static int GetIronRate(Structure structure) {
            int[] multiplier = { int.MaxValue, 70, 66, 63, 60, 56, 53, 5, 46, 43, 40 };
            return structure.Stats.Labor * 10 / multiplier[structure.Lvl];
        }
        public static int GetXForOneCount(TechnologyManager tech) {
            int ret = tech.GetEffects(EffectCode.XFor1, EffectInheritance.INVISIBLE).DefaultIfEmpty(new Effect()).Min(x => x.value[0]==null?ushort.MaxValue:(int)x.value[0]);
            return ret;
        }
    }
}
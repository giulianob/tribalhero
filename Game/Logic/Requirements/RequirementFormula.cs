#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using System.Linq;
#endregion

namespace Game.Logic {
    public class RequirementFormula {

        public static Error HaveAttackRate(GameObject obj, IEnumerable<Effect> effects, String[] parms, uint id) {
            if (obj.City.AttackPoint > int.Parse(parms[0])) {
                return Error.OK;
            }
            return Error.EFFECT_REQUIREMENT_NOT_MET;
        }

        public static Error HaveUnit(GameObject obj, IEnumerable<Effect> effects, String[] parms, uint id) {
            
        }
        public static Error AwayFromStructure(GameObject obj, IEnumerable<Effect> effects, String[] parms, uint id) {
            int distance = int.Parse(parms[1]) + effects.Sum<Effect>(x=> (x.id == EffectCode.AwayFromStructureMod && (int)x.value[0] == id) ? (int)x.value[1]:0);
            int type = int.Parse(parms[0]);
            
            if (obj.City.Any(x => x.Type == type && x.RadiusDistance(obj) < distance)) {
                return Error.LAYOUT_NOT_FULLFILLED;
            }
            return Error.OK;
        }

        public static Error CanBuild(GameObject obj, IEnumerable<Effect> effects, String[] parms, uint id) {
            foreach (Effect effect in effects) {
                if ((int) effect.value[0] == int.Parse(parms[0]))
                    return Error.OK;
            }
            return Error.EFFECT_REQUIREMENT_NOT_MET;
        }

        public static Error UniqueTechnology(GameObject obj, IEnumerable<Effect> effects, string[] parms, uint id) {
            if( obj.City.Any(s=>s!=obj && s.Technologies.Any<Technology>(t=>t.Type==uint.Parse(parms[0])))) {
                return Error.EFFECT_REQUIREMENT_NOT_MET;
            }
            return Error.OK;
        }

        public static Error HaveTechnology(GameObject obj, IEnumerable<Effect> effects, string[] parms, uint id) {
            int count = 0;
            foreach (Effect effect in effects) {
                if (effect.id != EffectCode.HaveTechnology)
                    continue;

                if ((int) effect.value[0] == int.Parse(parms[0]) && (int) effect.value[1] >= int.Parse(parms[1]))
                    ++count;
            }

            return count >= int.Parse(parms[2]) ? Error.OK : Error.EFFECT_REQUIREMENT_NOT_MET;
        }

        public static Error HaveStructure(GameObject obj, IEnumerable<Effect> effects, string[] parms, uint id) {            
            ushort type = ushort.Parse(parms[0]);
            byte min = byte.Parse(parms[1]);
            byte max = byte.Parse(parms[2]);

            foreach (Structure structure in obj.City) {
                if (structure.Type == type && structure.Lvl >= min && structure.Lvl <= max)
                    return Error.OK;
            }
            return Error.EFFECT_REQUIREMENT_NOT_MET;
        }

        public static Error HaveNoStructure(GameObject obj, IEnumerable<Effect> effects, string[] parms, uint id) {
            ushort type = ushort.Parse(parms[0]);
            byte min = byte.Parse(parms[1]);
            byte max = byte.Parse(parms[2]);

            foreach (Structure structure in obj.City) {
                if (structure.Type == type && structure.Lvl >= min && structure.Lvl <= max)
                    return Error.EFFECT_REQUIREMENT_NOT_MET;
            }

            return Error.OK;
        }

        public static Error CountLessThan(GameObject obj, IEnumerable<Effect> effects, string[] parms, uint id) {
            int effectCode = int.Parse(parms[0]);
            int maxCount = int.Parse(parms[1]);

            int count = 0;
            foreach (Effect effect in effects) {
                if (effect.id == EffectCode.CountEffect && (int) effect.value[0] == effectCode)
                    count += (int) effect.value[1];
            }

            return count < maxCount ? Error.OK : Error.EFFECT_REQUIREMENT_NOT_MET;
        }
    }
}
#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic {
    public class RequirementFormula {
        public static Error CanBuild(GameObject obj, IEnumerable<Effect> effects, String[] parms) {
            foreach (Effect effect in effects) {
                if ((int) effect.value[0] == int.Parse(parms[0]))
                    return Error.OK;
            }
            return Error.EFFECT_REQUIREMENT_NOT_MET;
        }

        public static Error HaveTechnology(GameObject obj, IEnumerable<Effect> effects, string[] parms) {
            int count = 0;
            foreach (Effect effect in effects) {
                if ((int) effect.value[0] == int.Parse(parms[0]) && (int) effect.value[1] >= int.Parse(parms[1]))
                    ++count;
            }
            if (count >= int.Parse(parms[2]))
                return Error.OK;
            return Error.EFFECT_REQUIREMENT_NOT_MET;
        }

        public static Error HaveStructure(GameObject obj, IEnumerable<Effect> effects, string[] parms) {
            Structure structure;
            ushort type = ushort.Parse(parms[0]);
            byte min = byte.Parse(parms[1]);
            byte max = byte.Parse(parms[2]);

            foreach (GameObject tmp in obj.City) {
                if ((structure = tmp as Structure) != null) {
                    if (structure.Type == type && structure.Lvl >= min && structure.Lvl <= max)
                        return Error.OK;
                }
            }
            return Error.EFFECT_REQUIREMENT_NOT_MET;
        }

        public static Error HaveNoStructure(GameObject obj, IEnumerable<Effect> effects, string[] parms) {
            Structure structure;
            ushort type = ushort.Parse(parms[0]);
            byte min = byte.Parse(parms[1]);
            byte max = byte.Parse(parms[2]);

            foreach (GameObject tmp in obj.City) {
                if ((structure = tmp as Structure) != null) {
                    if (structure.Type == type && structure.Lvl >= min && structure.Lvl <= max)
                        return Error.EFFECT_REQUIREMENT_NOT_MET;
                }
            }
            return Error.OK;
        }

        public static Error CountLessThan(GameObject obj, IEnumerable<Effect> effects, string[] parms) {
            int count = 0;
            foreach (Effect effect in effects) {
                if (effect.id == EffectCode.CountEffect && (int) effect.value[0] == int.Parse(parms[0]))
                    count += (int) effect.value[1];
            }
            if (count < int.Parse(parms[1]))
                return Error.OK;
            return Error.EFFECT_REQUIREMENT_NOT_MET;
        }
    }
}
#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Setup;

#endregion

namespace Game.Logic
{
    public class RequirementFormula
    {
        public static Error DefensePoint(GameObject obj, IEnumerable<Effect> effects, String[] parms, uint id)
        {
            switch(parms[0])
            {
                case "lt":
                    if (obj.City.DefensePoint < int.Parse(parms[1]))
                        return Error.Ok;
                    break;
                case "gt":
                    if (obj.City.DefensePoint > int.Parse(parms[1]))
                        return Error.Ok;
                    break;
                default:
                    throw new Exception("Bad requirement parameter!");
            }
            return Error.EffectRequirementNotMet;
        }

        public static Error AttackPoint(GameObject obj, IEnumerable<Effect> effects, String[] parms, uint id)
        {
            switch(parms[0])
            {
                case "lt":
                    if (obj.City.AttackPoint < int.Parse(parms[1]))
                        return Error.Ok;
                    break;
                case "gt":
                    if (obj.City.AttackPoint > int.Parse(parms[1]))
                        return Error.Ok;
                    break;
                default:
                    throw new Exception("Bad requirement parameter!");
            }
            return Error.EffectRequirementNotMet;
        }

        public static Error HaveUnit(GameObject obj, IEnumerable<Effect> effects, String[] parms, uint id)
        {
            ushort type = ushort.Parse(parms[0]);
            int sum = obj.City.Troops.MyStubs().Sum(stub => stub.Sum<Formation>(formation => formation.ContainsKey(type) ? formation[type] : 0));
            switch(parms[1])
            {
                case "lt":
                    if (sum < int.Parse(parms[2]))
                        return Error.Ok;
                    break;
                case "gt":
                    if (sum > int.Parse(parms[2]))
                        return Error.Ok;
                    break;
                default:
                    throw new Exception("Bad requirement parameter!");
            }
            return Error.EffectRequirementNotMet;
        }

        public static Error CanBuild(GameObject obj, IEnumerable<Effect> effects, String[] parms, uint id)
        {
            if (effects.Any(effect => (int)effect.Value[0] == int.Parse(parms[0])))
                return Error.Ok;
            return Error.EffectRequirementNotMet;
        }

        public static Error UniqueTechnology(GameObject obj, IEnumerable<Effect> effects, string[] parms, uint id)
        {
            if (obj.City.Any(s => s != obj && s.Technologies.Any<Technology>(t => t.Level > 0 && t.Type == uint.Parse(parms[0]))))
                return Error.EffectRequirementNotMet;
            return Error.Ok;
        }

        public static Error HaveTechnology(GameObject obj, IEnumerable<Effect> effects, string[] parms, uint id)
        {
            int count = 0;
            foreach (var effect in effects)
            {
                if (effect.Id != EffectCode.HaveTechnology)
                    continue;

                if ((int)effect.Value[0] == int.Parse(parms[0]) && (int)effect.Value[1] >= int.Parse(parms[1]))
                    ++count;
            }

            return count >= int.Parse(parms[2]) ? Error.Ok : Error.EffectRequirementNotMet;
        }

        public static Error HaveStructure(GameObject obj, IEnumerable<Effect> effects, string[] parms, uint id)
        {
            ushort type = ushort.Parse(parms[0]);
            byte min = byte.Parse(parms[1]);
            byte max = byte.Parse(parms[2]);

            foreach (var structure in obj.City)
            {
                if (structure.Type == type && structure.Lvl >= min && structure.Lvl <= max)
                    return Error.Ok;
            }
            return Error.EffectRequirementNotMet;
        }

        public static Error HaveNoStructure(GameObject obj, IEnumerable<Effect> effects, string[] parms, uint id)
        {
            ushort type = ushort.Parse(parms[0]);
            byte min = byte.Parse(parms[1]);
            byte max = byte.Parse(parms[2]);

            foreach (var structure in obj.City)
            {
                if (structure.Type == type && structure.Lvl >= min && structure.Lvl <= max)
                    return Error.EffectRequirementNotMet;
            }

            return Error.Ok;
        }

        public static Error CountLessThan(GameObject obj, IEnumerable<Effect> effects, string[] parms, uint id)
        {
            int effectCode = int.Parse(parms[0]);
            int maxCount = int.Parse(parms[1]);

            int count = 0;
            foreach (var effect in effects)
            {
                if (effect.Id == EffectCode.CountEffect && (int)effect.Value[0] == effectCode)
                    count += (int)effect.Value[1];
            }

            return count < maxCount ? Error.Ok : Error.EffectRequirementNotMet;
        }
    }
}
#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Logic.Requirements;
using Game.Setup;

#endregion

namespace Game.Logic
{
    public class RequirementFormula
    {
        private readonly IObjectTypeFactory objectTypeFactory;

        public RequirementFormula(IObjectTypeFactory objectTypeFactory)
        {
            this.objectTypeFactory = objectTypeFactory;
        }

        public Error DefensePoint(IGameObject obj, IEnumerable<Effect> effects, String[] parms, uint id)
        {
            switch(parms[0])
            {
                case "lt":
                    if (obj.City.DefensePoint < int.Parse(parms[1]))
                    {
                        return Error.Ok;
                    }
                    break;
                case "gt":
                    if (obj.City.DefensePoint > int.Parse(parms[1]))
                    {
                        return Error.Ok;
                    }
                    break;
                default:
                    throw new Exception("Bad requirement parameter!");
            }
            return Error.EffectRequirementNotMet;
        }

        public Error AttackPoint(IGameObject obj, IEnumerable<Effect> effects, String[] parms, uint id)
        {
            switch(parms[0])
            {
                case "lt":
                    if (obj.City.AttackPoint < int.Parse(parms[1]))
                    {
                        return Error.Ok;
                    }
                    break;
                case "gt":
                    if (obj.City.AttackPoint > int.Parse(parms[1]))
                    {
                        return Error.Ok;
                    }
                    break;
                default:
                    throw new Exception("Bad requirement parameter!");
            }
            return Error.EffectRequirementNotMet;
        }

        public Error PlayerDefensePoint(IGameObject obj, IEnumerable<Effect> effects, String[] parms, uint id)
        {
            switch(parms[0])
            {
                case "lt":
                    if (obj.City.Owner.DefensePoint < int.Parse(parms[1]))
                    {
                        return Error.Ok;
                    }
                    break;
                case "gt":
                    if (obj.City.Owner.DefensePoint > int.Parse(parms[1]))
                    {
                        return Error.Ok;
                    }
                    break;
                default:
                    throw new Exception("Bad requirement parameter!");
            }
            return Error.EffectRequirementNotMet;
        }

        public Error PlayerAttackPoint(IGameObject obj, IEnumerable<Effect> effects, String[] parms, uint id)
        {
            switch(parms[0])
            {
                case "lt":
                    if (obj.City.Owner.AttackPoint < int.Parse(parms[1]))
                    {
                        return Error.Ok;
                    }
                    break;
                case "gt":
                    if (obj.City.Owner.AttackPoint > int.Parse(parms[1]))
                    {
                        return Error.Ok;
                    }
                    break;
                default:
                    throw new Exception("Bad requirement parameter!");
            }
            return Error.EffectRequirementNotMet;
        }

        public Error HaveUnit(IGameObject obj, IEnumerable<Effect> effects, String[] parms, uint id)
        {
            ushort type = ushort.Parse(parms[0]);
            int sum =
                    obj.City.Troops.MyStubs()
                       .Sum(stub => stub.Sum(formation => formation.ContainsKey(type) ? formation[type] : 0));
            switch(parms[1])
            {
                case "lt":
                    if (sum < int.Parse(parms[2]))
                    {
                        return Error.Ok;
                    }
                    break;
                case "gt":
                    if (sum > int.Parse(parms[2]))
                    {
                        return Error.Ok;
                    }
                    break;
                default:
                    throw new Exception("Bad requirement parameter!");
            }
            return Error.EffectRequirementNotMet;
        }

        public Error CanBuild(IGameObject obj, IEnumerable<Effect> effects, String[] parms, uint id)
        {
            if (effects.Any(effect => (int)effect.Value[0] == int.Parse(parms[0])))
            {
                return Error.Ok;
            }
            return Error.EffectRequirementNotMet;
        }

        public Error HaveTechnology(IGameObject obj, IEnumerable<Effect> effects, string[] parms, uint id)
        {
            int count =
                    effects.Count(
                                  effect =>
                                  effect.Id == EffectCode.HaveTechnology && (int)effect.Value[0] == int.Parse(parms[0]) &&
                                  (int)effect.Value[1] >= int.Parse(parms[1]));

            return count >= int.Parse(parms[2]) ? Error.Ok : Error.EffectRequirementNotMet;
        }

        public Error HaveStructure(IGameObject obj, IEnumerable<Effect> effects, string[] parms, uint id)
        {
            ushort type = ushort.Parse(parms[0]);
            byte min = byte.Parse(parms[1]);
            byte max = byte.Parse(parms[2]);

            return obj.City.Any(structure => structure.Type == type && structure.Lvl >= min && structure.Lvl <= max)
                           ? Error.Ok
                           : Error.EffectRequirementNotMet;
        }

        public Error HaveNoStructure(IGameObject obj, IEnumerable<Effect> effects, string[] parms, uint id)
        {
            ushort type = ushort.Parse(parms[0]);
            byte min = byte.Parse(parms[1]);
            byte max = byte.Parse(parms[2]);
            byte count = byte.Parse(parms[3]);

            var totalStructures =
                    obj.City.Count(structure => structure.Type == type && structure.Lvl >= min && structure.Lvl <= max);

            return totalStructures < count ? Error.Ok : Error.EffectRequirementNotMet;
        }

        public Error CountLessThan(IGameObject obj, IEnumerable<Effect> effects, string[] parms, uint id)
        {
            int effectCode = int.Parse(parms[0]);
            int maxCount = int.Parse(parms[1]);

            int count =
                    effects.Sum(
                                effect =>
                                effect.Id == EffectCode.CountEffect && (int)effect.Value[0] == effectCode
                                        ? (int)effect.Value[1]
                                        : 0);

            return count < maxCount ? Error.Ok : Error.EffectRequirementNotMet;
        }

        public Error DistributedPointSystem(IGameObject obj, IEnumerable<Effect> effects, string[] parms, uint id)
        {
            DistributedPointSystemRequirement requirement = new DistributedPointSystemRequirement();
            return requirement.Validate(obj as Structure, uint.Parse(parms[0]));
        }

        public Error LessThanStructureCount(IGameObject obj, IEnumerable<Effect> effects, string[] parms, uint id)
        {
            string type = parms[0]; // tmp basement
            string type2 = parms[1]; // basement 
            var count1 = obj.City.Count(structure => objectTypeFactory.IsStructureType(type, structure));
            var count2 = obj.City.Count(structure => objectTypeFactory.IsStructureType(type2, structure));
            if (count1 < count2)
                return Error.Ok;
            return Error.EffectRequirementNotMet;
        }
    }
}
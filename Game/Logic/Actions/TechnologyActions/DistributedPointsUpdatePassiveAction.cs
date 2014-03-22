using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Setup;

namespace Game.Logic.Actions
{
    public class DistributedPointsUpdatePassiveAction : PassiveAction, IScriptable
    {
        private IStructure structure;

        public override ActionType Type
        {
            get
            {
                return ActionType.DistributedPointsUpdatePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return string.Empty;
            }
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
        }

        public override Error Validate(string[] parms)
        {
            return Error.ActionNotFound;
        }

        public override Error Execute()
        {
            var total = (1 + structure.Lvl) * structure.Lvl / 2;
            var used = structure.Technologies.GetEffects(EffectCode.PointSystemValue, EffectInheritance.Invisible).Sum(tech => (int)tech.Value[0]);

            object current;
            if (structure.Properties.TryGet("Points available", out current) && (int)current == total - used)
            {
                return Error.Ok;
            }

            structure.BeginUpdate();
            structure["Points used"] = used;
            structure["Points available"] = total - used;
            structure.EndUpdate();

            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
        }

        public void ScriptInit(IGameObject gameObject, string[] parms)
        {
            if ((structure = gameObject as IStructure) == null)
            {
                throw new Exception();
            }

            Execute();
        }
    }
}

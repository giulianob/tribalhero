#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Procedures;
using Game.Logic.Triggers;
using Game.Setup;

#endregion

namespace Game.Logic.Actions
{
    public class TechnologyCreatePassiveAction : PassiveAction, IScriptable
    {
        private readonly TechnologyFactory technologyFactory;

        private readonly CallbackProcedure callbackProcedure;

        private byte lvl;

        private IStructure obj;

        private uint techId;

        public override ActionType Type
        {
            get
            {
                return ActionType.TechnologyCreatePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return string.Empty;
            }
        }

        public override Error SystemCancelable
        {
            get
            {
                return Error.UncancelableTechnologyCreate;
            }
        }

        public TechnologyCreatePassiveAction(TechnologyFactory technologyFactory, CallbackProcedure callbackProcedure)
        {
            this.technologyFactory = technologyFactory;
            this.callbackProcedure = callbackProcedure;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
        }

        public void ScriptInit(IGameObject obj, string[] parms)
        {
            this.obj = obj as IStructure;
            if (this.obj == null)
            {
                throw new Exception("Invalid script init obj");
            }

            techId = uint.Parse(parms[0]);
            lvl = byte.Parse(parms[1]);
            Execute();
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            if (obj == null)
            {
                return Error.ObjectNotFound;
            }

            TechnologyBase techBase = technologyFactory.GetTechnologyBase(techId, lvl);
            if (techBase == null)
            {
                return Error.ObjectNotFound;
            }

            Technology tech;
            if (!obj.Technologies.TryGetTechnology(techBase.Techtype, out tech))
            {
                tech = new Technology(techBase);
                obj.Technologies.BeginUpdate();
                obj.Technologies.Add(tech);
                obj.Technologies.EndUpdate();
                callbackProcedure.OnTechnologyUpgrade(obj, tech.TechBase);
            }
            else if (tech.Level != techBase.Level)
            {
                obj.Technologies.BeginUpdate();
                obj.Technologies.Upgrade(new Technology(techBase));
                obj.Technologies.EndUpdate();
                callbackProcedure.OnTechnologyUpgrade(obj, tech.TechBase);
            }

            StateChange(ActionState.Completed);

            return Error.Ok;
        }

        public override void WorkerRemoved(bool wasKilled)
        {
        }

        public override void UserCancelled()
        {
        }
    }
}
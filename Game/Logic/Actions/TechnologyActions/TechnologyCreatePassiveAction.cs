#region

using System;
using Game.Data;
using Game.Logic.Procedures;
using Game.Logic.Triggers;
using Game.Setup;
using Ninject;

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

        public TechnologyCreatePassiveAction(TechnologyFactory technologyFactory, CallbackProcedure callbackProcedure)
        {
            this.technologyFactory = technologyFactory;
            this.callbackProcedure = callbackProcedure;
        }

        #region IScriptable Members

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

        #endregion

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
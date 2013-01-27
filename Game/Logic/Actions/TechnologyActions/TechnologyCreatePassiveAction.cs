#region

using System;
using Game.Data;
using Game.Logic.Procedures;
using Game.Setup;
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    public class TechnologyCreatePassiveAction : PassiveAction, IScriptable
    {
        private byte lvl;

        private IStructure obj;

        private uint techId;

        private TimeSpan ts;

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

        #region IScriptable Members

        public void ScriptInit(IGameObject obj, string[] parms)
        {
            if ((this.obj = obj as IStructure) == null)
            {
                throw new Exception();
            }
            techId = uint.Parse(parms[0]);
            lvl = byte.Parse(parms[1]);
            ts = TimeSpan.FromSeconds(int.Parse(parms[2]));
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

            TechnologyBase techBase = Ioc.Kernel.Get<TechnologyFactory>().GetTechnologyBase(techId, lvl);
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
                Procedure.Current.OnTechnologyChange(obj);
            }
            else
            {

                tech = new Technology(techBase);
                obj.Technologies.BeginUpdate();

                if (!obj.Technologies.Upgrade(tech))
                {
                    obj.EndUpdate();
                    StateChange(ActionState.Failed);
                    return Error.Unexpected;
                }
                obj.Technologies.EndUpdate();
                Procedure.Current.OnTechnologyChange(obj);
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
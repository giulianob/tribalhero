#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    class TechnologyUpgradeAction : ScheduledActiveAction, IScriptable
    {
        private uint cityId;
        private bool isSelfInit;
        private uint structureId;
        private uint techId;

        public TechnologyUpgradeAction()
        {
        }

        public TechnologyUpgradeAction(uint cityId, uint structureId, uint techId)
        {
            this.cityId = cityId;
            this.structureId = structureId;
            this.techId = techId;
        }

        public TechnologyUpgradeAction(uint id,
                                       DateTime beginTime,
                                       DateTime nextTime,
                                       DateTime endTime,
                                       int workerType,
                                       byte workerIndex,
                                       ushort actionCount,
                                       Dictionary<string, string> properties) : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            techId = uint.Parse(properties["tech_id"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.TechnologyUpgrade;
            }
        }

        #region IScriptable Members

        public void ScriptInit(GameObject obj, string[] parms)
        {
            throw new Exception("have to add logic so that upgrading after a building has been knocked down won't upgrade this tech again");

#pragma warning disable 162
            if ((obj = obj as Structure) == null)
                throw new Exception();
            cityId = obj.City.Id;
            structureId = obj.ObjectId;
            techId = uint.Parse(parms[0]);
            isSelfInit = true;
            Execute();
#pragma warning restore 162
        }

        #endregion

        #region IPersistable

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[] { new XmlKvPair("tech_id", techId), new XmlKvPair("city_id", cityId), new XmlKvPair("structure_id", structureId) });
            }
        }

        #endregion

        public override Error Validate(string[] parms)
        {
            byte maxLevel = byte.Parse(parms[1]);

            if (uint.Parse(parms[0]) != techId)
                return Error.ActionInvalid;

            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.ObjectNotFound;

            Technology tech;
            if (!structure.Technologies.TryGetTechnology(techId, out tech))
                return Error.Ok;

            if (tech.Level >= maxLevel)
                return Error.TechnologyMaxLevelReached;

            return Error.Ok;
        }

        public override Error Execute()
        {
            City city;
            Structure structure;
            if (!Global.World.TryGetObjects(cityId, structureId, out city, out structure))
                return Error.ObjectNotFound;

            Technology tech;
            TechnologyBase techBase;
            if (structure.Technologies.TryGetTechnology(techId, out tech))
                techBase = TechnologyFactory.GetTechnologyBase(tech.Type, (byte)(tech.Level + 1));
            else
                techBase = TechnologyFactory.GetTechnologyBase(techId, 1);

            if (techBase == null)
                return Error.ObjectNotFound;

            if (isSelfInit)
            {
                BeginTime = DateTime.UtcNow;
                endTime = DateTime.UtcNow;
            }
            else
            {
                if (!city.Resource.HasEnough(techBase.Resources))
                    return Error.ResourceNotEnough;

                city.BeginUpdate();
                city.Resource.Subtract(techBase.Resources);
                city.EndUpdate();

                BeginTime = DateTime.UtcNow;
                endTime = DateTime.UtcNow.AddSeconds(CalculateTime((int)techBase.Time));
            }

            return Error.Ok;
        }

        private void InterruptCatchAll(bool wasKilled)
        {
            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (!IsValid())
                    return;

                Technology tech;
                TechnologyBase techBase;
                if (city.Technologies.TryGetTechnology(techId, out tech))
                    techBase = TechnologyFactory.GetTechnologyBase(tech.Type, (byte)(tech.Level + 1));
                else
                    techBase = TechnologyFactory.GetTechnologyBase(techId, 1);

                if (techBase == null)
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                if (!wasKilled)
                {
                    city.BeginUpdate();
                    city.Resource.Add(Formula.GetActionCancelResource(BeginTime, techBase.Resources));
                    city.EndUpdate();
                }

                StateChange(ActionState.Failed);
            }
        }

        public override void UserCancelled()
        {
            InterruptCatchAll(false);
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            InterruptCatchAll(wasKilled);
        }

        public override void Callback(object custom)
        {
            City city;
            Structure structure;
            using (new MultiObjectLock(cityId, out city))
            {
                if (!IsValid())
                    return;

                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                Technology tech;
                if (structure.Technologies.TryGetTechnology(techId, out tech))
                {
                    TechnologyBase techBase = TechnologyFactory.GetTechnologyBase(tech.Type, (byte)(tech.Level + 1));

                    if (techBase == null)
                    {
                        StateChange(ActionState.Failed);
                        return;
                    }

                    structure.Technologies.BeginUpdate();
                    if (!structure.Technologies.Upgrade(new Technology(techBase)))
                    {
                        structure.EndUpdate();
                        StateChange(ActionState.Failed);
                        return;
                    }

                    structure.Technologies.EndUpdate();
                }
                else
                {
                    TechnologyBase techBase = TechnologyFactory.GetTechnologyBase(techId, 1);

                    if (techBase == null)
                    {
                        StateChange(ActionState.Failed);
                        return;
                    }

                    structure.Technologies.BeginUpdate();
                    if (!structure.Technologies.Add(new Technology(techBase)))
                    {
                        structure.EndUpdate();
                        StateChange(ActionState.Failed);
                        return;
                    }

                    structure.Technologies.EndUpdate();
                }

                StateChange(ActionState.Completed);
            }
        }
    }
}
#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    class StarvePassiveAction : ScheduledPassiveAction
    {
        private readonly uint cityId;

        public StarvePassiveAction(uint cityId)
        {
            this.cityId = cityId;
        }

        public StarvePassiveAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible, string nlsDescription, Dictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            cityId = uint.Parse(properties["city_id"]);
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StarvePassive;
            }
        }

        private ILockable[] GetTroopLockList(object[] custom)
        {
            var toBeLocked = new List<ILockable>();

            foreach (var stub in ((City)custom[0]).Troops)
            {
                if (stub.StationedCity != null)
                    toBeLocked.Add(stub.StationedCity);
            }

            return toBeLocked.ToArray();
        }

        public override Error Execute()
        {
            BeginTime = DateTime.UtcNow;
            endTime = DateTime.UtcNow.AddSeconds(1);

            return Error.Ok;
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override void Callback(object custom)
        {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                throw new Exception("City not found");

            using (new CallbackLock(GetTroopLockList, new[] {city}, city))
            {
                if (!IsValid())
                    return;

                city.Troops.Starve();

                StateChange(ActionState.Completed);
            }
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
        }

        #region IPersistable

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] {new XmlKvPair("city_id", cityId)});
            }
        }

        #endregion
    }
}
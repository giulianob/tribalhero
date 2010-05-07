#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class StarveAction : ScheduledPassiveAction {        
        private readonly uint cityId;

        public StarveAction(uint cityId) {
            this.cityId = cityId;
        }

        public StarveAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, bool isVisible,
                          Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, isVisible) {
            cityId = uint.Parse(properties["city_id"]);
        }

        ILockable[] GetTroopLockList(object[] custom) {
            List<ILockable> toBeLocked = new List<ILockable>();

            foreach (TroopStub stub in ((City)custom[0]).Troops) {
                if (stub.StationedCity != null)
                    toBeLocked.Add(stub.StationedCity);
            }

            return toBeLocked.ToArray();
        }

        #region IAction Members

        public override ActionType Type {
            get { return ActionType.STARVE; }
        }

        public override Error Execute() {
            beginTime = DateTime.UtcNow;
            endTime = DateTime.UtcNow.AddSeconds(1);

            return Error.OK;
        }

        public override Error Validate(string[] parms) {
            return Error.OK;
        }

        #endregion

        #region ISchedule Members

        public override void Callback(object custom) {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city)) {
                throw new Exception("City not found");
            }

            using (new CallbackLock(GetTroopLockList, new[] { city }, city)) {
                if (!IsValid())
                    return;

                city.Troops.Starve();

                StateChange(ActionState.COMPLETED);
            }
        }

        #endregion

        public override void UserCancelled() {            
        }

        public override void WorkerRemoved(bool wasKilled) {           
        }

        #region IPersistable

        public override string Properties {
            get { return XMLSerializer.Serialize(new[] {new XMLKVPair("city_id", cityId)}); }
        }

        #endregion
    }
}
#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class StarvePassiveAction : ScheduledPassiveAction
    {
        private uint cityId;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        public StarvePassiveAction(IGameObjectLocator gameObjectLocator, ILocker locker)
        {
            this.gameObjectLocator = gameObjectLocator;
            this.locker = locker;
        }

        public StarvePassiveAction(uint cityId, IGameObjectLocator gameObjectLocator, ILocker locker)
            : this(gameObjectLocator, locker)
        {
            this.cityId = cityId;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
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

            foreach (var stub in ((ICity)custom[0]).Troops)
            {
                if (stub.Station != null)
                {
                    toBeLocked.Add(stub.Station);
                }
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
            ICity city;
            if (!gameObjectLocator.TryGetObjects(cityId, out city))
            {
                throw new Exception("City not found");
            }

            locker.Lock(GetTroopLockList, new object[] {city}, city).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                city.Troops.Starve();

                StateChange(ActionState.Completed);
            });
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
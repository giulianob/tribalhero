#region

using System;
using Game.Map;
using Game.Util.Locking;

#endregion

namespace Game.Data
{
    public abstract class GameObject : SimpleGameObject, IGameObject
    {
        private bool isBlocked;

        public bool IsBlocked
        {
            get
            {
                return isBlocked;
            }
            set
            {
                CheckUpdateMode();
                isBlocked = value;
            }
        }

        public ICity City { get; set; }

        public override uint GroupId
        {
            get
            {
                return City.Id;
            }
        }

        public uint WorkerId
        {
            get
            {
                return objectId;
            }
        }

        #region Update Events

        public override void CheckUpdateMode()
        {
            //If city is null then we dont care about being inside of a begin/end update block
            if (!Global.FireEvents || City == null)
            {
                return;
            }

            if (!updating)
            {
                throw new Exception("Changed state outside of begin/end update block");
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(City);
        }

        protected override void Update()
        {
            if (!Global.FireEvents)
            {
                return;
            }

            if (updating)
            {
                return;
            }

            if (City == null)
            {
                return;
            }

            City.ObjUpdateEvent(this, origX, origY);
            World.Current.Regions.ObjectUpdateEvent(this, origX, origY);
        }

        #endregion
    }
}
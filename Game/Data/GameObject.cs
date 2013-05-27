#region

using System;
using Game.Data.Events;
using Game.Util.Locking;

#endregion

namespace Game.Data
{
    public abstract class GameObject : SimpleGameObject, IGameObject
    {
        protected GameObject(uint x, uint y) : base(x, y)
        {
        }

        private uint isBlocked;

        public uint IsBlocked
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

        protected override void CheckUpdateMode()
        {
            //If city is null then we dont care about being inside of a begin/end update block
            if (!Global.Current.FireEvents || City == null)
            {
                return;
            }

            if (!Updating)
            {
                throw new Exception("Changed state outside of begin/end update block");
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(City);
        }

        protected override bool Update()
        {
            if (City == null)
            {
                return false;
            }

            return base.Update();
        }

        public bool CheckBlocked(uint actionId)
        {
            return isBlocked > 0 && isBlocked != actionId;
        }

        #endregion
    }
}
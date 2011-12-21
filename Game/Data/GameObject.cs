#region

using System;
using Game.Logic;
using Game.Map;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Data
{
    public abstract class GameObject : SimpleGameObject, ICanDo
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

        #region ICanDo Members

        public City City { get; set; }

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

        #endregion

        #region Update Events

        public override void CheckUpdateMode()
        {
            //If city is null then we dont care about being inside of a begin/end update block
            if (!Global.FireEvents || City == null)
                return;

            if (!updating)
                throw new Exception("Changed state outside of begin/end update block");

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(City);
        }

        public new void BeginUpdate()
        {
            if (updating)
                throw new Exception("Nesting beginupdate");
            updating = true;

            origX = x;
            origY = y;
        }

        protected new void Update()
        {
            if (!Global.FireEvents)
                return;

            if (updating)
                return;

            if (City == null)
                return;

            City.ObjUpdateEvent(this, origX, origY);
            World.Current.ObjectUpdateEvent(this, origX, origY);
        }

        #endregion
    }
}
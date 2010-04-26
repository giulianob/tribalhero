using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Logic;
using Game.Util;

namespace Game.Data {
    public abstract class GameObject : SimpleGameObject, ICanDo {
        public City City { get; set; }

        #region ICanDo Members

        public uint WorkerId {
            get { return objectId; }
        }

        #endregion

        #region Update Events

        public override void CheckUpdateMode() {
            //If city is null then we dont care about being inside of a begin/end update block
            if (!Global.FireEvents || City == null)
                return;

            if (!updating)
                throw new Exception("Changed state outside of begin/end update block");

            MultiObjectLock.ThrowExceptionIfNotLocked(City);
        }

        public new void BeginUpdate() {
            if (updating)
                throw new Exception("Nesting beginupdate");
            updating = true;

            origX = x;
            origY = y;
        }        

        protected new void Update() {
            if (!Global.FireEvents)
                return;

            if (updating)
                return;

            if (City == null)
                return;

            City.ObjUpdateEvent(this, origX, origY);
            Global.World.ObjectUpdateEvent(this, origX, origY);
        }

        #endregion
    }
}

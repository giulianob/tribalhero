using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data.Troop;
using Game.Logic;
using Game.Setup;
using Game.Util;

namespace Game.Data.Tribe {

    public class Assignment: ISchedule
    {
        DateTime time;
        uint x,y;
        List<TroopStub> stubs;

        void Schedule()
        {
            // stubs.Min(x=>x.City.X)
            // reinsert
        }

        public Assignment(uint x, uint y, DateTime time, TroopStub stub)
        {
            this.time = time;
            this.x = x;
            this.y = y;
            stubs.Add(stub);
        }

        public Error Join(TroopStub stub)
        {
            stubs.Add(stub);
            return Error.Ok;
        }

        #region ISchedule Members
        public bool IsScheduled {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public DateTime Time {
            get { throw new NotImplementedException(); }
        }

        public void Callback(object custom) {

            throw new NotImplementedException();
        }

        #endregion
    }
}

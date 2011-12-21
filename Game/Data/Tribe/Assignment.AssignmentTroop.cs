using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data.Troop;

namespace Game.Data.Tribe
{
	public partial class Assignment
	{
        public class AssignmentTroop
        {
            public TroopStub Stub { get; set; }
            public DateTime DepartureTime { get; set; }
            public bool Dispatched { get; set; }

            public AssignmentTroop(TroopStub stub, DateTime departureTime, bool dispatched = false)
            {
                Stub = stub;
                DepartureTime = departureTime;
                Dispatched = dispatched;
            }
        }
	}
}

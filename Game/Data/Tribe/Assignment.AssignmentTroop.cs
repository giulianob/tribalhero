using System;
using Game.Data.Troop;

namespace Game.Data.Tribe
{
    public partial class Assignment
    {
        public class AssignmentTroop
        {
            public AssignmentTroop(ITroopStub stub, DateTime departureTime, bool dispatched = false)
            {
                Stub = stub;
                DepartureTime = departureTime;
                Dispatched = dispatched;
            }

            public ITroopStub Stub { get; set; }

            public DateTime DepartureTime { get; set; }

            public bool Dispatched { get; set; }
        }
    }
}
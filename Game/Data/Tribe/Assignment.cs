using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

namespace Game.Data.Tribe {

    public class Assignment: ISchedule
    {
        DateTime time;
        uint x, y;
        AttackMode attackMode;

        Dictionary<DateTime, TroopStub> stubs= new Dictionary<DateTime, TroopStub>();

        public Assignment(uint x, uint y, AttackMode mode, DateTime time, TroopStub stub)
        {
            this.time = time;
            this.x = x;
            this.y = y;
            this.attackMode = mode;
            stubs.Add(DepartureTime(stub), stub);
            Global.Scheduler.Put(this);
        }


        public Error Join(TroopStub stub)
        {
            while (!Global.Scheduler.Remove(this))
            {
            }
            stubs.Add(DepartureTime(stub), stub);
            Global.Scheduler.Put(this);
            return Error.Ok;
        }

        private DateTime DepartureTime(TroopStub stub)
        {
            int distance = SimpleGameObject.TileDistance(stub.City.X,stub.City.Y,x,y);
            return DateTime.UtcNow.AddSeconds(Formula.MoveTime(Formula.GetTroopSpeed(stub)) * Formula.MoveTimeMod(stub.City, distance, true));
        }

        private void Dispatch(TroopStub stub) {

         /*   Structure structure = (Structure)Global.World.GetObjects(x, y).Find(x => x is Structure);
            if(structure==null)
            {
                throw  new Exception("nothing to attack, please add code to handle!");
            }
            
            // Create troop object
            if (!Procedure.TroopObjectCreate(stub.City, stub, stub.City.X, stub.City.Y)) {
                throw new Exception("fail to create troop object?!?");
            }
            var aa = new AttackChainAction(stub.City.Id, stub.TroopId, structure.City.Id, structure.ObjectId, attackMode);
            stub.City.Worker.DoPassive(stub.City, aa, true);*/
        }


        #region ISchedule Members

        public bool IsScheduled { get; set; }

        public DateTime Time {
            get
            {
                return stubs.Keys.Min();
            }
        }

        public void Callback(object custom) {
            foreach(var kvp in stubs.Where(z=>z.Key<=DateTime.UtcNow))
            {
                Dispatch(kvp.Value);
            }
            if(stubs.Any())
                Global.Scheduler.Put(this);
        }

        #endregion
    }
}

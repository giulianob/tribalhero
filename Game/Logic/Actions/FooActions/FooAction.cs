using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;

namespace Game.Logic.Actions {

    class FooAction : Action, IScriptable {
        GameObject self;
        byte radius;
        ushort building_type;
        #region IAction Members

        public override ActionType Type {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        bool work(uint x, uint y, object custom) {
            Console.Out.WriteLine("{0},{1}", x, y);
            if (self.distance(x, y) != radius - 1) return true;
            StructureBuildAction ba = new StructureBuildAction(Global.World, self.City, building_type, x, y);
            self.City.Worker.do_passive(self, ba);
            //   self.Worker.do_passive(ba);

            System.Threading.Thread.Sleep(50);
            return true;
        }
        public override int execute() {
            Game.Map.RadiusLocator.foreach_object(self.X, self.Y, radius, false, work, null);
            stateChange(ActionState.COMPLETED);
            return 0;
        }

        public override bool validate(string[] parms) {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region ICanInit Members

        public void ScriptInit(Game.Data.GameObject obj, string[] parms) {
            this.self = obj;
            this.radius = byte.Parse(parms[0]);
            this.building_type = ushort.Parse(parms[1]);
            execute();
        }

        #endregion

        public override void interrupt(ActionInterrupt state) {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

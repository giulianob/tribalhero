using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;

namespace Game.Map {
    class ObjectLink {
        GameObject value;

        public GameObject Value {
            get { return value; }
        }

        public ObjectLink(GameObject gameObj) {
            value = gameObj;
        }
    }
}

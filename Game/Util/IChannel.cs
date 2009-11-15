using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Util {
    public interface IChannel {
        void OnPost(object message);
    }
}

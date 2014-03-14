using Game.Util;

namespace Game.Data
{
    public interface IGlobal
    {
        IChannel Channel { get; }

        bool FireEvents { get; set; }
    }
}
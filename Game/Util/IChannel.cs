using Game.Comm;

namespace Game.Util
{
    public interface IChannel
    {
        void OnPost(Packet message);
    }
}
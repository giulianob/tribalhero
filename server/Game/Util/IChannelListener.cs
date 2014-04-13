using Game.Comm;

namespace Game.Util
{
    public interface IChannelListener
    {
        void OnPost(Packet message);
    }
}
namespace Game.Util {
    public interface IChannel {
        void OnPost(object message);
    }
}
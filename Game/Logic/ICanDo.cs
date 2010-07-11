using Game.Data;

namespace Game.Logic {
    public interface ICanDo {
        City City { get; }
        uint WorkerId { get; }
    }
}
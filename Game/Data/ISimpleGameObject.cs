using System;
using Game.Data.Events;
using Game.Map;
using Game.Util.Locking;

namespace Game.Data
{
    public interface ISimpleGameObject : ILockable, IPrimaryPosition
    {
        event EventHandler<SimpleGameObjectArgs> ObjectUpdated;

        bool InWorld { get; set; }

        GameObjectState State { get; set; }

        ushort Type { get; }

        uint GroupId { get; }

        uint ObjectId { get; }

        byte Size { get; }

        void BeginUpdate();

        void EndUpdate();

        string ToString();
    }
}
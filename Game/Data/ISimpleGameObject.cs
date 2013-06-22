using System;
using Game.Data.Events;
using Game.Map;

namespace Game.Data
{
    public interface ISimpleGameObject : IXYPosition
    {
        event EventHandler<SimpleGameObjectArgs> ObjectUpdated;

        bool InWorld { get; set; }

        GameObjectState State { get; set; }

        ushort Type { get; }

        uint GroupId { get; }

        uint ObjectId { get; }

        void BeginUpdate();

        void EndUpdate();

        string ToString();
    }
}
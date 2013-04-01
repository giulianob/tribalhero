using System;
using Game.Data.Events;

namespace Game.Data
{
    public interface ISimpleGameObject : IXYPosition
    {
        event EventHandler<SimpleGameObjectArgs> ObjectUpdated;

        bool InWorld { get; set; }

        GameObjectState State { get; set; }

        ushort Type { get; }

        uint GroupId { get; }

        uint ObjectId { get; set; }

        uint RelX { get; }

        uint RelY { get; }

        void BeginUpdate();

        void EndUpdate();

        string ToString();

        int TileDistance(uint x1, uint y1);

        int TileDistance(ISimpleGameObject obj);

        int RadiusDistance(uint x1, uint y1);

        int RadiusDistance(ISimpleGameObject obj);
    }
}
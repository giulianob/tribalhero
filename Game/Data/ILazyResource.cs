namespace Game.Data
{
    public interface ILazyResource
    {
        ILazyValue Crop { get; }

        ILazyValue Wood { get; }

        ILazyValue Iron { get; }

        ILazyValue Gold { get; }

        ILazyValue Labor { get; }

        event LazyValue.OnResourcesUpdate ResourcesUpdate;

        void SetLimits(int cropLimit, int goldLimit, int ironLimit, int woodLimit, int laborLimit);

        int FindMaxAffordable(Resource costPerUnit);

        bool HasEnough(Resource cost);

        void Subtract(Resource resource);

        void Subtract(Resource cost, out Resource actual);

        void Subtract(Resource cost, Resource hidden, out Resource actual);

        void Add(Resource resource);

        void Add(int crop, int gold, int iron, int wood, int labor);

        Resource GetResource();

        void BeginUpdate();

        void EndUpdate();

        string ToString();
    }
}
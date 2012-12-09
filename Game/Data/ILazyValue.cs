using System;

namespace Game.Data
{
    public interface ILazyValue
    {
        DateTime LastRealizeTime { get; }

        int Limit { get; set; }

        int Value { get; }

        int RawValue { get; }

        int Rate { get; set; }

        int Upkeep { get; set; }

        event LazyValue.OnResourcesUpdate ResourcesUpdate;

        void Add(int val);

        void Subtract(int val);

        /// <summary>
        ///     Returns the amount of resources received for the given timeframe.
        ///     NOTE: This can return a negative amount if upkeep is higher than rate.
        /// </summary>
        /// <param name="millisecondInterval"></param>
        /// <returns></returns>
        int GetAmountReceived(int millisecondInterval);
    }
}
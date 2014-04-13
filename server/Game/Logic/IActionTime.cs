#region

using System;

#endregion

namespace Game.Logic
{
    public interface IActionTime
    {
        DateTime BeginTime { get; }

        DateTime EndTime { get; }

        DateTime NextTime { get; }
    }
}
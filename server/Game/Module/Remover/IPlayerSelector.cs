using System.Collections.Generic;

namespace Game.Module
{
    public interface IPlayerSelector
    {
        IEnumerable<uint> GetPlayerIds();
    }
}
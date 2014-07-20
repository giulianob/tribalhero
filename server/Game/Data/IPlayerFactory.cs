using System;

namespace Game.Data
{
    public interface IPlayerFactory
    {
        IPlayer CreatePlayer(uint playerid,
                             DateTime created,
                             DateTime lastLogin,
                             string name,
                             string description,
                             PlayerRights playerRights,
                             string sessionId);
    }
}
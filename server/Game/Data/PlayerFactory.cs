using System;
using Game.Setup.DependencyInjection;
using Game.Util;
using Persistance;

namespace Game.Data
{
    public class PlayerFactory : IPlayerFactory
    {
        private readonly IKernel kernel;

        public PlayerFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IPlayer CreatePlayer(uint playerid, DateTime created, DateTime lastLogin, string name, string description, PlayerRights playerRights, string sessionId)
        {
            return new Player(playerid, created, lastLogin, name, description, playerRights, sessionId, kernel.Get<IChannel>(), kernel.Get<IDbManager>());
        }
    }
}
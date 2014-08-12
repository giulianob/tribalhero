using System.Collections.Generic;
using Game.Comm.Api;
using Game.Data;
using Game.Data.Store;
using Game.Setup;

namespace Game.Comm
{
    class DummyLoginHandler : ILoginHandler
    {
        public Error Login(LoginHandlerMode loginMode, string playerName, string playerLoginKey, out LoginResponseData loginData)
        {
            loginData = null;
            
            uint playerId;
            if (!uint.TryParse(playerName, out playerId))
            {
                return Error.PlayerNotFound;
            }

            loginData = new LoginResponseData
            {
                Player = new LoginResponseData.PlayerData
                {
                    Banned = false,
                    Id = playerId,
                    Name = "Player " + playerId,
                    Rights = PlayerRights.Basic
                },
                Achievements = new List<Achievement>(),
                ThemePurchases = new List<ThemePurchase>(),
            };

            return Error.Ok;
        }
    }
}
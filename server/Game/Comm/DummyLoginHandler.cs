using System.Collections.Generic;
using System.Linq;
using Game.Comm.Api;
using Game.Data;
using Game.Data.Store;
using Game.Setup;

namespace Game.Comm
{
    class DummyLoginHandler : ILoginHandler
    {
        private readonly IThemeManager themeManager;

        public DummyLoginHandler(IThemeManager themeManager)
        {
            this.themeManager = themeManager;
        }

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
                ThemePurchases = themeManager.Themes.Where(theme => theme.Id != Theme.DEFAULT_THEME_ID)
                                             .Select(theme => new ThemePurchase {ThemeId = theme.Id})
                                             .ToList()
            };

            return Error.Ok;
        }
    }
}
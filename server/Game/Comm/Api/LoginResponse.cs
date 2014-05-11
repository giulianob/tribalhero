using System.Collections.Generic;
using Game.Data;
using Game.Data.Store;
using Newtonsoft.Json;

namespace Game.Comm.Api
{
    public class LoginResponseData
    {
        public LoginResponseData.PlayerData Player { get; set; }

        public IEnumerable<Achievement> Achievements { get; set; }

        public IEnumerable<ThemePurchase> ThemePurchases { get; set; }

        public class PlayerData
        {
            public uint Id { get; set; }
            
            public string Name { get; set; }

            public string TwoFactorSecretKey { get; set; }

            public PlayerRights Rights { get; set; }

            public bool Banned { get; set; }
        }
    }
}
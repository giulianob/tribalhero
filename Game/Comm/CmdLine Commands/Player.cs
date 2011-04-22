#region

using System;
using System.Linq;
using Game.Data;
using Game.Module;
using Game.Util;
using NDesk.Options;

#endregion

namespace Game.Comm
{
    partial class CmdLineProcessor
    {
        public string CmdBanPlayer(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet {{"?|help|h", v => help = true}, {"player=", v => playerName = v.TrimMatchingQuotes()}};
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return "ban --player=player";

            ApiResponse response = ApiCaller.Ban(playerName);

            return response.Success ? "OK!" : response.ErrorMessage;
        }

        public string CmdUnbanPlayer(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try
            {
                var p = new OptionSet {{"?|help|h", v => help = true}, {"player=", v => playerName = v.TrimMatchingQuotes()}};
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return "unban --player=player";

            ApiResponse response = ApiCaller.Unban(playerName);

            return response.Success ? "OK!" : response.ErrorMessage;
        }

        public string CmdDeletePlayer(Session session, string[] parms)
        {
            bool help = false;
            string playerName = string.Empty;

            try {
                var p = new OptionSet { { "?|help|h", v => help = true }, { "player=", v => playerName = v.TrimMatchingQuotes() } };
                p.Parse(parms);
            } catch (Exception) {
                help = true;
            }

            if (help || string.IsNullOrEmpty(playerName))
                return "delete --player=player";

            uint playerId;
            if (!Global.World.FindPlayerId(playerName, out playerId))
                return "Player not found";

            Player player;
            using (new MultiObjectLock(playerId, out player)) {
                if (player == null)
                    return "Player not found";

                if (player.Session != null)
                {
                    try
                    {
                        player.Session.CloseSession();
                    }
                    catch(Exception)
                    {
                    }
                }

                foreach (City city in player.GetCityList())
                {
                    CityRemover cr = new CityRemover(city.Id);
                    cr.Start();
                }
            }

            return "OK!";

        }
    }
}
#region

using System;
using Game.Data;
using Game.Util;
using NDesk.Options;

#endregion

namespace Game.Comm
{
    partial class CmdLineProcessor
    {
        public string CmdBanPlayer(string[] parms)
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

            uint playerId;
            if (!Global.World.FindPlayerId(playerName, out playerId))
                return "Player not found";

            Player player;
            using (new MultiObjectLock(playerId, out player))
            {
                if (player == null)
                    return "Player not found";

                player.Banned = true;
                Global.DbManager.Save(player);
            }

            return "OK!";
        }

        public string CmdUnbanPlayer(string[] parms)
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

            uint playerId;
            if (!Global.World.FindPlayerId(playerName, out playerId))
                return "Player not found";

            Player player;
            using (new MultiObjectLock(playerId, out player))
            {
                if (player == null)
                    return "Player not found";

                player.Banned = false;
                Global.DbManager.Save(player);
            }

            return "OK!";
        }
    }
}
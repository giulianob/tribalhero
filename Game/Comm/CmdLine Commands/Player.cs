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

            ApiResponse response = ApiCaller.Ban(playerName);

            return response.Success ? "OK!" : response.ErrorMessage;
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

            ApiResponse response = ApiCaller.Unban(playerName);

            return response.Success ? "OK!" : response.ErrorMessage;
        }
    }
}
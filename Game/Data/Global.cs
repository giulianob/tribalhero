#region

using System;
using System.Collections.Generic;
using Game.Module;
using Game.Setup;
using Game.Util;
using log4net;

#endregion

namespace Game.Data
{
    public class Global
    {
        public static Global Current { get; set; }

        public Channel Channel { get; private set; }
        /// <summary>
        /// Matches an alphanumeric string that is at least 2 characters and does not begin or end with space.
        /// </summary>
        public const string ALPHANUMERIC_NAME = "^([a-z0-9][a-z0-9\\s]*[a-z0-9])$";

        public Global(Channel channel)
        {
            Channel = channel;
            FireEvents = true;
            SystemVariables = new Dictionary<string, SystemVariable>();
        }

        public Dictionary<string, SystemVariable> SystemVariables { get; private set; }

        public bool FireEvents { get; set; }

        public bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
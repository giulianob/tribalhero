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
        #region Locks enum

        /// <summary>
        ///     List of global locks. These should all have negative ids so they do not conflict with player locks.
        /// </summary>
        public enum Locks
        {
            Forest = -1
        }

        #endregion

        public static readonly ILog Logger = LogManager.GetLogger(typeof(Global));

        public static readonly Ai Ai = new Ai();

        public static readonly Channel Channel = new Channel();

        static Global()
        {
            FireEvents = true;
            SystemVariables = new Dictionary<string, SystemVariable>();
        }

        public static Dictionary<string, SystemVariable> SystemVariables { get; private set; }

        public static bool FireEvents { get; set; }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
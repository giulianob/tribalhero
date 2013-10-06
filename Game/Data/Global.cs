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
        /// <summary>
        /// Matches an alphanumeric string that is at least 2 characters and does not begin or end with space.
        /// </summary>
        public const string ALPHANUMERIC_NAME = "^([a-z0-9][a-z0-9 ]*[a-z0-9])$";

        #region Locks enum

        /// <summary>
        ///     List of global locks. These should all have negative ids so they do not conflict with player locks.
        /// </summary>
        public enum Locks
        {
            Forest = -1
        }

        #endregion

        public static readonly Channel Channel = new Channel();

        static Global()
        {
            FireEvents = true;
        }

        public static bool FireEvents { get; set; }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
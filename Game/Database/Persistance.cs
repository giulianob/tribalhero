using System;
using Persistance;

namespace Game.Database
{
    public class DbPersistance
    {
        [Obsolete("Inject IDbManager instead")]
        public static IDbManager Current { get; set; }
    }
}
#region

using System;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Map;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;

#endregion

namespace Game.Comm
{
    class BarbarianTribeCommandsLineModule : CommandLineModule
    {
        private readonly IBarbarianTribeManager barbarianTribeManager;

        private readonly ILocker locker;

        private readonly IWorld world;

        public BarbarianTribeCommandsLineModule(IBarbarianTribeManager barbarianTribeManager,
                                                ILocker locker,
                                                IWorld world)
        {
            this.barbarianTribeManager = barbarianTribeManager;
            this.locker = locker;
            this.world = world;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("BarbarianTribeCreate", Create, PlayerRights.Admin);
        }

        private string Create(Session session, string[] parms)
        {
            bool help = false;
            byte? level = null;
            int? campCount = null;
            uint? x = null;
            uint? y = null;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"x=", v => x = uint.Parse(v.TrimMatchingQuotes())},
                        {"y=", v => y = uint.Parse(v.TrimMatchingQuotes())},
                        {"camps=", v => campCount = int.Parse(v.TrimMatchingQuotes())},
                        {"level=", v => level = byte.Parse(v.TrimMatchingQuotes())},
                };
                p.Parse(parms);
                
            }
            catch (Exception)
            {
                help = true;
            }
            
            if (help || !x.HasValue || !y.HasValue || !campCount.HasValue || !level.HasValue)
            {
                return "BarbarianTribeCreate --x=x --y=y --level=level --camps=camps";
            }

            using (locker.Lock(session.Player))
            {
                world.Regions.LockRegion(x.Value, y.Value);
                barbarianTribeManager.CreateBarbarianTribeNear(level.Value, campCount.Value, x.Value, y.Value, 0);
                world.Regions.UnlockRegion(x.Value, y.Value);
            }


            return "OK";
        }       
    }
}
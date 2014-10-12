#region

using System;
using System.Linq;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Map;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;

#endregion

namespace Game.Comm
{
    class BarbarianTribeCommandsLineModule : ICommandLineModule
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

        public void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("BarbarianTribeCreate", Create, PlayerRights.Admin);
            processor.RegisterCommand("BarbarianTribeRelocate", Relocate, PlayerRights.Admin);
            processor.RegisterCommand("BarbarianTribeRelocateAllIdle", RelocateAllIdle, PlayerRights.Admin);
            processor.RegisterCommand("BarbarianTribeRelocateAsNeeded", RelocateAsNeeded, PlayerRights.Admin);
        }

        private string RelocateAllIdle(Session session, string[] parms)
        {
            barbarianTribeManager.RelocateIdleBarbCamps();

            return "OK!";
        }

        private string RelocateAsNeeded(Session session, string[] parms)
        {
            barbarianTribeManager.RelocateAsNeeded();

            return "OK!";
        }

        private string Relocate(Session session, string[] parms)
        {
            bool help = false;
            uint? x = null;
            uint? y = null;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"x=", v => x = uint.Parse(v.TrimMatchingQuotes())},
                        {"y=", v => y = uint.Parse(v.TrimMatchingQuotes())},
                };
                p.Parse(parms);                
            }
            catch (Exception)
            {
                help = true;
            }

            if (help ||
                x == null ||
                y == null ||
                !x.HasValue ||
                !y.HasValue)
            {
                return "BarbarianTribeRelocate --x=x --y=y";
            }

            var barbarianTribe = world.Regions.GetObjectsInTile(x.Value, y.Value).OfType<IBarbarianTribe>().FirstOrDefault();
            if (barbarianTribe == null)
            {
                return "Barbarian tribe not found";
            }

            if (!barbarianTribeManager.RelocateBarbarianTribe(barbarianTribe))
            {
                return "Barbarian tribe is busy and cannot be relocated at this moment.";
            }

            return "OK";
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

            if (help ||
                x == null ||
                y == null ||
                level == null ||
                campCount == null ||
                !x.HasValue ||
                !y.HasValue ||
                !campCount.HasValue ||
                !level.HasValue)
            {
                return "BarbarianTribeCreate --x=x --y=y --level=level --camps=camps";
            }

            var success = false;
            locker.Lock(session.Player).Do(() =>
            {
                world.Regions.LockRegion(x.Value, y.Value);
                success = barbarianTribeManager.CreateBarbarianTribeNear(level.Value, campCount.Value, x.Value, y.Value, 0);
                world.Regions.UnlockRegion(x.Value, y.Value);
            });

            return success ? "OK" : "Unable to find empty spot";
        }       
    }
}
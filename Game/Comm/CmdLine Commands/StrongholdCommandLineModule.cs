#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using NDesk.Options;
using Ninject;
using Persistance;

#endregion

namespace Game.Comm
{
    class StrongholdCommandLineModule : CommandLineModule
    {
        public StrongholdCommandLineModule()
        {
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("StrongholdTransfer", CmdStrongholdTransfer, PlayerRights.Admin);
            processor.RegisterCommand("StrongholdAddTroop", CmdStrongholdAddTroop, PlayerRights.Admin);
        }

        private string CmdStrongholdAddTroop(Session session, string[] parms)
        {
            bool help = false;
            string strongholdName = string.Empty;
            string cityName = string.Empty;
            
            try
            {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "stronghold=", v => strongholdName = v.TrimMatchingQuotes()},
                        { "city=", v => cityName = v.TrimMatchingQuotes()},
                    };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(strongholdName) || string.IsNullOrEmpty(cityName))
                return "StrongholdAddTroop --stronghold=tribe_name --city=city_name";


            uint cityId;
            if (!World.Current.FindCityId(cityName, out cityId))
                return "City not found";

            ICity city;
            if (!World.Current.TryGetObjects(cityId, out city))
                return "City not found";


            IStrongholdManager strongholdManager = Ioc.Kernel.Get<IStrongholdManager>();

            var stronghold = strongholdManager.FirstOrDefault(x => String.Compare(x.Name, strongholdName, StringComparison.OrdinalIgnoreCase) == 0);
            if (stronghold == null)
            {
                return "Stronghold not found";
            }

            using (Concurrency.Current.Lock(city, stronghold))
            {
                if (city.DefaultTroop.Upkeep == 0)
                {
                    return "No troops in the city!";
                }

                ITroopStub stub = city.Troops.Create();
                stub.BeginUpdate();
                stub.AddFormation(FormationType.Defense);
                foreach (var unit in city.DefaultTroop[FormationType.Normal])
                {
                    stub.AddUnit(FormationType.Defense, unit.Key, unit.Value);
                }
                stub.Template.LoadStats(TroopBattleGroup.Defense);
                stub.EndUpdate();

                if (!stronghold.Troops.AddStationed(stub))
                {
                    return "Error Adding to Station";
                }
            }
            return "OK!";
        }

        private string CmdStrongholdTransfer(Session session, string[] parms)
        {
            bool help = false;
            string strongholdName = string.Empty;
            string tribeName = string.Empty;

            try
            {
                var p = new OptionSet
                    {
                        { "?|help|h", v => help = true }, 
                        { "stronghold=", v => strongholdName = v.TrimMatchingQuotes()},
                        { "tribe=", v => tribeName = v.TrimMatchingQuotes()},
                    };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(strongholdName) || string.IsNullOrEmpty(tribeName))
                return "StrongholdTransfer --stronghold=tribe_name --tribe=tribe_name";


            uint tribeId;
            if (!World.Current.FindTribeId(tribeName, out tribeId))
                return "Tribe not found";

            ITribe tribe;
            if (!World.Current.TryGetObjects(tribeId, out tribe))
                return "Tribe not found";


            IStrongholdManager strongholdManager = Ioc.Kernel.Get<IStrongholdManager>();

            var stronghold = strongholdManager.FirstOrDefault(x => String.Compare(x.Name, strongholdName, StringComparison.OrdinalIgnoreCase)==0);
            if(stronghold==null)
            {
                return "Stronghold not found";
            }

            using (Concurrency.Current.Lock(tribe,stronghold))
            {
                strongholdManager.TransferTo(stronghold, tribe);
            }

            return "OK!";
        }
    }
}
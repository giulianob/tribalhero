using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Common;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Forest;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic;
using Game.Map;
using Game.Setup;
using Game.Util;
using Newtonsoft.Json;

namespace Game.Module
{
    public class MapDataExport : ISchedule
    {
        private readonly ILogger logger = LoggerFactory.Current.GetLogger<MapDataExport>();

        private readonly IStrongholdManager strongholdManager;

        private readonly ICityManager cityManager;

        private readonly IForestManager forestManager;

        private readonly IBarbarianTribeManager barbarianTribeManager;

        private readonly ITribeManager tribeManager;

        private readonly IWorld world;

        private readonly IScheduler scheduler;

        public MapDataExport(IStrongholdManager strongholdManager,
                             ICityManager cityManager,
                             IForestManager forestManager,
                             IBarbarianTribeManager barbarianTribeManager,
                             ITribeManager tribeManager,
                             IWorld world,
                             IScheduler scheduler)
        {
            this.strongholdManager = strongholdManager;
            this.cityManager = cityManager;
            this.forestManager = forestManager;
            this.barbarianTribeManager = barbarianTribeManager;
            this.tribeManager = tribeManager;
            this.world = world;
            this.scheduler = scheduler;
        }

        public bool IsScheduled { get; set; }

        public DateTime Time { private set; get; }

        public TimeSpan TimeSpan { get; set; }

        public void Start(TimeSpan timeSpan)
        {
            if (IsScheduled)
            {
                return;
            }

            TimeSpan = timeSpan;
            Time = DateTime.UtcNow.AddSeconds(30);
            scheduler.Put(this);
        }

        public void Callback(object custom)
        {
            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var tmpFile = Path.GetFullPath(Path.Combine(Config.data_folder, "map.json.tmp"));
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }

                using (var file = File.Create(tmpFile))
                    using (StreamWriter sw = new StreamWriter(file))
                        using (JsonWriter jw = new JsonTextWriter(sw))
                        {
                            jw.Formatting = Formatting.None;
                            jw.DateFormatHandling = DateFormatHandling.IsoDateFormat;

                            jw.WriteStartObject();
                            SerializeStrongholds(jw);
                            SerializeBarbarianCamps(jw);
                            SerializeForests(jw);
                            SerializePlayers(jw);
                            SerializeCities(jw);
                            SerializeTribes(jw);
                            jw.WriteEndObject();
                        }

                logger.Info("Generated map export in {0}", stopWatch.Elapsed);
                stopWatch.Stop();


                var fileName = Path.GetFullPath(Path.Combine(Config.data_folder, "map.json"));
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                File.Move(tmpFile, fileName);
            }
            catch(Exception e)
            {
                logger.Error(e, "Error while creating export file");
            }

            Time = SystemClock.Now.Add(TimeSpan);
            scheduler.Put(this);
        }

        private void SerializePlayers(JsonWriter jw)
        {
            jw.WritePropertyName("players");
            jw.WriteStartArray();
            foreach (var player in world.Players.Values)
            {
                jw.WriteStartObject();

                jw.WritePropertyName("id");
                jw.WriteValue(player.PlayerId);
                
                jw.WritePropertyName("name");
                jw.WriteValue(player.Name);

                jw.WritePropertyName("tribe_id");
                var tribesman = player.Tribesman;
                var tribe = tribesman != null ? tribesman.Tribe : null;
                jw.WriteValue(tribe != null ? (uint?)tribe.Id : null);

                jw.WriteEndObject();
            }
            jw.WriteEndArray();
        }

        private void SerializeForests(JsonWriter jw)
        {
            jw.WritePropertyName("forests");
            jw.WriteStartArray();
            foreach (var forest in forestManager.AllForests)
            {
                jw.WriteStartObject();
                
                jw.WritePropertyName("x");
                jw.WriteValue(forest.PrimaryPosition.X);

                jw.WritePropertyName("y");
                jw.WriteValue(forest.PrimaryPosition.Y);

                jw.WritePropertyName("deplete");
                jw.WriteValue(forest.DepleteTime);
                
                jw.WriteEndObject();
            }
            jw.WriteEndArray();
        }

        private void SerializeBarbarianCamps(JsonWriter jw)
        {
            jw.WritePropertyName("barbarian_tribes");
            jw.WriteStartArray();
            foreach (var tribe in barbarianTribeManager.AllTribes.Where(x => x.InWorld))
            {
                jw.WriteStartObject();

                jw.WritePropertyName("level");
                jw.WriteValue(tribe.Lvl);

                jw.WritePropertyName("x");
                jw.WriteValue(tribe.PrimaryPosition.X);

                jw.WritePropertyName("y");
                jw.WriteValue(tribe.PrimaryPosition.Y);
                
                jw.WritePropertyName("camps_remaining");
                jw.WriteValue(tribe.CampRemains);

                jw.WriteEndObject();
            }
            jw.WriteEndArray();
        }

        private void SerializeTribes(JsonWriter jw)
        {
            jw.WritePropertyName("tribes");
            jw.WriteStartArray();
            foreach (var tribe in tribeManager.AllTribes)
            {
                jw.WriteStartObject();

                jw.WritePropertyName("level");
                jw.WriteValue(tribe.Level);

                jw.WritePropertyName("name");
                jw.WriteValue(tribe.Name);

                jw.WritePropertyName("attack_points");
                jw.WriteValue(tribe.AttackPoint);

                jw.WritePropertyName("defense_points");
                jw.WriteValue(tribe.DefensePoint);

                jw.WritePropertyName("victory_points");
                jw.WriteValue(tribe.VictoryPoint);

                jw.WritePropertyName("owner_player_id");
                var owner = tribe.Owner;
                jw.WriteValue(owner == null ? null : (uint?)owner.PlayerId);

                jw.WritePropertyName("created");
                jw.WriteValue(tribe.Created);

                jw.WriteEndObject();
            }
            jw.WriteEndArray();
        } 

        private void SerializeCities(JsonWriter jw)
        {
            jw.WritePropertyName("cities");
            jw.WriteStartArray();
            foreach (var city in cityManager.AllCities().Where(c => c.Deleted == City.DeletedState.NotDeleted))
            {
                jw.WriteStartObject();

                jw.WritePropertyName("id");
                jw.WriteValue(city.Id);

                jw.WritePropertyName("player_id");
                var owner = city.Owner;
                jw.WriteValue(owner == null ? null : (uint?)owner.PlayerId);

                jw.WritePropertyName("name");
                jw.WriteValue(city.Name);

                jw.WritePropertyName("x");
                jw.WriteValue(city.PrimaryPosition.X);

                jw.WritePropertyName("y");
                jw.WriteValue(city.PrimaryPosition.Y);

                jw.WritePropertyName("value");
                jw.WriteValue(city.Value);

                jw.WritePropertyName("radius");
                jw.WriteValue(city.Radius);

                jw.WritePropertyName("loot_stolen");
                jw.WriteValue(city.LootStolen);

                jw.WritePropertyName("attack_points");
                jw.WriteValue(city.AttackPoint);

                jw.WritePropertyName("defense_point");
                jw.WriteValue(city.DefensePoint);

                jw.WritePropertyName("expense_value");
                jw.WriteValue(city.ExpenseValue);

                jw.WritePropertyName("troops");
                jw.WriteStartArray();
                foreach (var troop in city.TroopObjects.Where(x => x.InWorld))
                {
                    jw.WriteStartObject();

                    jw.WritePropertyName("x");
                    jw.WriteValue(troop.PrimaryPosition.X);

                    jw.WritePropertyName("y");
                    jw.WriteValue(troop.PrimaryPosition.Y);

                    jw.WritePropertyName("state");
                    var stub = troop.Stub;
                    jw.WriteValue(stub != null ? stub.State.ToString() : TroopState.Idle.ToString());

                    jw.WriteEndObject();
                }
                jw.WriteEndArray();

                jw.WriteEndObject();
            }
            jw.WriteEndArray();
        }

        private void SerializeStrongholds(JsonWriter jw)
        {
            jw.WritePropertyName("strongholds");
            jw.WriteStartArray();
            foreach (var stronghold in strongholdManager.Where(s => s.StrongholdState != StrongholdState.Inactive))
            {
                jw.WriteStartObject();

                jw.WritePropertyName("id");
                jw.WriteValue(stronghold.ObjectId);

                jw.WritePropertyName("name");
                jw.WriteValue(stronghold.Name);

                jw.WritePropertyName("x");
                jw.WriteValue(stronghold.PrimaryPosition.X);

                jw.WritePropertyName("y");
                jw.WriteValue(stronghold.PrimaryPosition.Y);

                jw.WritePropertyName("gate_open_to_tribe_id");
                var gateOpenTo = stronghold.GateOpenTo;
                jw.WriteValue(gateOpenTo == null ? null : (uint?)gateOpenTo.Id);

                jw.WritePropertyName("owner_tribe_id");
                var owner = stronghold.Tribe;
                jw.WriteValue(owner == null ? null : (uint?)owner.Id);

                jw.WritePropertyName("level");
                jw.WriteValue(stronghold.Lvl);

                jw.WritePropertyName("state");
                jw.WriteValue(stronghold.StrongholdState.ToString());

                jw.WritePropertyName("victory_point_rate");
                jw.WriteValue(stronghold.VictoryPointRate);

                jw.WritePropertyName("gate_max");
                jw.WriteValue(stronghold.GateMax);

                jw.WritePropertyName("theme");
                jw.WriteValue(stronghold.Theme);

                jw.WritePropertyName("date_occupied");
                jw.WriteValue(stronghold.DateOccupied);

                jw.WriteEndObject();
            }
            jw.WriteEndArray();
        }
    }
}
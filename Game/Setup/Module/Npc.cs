using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Game.Logic;
using Game.Setup;
using Game.Data;
using Game.Util;
using Game.Database;
using Game.Logic.Procedures;
using Game.Fighting;

namespace Game.Module {
    public class AI : ISchedule {
        DateTime time;
        List<Player> playerList = new List<Player>();

        public AI() {
            time = DateTime.Now.AddSeconds(30);
            Scheduler.put(this);
        }

        public void Start(Player player) {
            lock (this) {
                playerList.Add(player);
            }
        }
        public void stop(Player player) {
            lock (this) {
                playerList.Remove(player);
            }
        }


        #region ISchedule Members

        public DateTime Time {
            get { return time; }
        }

        public void callback(object custom) {
            lock (this) {
                foreach (Player npc in playerList) {
                    using (new MultiObjectLock(npc)) {
                        List<City> cities = npc.getCityList();
                        foreach (City city in cities) {
                            //city.DefaultTroop.addUnit(Game.Fighting.FormationType.Normal, (ushort)rand.Next(101, 103), 10);
                            // transaction.Save(city.DefaultTroop);
                        }
                        while (npc.getCityList().Count < 200) {
                            Structure structure;
                            if (!Randomizer.MainBuilding(out structure, 2)) {
                                Global.Logger.Info(npc.Name);
                                break;
                            }
                            City city = new City(npc, "NPC", new Resource(500, 500, 500, 500), structure);

                            Global.World.add(city);
                            Global.World.add(structure);

                            TroopStub stub = city.DefaultTroop;
                            stub.BeginUpdate();
                            stub.addFormation(FormationType.Garrison);
                            stub.addFormation(FormationType.Normal);
                            stub.addUnit(FormationType.Normal, 101, 10);
                            stub.addUnit(FormationType.Normal, 102, 10);
                            stub.addUnit(FormationType.Normal, 103, 10);
                            stub.EndUpdate();
                        }
                    }
                }
            }
            time = DateTime.Now.AddSeconds(10);
            Scheduler.put(this);
        }

        #endregion

        public static void Init() {
            for (uint i = 1; i <= 2; ++i) {
                if (!Global.Players.ContainsKey(i)) {
                    Player computer = new Player(i, "Computer" + i);
                    Global.Players.Add(i, computer);
                    Global.dbManager.Save(computer);
                }
                Global.ai.Start(Global.Players[i]);
            }

        }
    }
}

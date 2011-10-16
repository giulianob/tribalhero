using System;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Procedures;
using Game.Setup;
using Moq;

namespace Testing.Fixtures
{
    class SimpleFixture
    {
        /*
        public static void QuickCreate(int players = 0, int cities = 0, int troopsPerCity = 0, bool tribe = false)
        {
            for (int playerCnt = 0; playerCnt < players; playerCnt++)
            {
                Player player = AddPlayer((uint)(playerCnt + 1));
                for (int cityCnt = 0; cityCnt < cities; cityCnt++)
                {
                    City city = AddCity(player, cityCnt + 1, (uint)cityCnt, (uint)cityCnt);
                    for (int troopCnt = 0; troopCnt < troopsPerCity; troopCnt++)
                    {
                        AddTroopStub(city);
                    }
                }

                if (tribe)
                {
                    AddTribe(player);
                }
            }
        }

        public static Player AddPlayer(uint id)
        {
            Player player = new Player(id, DateTime.UtcNow, DateTime.UtcNow, string.Format("Player_{0}", id), "", false);
            Global.World.Players.Add(id, player);

            return player;
        }

        public static City AddCity(Player player, int id, uint x = 0, uint y = 0)
        {
            var mapFactory = new Mock<IMapFactory>();
            mapFactory.Setup(mock => mock.NextLocation(out x, out y, It.IsAny<byte>())).Returns(true);

            MapFactory.Set(mapFactory.Object);

            City city;
            Procedure.Instance().CreateCity(player, string.Format("City_{0}", id), out city);

            return city;
        }

        public static TroopStub AddTroopStub(City city)
        {
            TroopStub stub = new TroopStub();
            city.Troops.Add(stub);

            return stub;
        }

        public static Game.Data.Tribe.Tribe AddTribe(Player owner)
        {
            var tribe = new Game.Data.Tribe.Tribe(owner, string.Format("Tribe_", owner.PlayerId));
            
            return tribe;
        }

        public static BaseBattleStats GetDummyBaseBattleStats()
        {
            return new BaseBattleStats(1, 1, WeaponType.Ball, WeaponClass.Basic, ArmorType.Building1, ArmorClass.Leather, 100, 100, 1, 1, 1, 1, 1, 1);
        }

        public static StructureStats GetDummyStructureStats()
        {
            return new StructureStats(new StructureBaseStats("", "", 1, 1, 1, new Resource(), GetDummyBaseBattleStats(), 1, 1, 1, ClassId.Structure));
        }
         */
    }
}

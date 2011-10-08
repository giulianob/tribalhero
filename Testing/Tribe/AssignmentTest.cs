using Game.Data;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing.Tribe
{
    [TestClass]
    public class AssignmentTest : TestBase
    {
        private Player owner;
        private Game.Data.Tribe.Tribe tribe;
        private City city;
        private TroopStub attackStub;
        private City targetCity;        

        [TestInitialize]
        public void TestInitialize()
        {
            /*
            Global.FireEvents = false;            

            var structureFactory = new Mock<IStructureFactory>();
            Structure mainBuilding = new Structure(SimpleFixture.GetDummyStructureStats());
            structureFactory.Setup(factory => factory.GetNewStructure(It.IsAny<ushort>(), It.IsAny<byte>())).Returns(mainBuilding);
            StructureFactory.Set(structureFactory.Object);

            owner = SimpleFixture.AddPlayer(1);
            city = SimpleFixture.AddCity(owner, 1, 100, 100);
            city.DefaultTroop.AddUnit(FormationType.Normal, 1, 100);
            tribe = SimpleFixture.AddTribe(owner);

            attackStub = new TroopStub();
            attackStub.AddFormation(FormationType.Attack);
            attackStub.AddUnit(FormationType.Attack, 1, 50);

            var targetPlayer = SimpleFixture.AddPlayer(2);
            targetCity = SimpleFixture.AddCity(targetPlayer, 2, 200, 200);
             */
        }

        [TestMethod]
        public void CreateAssignment()
        {
            int id;
            tribe.CreateAssignment(attackStub, targetCity[1].X, targetCity[1].Y, targetCity, SystemClock.Now, AttackMode.Normal, out id);
        }
    }
}

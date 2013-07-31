using System.Collections.Generic;
using Game.Data;
using Game.Data.Stats;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using NSubstitute;
using Ploeh.AutoFixture;

namespace Testing.ActionsTests
{
    public class StructureBuildActiveActionExecuteBuilder
    {
        private readonly IFixture fixture;

        private IStructureBaseStats structureBaseStats;

        private IWorld world;
        
        private ICity city;

        private Formula formula;

        private IStructureCsvFactory structureCsvFactory;

        private IObjectTypeFactory objectTypeFactory;

        private ITileLocator tileLocator;

        private IRoadPathFinder roadPathFinder;

        private IRequirementCsvFactory requirementCsvFactory;

        public StructureBuildActiveActionExecuteBuilder(IFixture fixture)
        {
            this.fixture = fixture;

            requirementCsvFactory = fixture.Freeze<IRequirementCsvFactory>();
            requirementCsvFactory
                    .GetLayoutRequirement(Arg.Any<ushort>(), Arg.Any<byte>())
                    .Validate(Arg.Any<IStructure>(), Arg.Any<ushort>(), Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<byte>())
                    .Returns(true);

            structureBaseStats = fixture.Freeze<IStructureBaseStats>();
            structureBaseStats.Size.Returns<byte>(0);
            var structureCost = new Resource(1234, 8446, 1343, 1234, 1246);
            structureBaseStats.Cost.Returns(structureCost);

            var structure = fixture.Freeze<IStructure>();
            structure.Stats.Base.Returns(structureBaseStats);

            objectTypeFactory = fixture.Freeze<IObjectTypeFactory>();
            objectTypeFactory.IsObjectType("NoRoadRequired", Arg.Any<ushort>()).Returns(true);

            city = fixture.Create<ICity>();
            city.X.Returns<uint>(99);
            city.Y.Returns<uint>(98);
            city.Radius.Returns<byte>(5);
            city.Resource.HasEnough(structureBaseStats.Cost).Returns(true);
            city.CreateStructure(Arg.Any<ushort>(), Arg.Any<byte>(), Arg.Any<uint>(), Arg.Any<uint>()).Returns(structure);           

            roadPathFinder = fixture.Freeze<IRoadPathFinder>();
            roadPathFinder.CanBuild(Arg.Any<uint>(), Arg.Any<uint>(), city, Arg.Any<bool>()).Returns(Error.Ok);
           
            tileLocator = fixture.Freeze<ITileLocator>();
            tileLocator.TileDistance(99, 98, 1, Arg.Any<uint>(), Arg.Any<uint>(), 1).Returns(1);

            world = fixture.Freeze<IWorld>();
            ICity outCity;            
            world.TryGetObjects(Arg.Any<uint>(), out outCity).Returns(x =>
                {
                    x[1] = city;
                    return true;
                });
            world.Regions.GetObjectsInTile(Arg.Any<uint>(), Arg.Any<uint>()).Returns(new List<ISimpleGameObject>());
            world.Regions.IsValidXandY(0, 0).ReturnsForAnyArgs(true);
            world.Regions.Add(structure).Returns(true);

            structureCsvFactory = fixture.Freeze<IStructureCsvFactory>();
            structureCsvFactory.GetBaseStats(0, 0).ReturnsForAnyArgs(structureBaseStats);

            formula = Substitute.For<Formula>();
            formula.CityMaxConcurrentBuildActions(0, 0, null, null).ReturnsForAnyArgs(Error.Ok);
            formula.StructureCost(null, null).ReturnsForAnyArgs(structureCost);
            fixture.Register(() => formula);    
        }

        public StructureBuildActiveAction Build()
        {
            return fixture.Create<StructureBuildActiveAction>();
        }
    }
}
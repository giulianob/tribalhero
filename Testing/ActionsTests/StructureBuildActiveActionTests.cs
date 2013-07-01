using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Data.Stats;
using Game.Map;
using Game.Setup;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.ActionsTests
{
    public class StructureBuildActiveActionTests
    {
        [Theory, AutoNSubstituteData]
        public void Execute_WhenAllStructureTilesAreValid_ReturnsOk(
                [Frozen] IWorld world,
                [Frozen] ITileLocator tileLocator,
                [Frozen] IStructureBaseStats structureBaseStats,
                Fixture fixture)
        {
            var action = new StructureBuildActiveActionExecuteBuilder(fixture).Build();
            
            structureBaseStats.Size.Returns<byte>(11);
            
            tileLocator.ForeachMultitile(action.X, action.Y, 11).Returns(new[] {new Position(1, 1), new Position(1, 2)});
            
            world.Regions.IsValidXandY(Arg.Any<uint>(), Arg.Any<uint>()).Returns(false);
            world.Regions.IsValidXandY(1, 1).Returns(true);
            world.Regions.IsValidXandY(1, 2).Returns(true);            

            action.Execute().Should().Be(Error.Ok);
        }

        [Theory, AutoNSubstituteData]
        public void Execute_WhenAStructureTileIsNotValid_ReturnsError(
                [Frozen] IWorld world,
                [Frozen] ITileLocator tileLocator,
                Fixture fixture)
        {
            var action = new StructureBuildActiveActionExecuteBuilder(fixture).Build();

            world.Regions.IsValidXandY(1, 1).Returns(true);
            world.Regions.IsValidXandY(1, 2).Returns(false);
            tileLocator.ForeachMultitile(0, 0, 0).ReturnsForAnyArgs(new[] {new Position(1, 1), new Position(1, 2)});

            action.Execute().Should().Be(Error.ActionInvalid);
        }        

        [Theory, AutoNSubstituteData]
        public void Execute_WhenAStructureTileIsOutOfCityRadius_ReturnsError(
                [Frozen] IWorld world,
                [Frozen] ITileLocator tileLocator,
                [Frozen] ICity city,
                [Frozen] IStructureBaseStats structureBaseStats,
                Fixture fixture)
        {
            var action = new StructureBuildActiveActionExecuteBuilder(fixture).Build();
            
            structureBaseStats.Size.Returns<byte>(11);
            
            tileLocator.ForeachMultitile(action.X, action.Y, 11).Returns(new[] {new Position(1, 1), new Position(1, 2)});
            tileLocator.TileDistance(city.X, city.Y, 1, 1).Returns(5);
            tileLocator.TileDistance(city.X, city.Y, 1, 2).Returns(6);

            city.Radius.Returns<byte>(6);

            action.Execute().Should().Be(Error.NotWithinWalls);
        }

        [Theory, AutoNSubstituteData]
        public void Execute_WhenAStructureTileIsAlreadyOccupied_ReturnsError(
                [Frozen] IWorld world,
                [Frozen] ITileLocator tileLocator,
                [Frozen] ICity city,
                [Frozen] IStructureBaseStats structureBaseStats,
                Fixture fixture)
        {
            var action = new StructureBuildActiveActionExecuteBuilder(fixture).Build();
            
            structureBaseStats.Size.Returns<byte>(11);
            
            tileLocator.ForeachMultitile(action.X, action.Y, 11).Returns(new[] {new Position(1, 1), new Position(1, 2)});
            world.Regions.GetObjectsInTile(1, 1).Returns(new List<ISimpleGameObject>());
            world.Regions.GetObjectsInTile(1, 2).Returns(new List<ISimpleGameObject> { Substitute.For<IStructure>() });            

            action.Execute().Should().Be(Error.StructureExists);
        }
    }
}
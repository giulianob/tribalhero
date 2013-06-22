using System;
using System.Collections.Generic;
using Common.Testing;
using Game.Data;
using Game.Data.Stats;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using NSubstitute;
using NSubstitute.Core;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.ActionsTests
{
    public class StructureBuildActiveActionTests
    {
        [Theory, AutoNSubstituteData]
        public void Execute_VerifiesAllOfAStructuresXAndYAreValid(
                [Frozen] IWorld world,
                [Frozen] IStructureCsvFactory structureCsvFactory,
                [Frozen] ITileLocator tileLocator,
                ICity city,
                IStructureBaseStats structureBaseStats,
                Fixture fixture)
        {
            var formula = Substitute.For<Formula>();
            formula.StructureCost(city, structureBaseStats).Returns(new Resource());
            formula.ConcurrentBuildUpgrades(Arg.Any<byte>()).Returns(99);
            fixture.Register(() => formula);            

            var action = fixture.Create<StructureBuildActiveAction>();
            
            ICity outCity;            
            world.TryGetObjects(Arg.Any<uint>(), out outCity).Returns(x =>
                {
                    x[1] = city;
                    return true;
                });

            world.Regions.IsValidXandY(0, 0).ReturnsForAnyArgs(true);

            structureBaseStats.Size.Returns<byte>(11);
            structureCsvFactory.GetBaseStats(0, 0).ReturnsForAnyArgs(structureBaseStats);
            tileLocator.ForeachMultitile(action.X, action.Y, 11)
                       .Returns(new[] {new Position(1, 1), new Position(1, 2)});


            action.Execute();


            world.Regions.Received(1).IsValidXandY(1, 1);
            world.Regions.Received(1).IsValidXandY(1, 2);
        }
    }
}
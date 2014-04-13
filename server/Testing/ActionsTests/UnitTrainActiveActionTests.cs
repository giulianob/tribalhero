using System;
using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using NSubstitute;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.ActionsTests
{
    public class UnitTrainActiveActionTests
    {
        public static IEnumerable<object[]> UserCancelledData
        {
            get
            {
                yield return new object[]
                {
                    new UserCancelledTestData
                    {
                        CostPerUnit = new Resource(13, 14, 28, 25, 3),
                        TotalCost = new Resource(13, 14, 28, 25, 3) * 1,
                        InstantTrainCount = 0,
                        TrainCount = 1,
                        XForOne = ushort.MaxValue,
                        CompletedTraining = 0,

                        ExpectedRefundAmount = new Resource(13, 14, 28, 25, 3) * 1,
                        ExpectedCityReceivedCount = 0,
                        ExpectedQueuedCount = 1
                    }
                };

                yield return new object[]
                {
                    new UserCancelledTestData
                    {
                        CostPerUnit = new Resource(13, 14, 28, 25, 3),
                        TotalCost = new Resource(13, 14, 28, 25, 3) * 57,
                        InstantTrainCount = 0,
                        TrainCount = 57,
                        XForOne = 2,
                        CompletedTraining = 0,

                        ExpectedRefundAmount = new Resource(13, 14, 28, 25, 3) * 57,
                        ExpectedCityReceivedCount = 0,
                        ExpectedQueuedCount = 85
                    }
                };

                yield return new object[]
                {
                    new UserCancelledTestData
                    {
                        CostPerUnit = new Resource(13, 14, 28, 25, 3),
                        TotalCost = new Resource(13, 14, 28, 25, 3) * 57,
                        InstantTrainCount = 7,                        
                        TrainCount = 57,
                        XForOne = ushort.MaxValue,
                        CompletedTraining = 0,                        

                        ExpectedRefundAmount = new Resource(13, 14, 28, 25, 3) * 50,
                        ExpectedCityReceivedCount = 7,
                        ExpectedQueuedCount = 50
                    }
                };

                yield return new object[]
                {
                    new UserCancelledTestData
                    {
                        CostPerUnit = new Resource(13, 14, 28, 25, 3),
                        TotalCost = new Resource(13, 14, 28, 25, 3) * 57,
                        InstantTrainCount = 7,                        
                        TrainCount = 57,
                        XForOne = 2,
                        CompletedTraining = 0,                        

                        ExpectedRefundAmount = new Resource(13, 14, 28, 25, 3) * 52,
                        ExpectedCityReceivedCount = 7,
                        ExpectedQueuedCount = 78
                    }
                };

                yield return new object[]
                {
                    new UserCancelledTestData
                    {
                        CostPerUnit = new Resource(13, 14, 28, 25, 3),
                        TotalCost = new Resource(13, 14, 28, 25, 3) * 57,
                        InstantTrainCount = 8,                        
                        TrainCount = 57,
                        XForOne = 2,
                        CompletedTraining = 0,                        

                        ExpectedRefundAmount = new Resource(13, 14, 28, 25, 3) * 51,
                        ExpectedCityReceivedCount = 8,
                        ExpectedQueuedCount = 77
                    }
                };

                yield return new object[]
                {
                    new UserCancelledTestData
                    {
                        CostPerUnit = new Resource(13, 14, 28, 25, 3),
                        TotalCost = new Resource(13, 14, 28, 25, 3) * 57,
                        InstantTrainCount = 9,                        
                        TrainCount = 57,
                        XForOne = 2,
                        CompletedTraining = 0,                        

                        ExpectedRefundAmount = new Resource(13, 14, 28, 25, 3) * 51,
                        ExpectedCityReceivedCount = 9,
                        ExpectedQueuedCount = 76
                    }
                };

                yield return new object[]
                {
                    new UserCancelledTestData
                    {
                        CostPerUnit = new Resource(13, 14, 28, 25, 3),
                        TotalCost = new Resource(13, 14, 28, 25, 3) * 57,
                        InstantTrainCount = 7,                        
                        TrainCount = 57,
                        XForOne = 2,
                        CompletedTraining = 10,

                        ExpectedRefundAmount = new Resource(13, 14, 28, 25, 3) * 45,
                        ExpectedCityReceivedCount = 17,
                        ExpectedQueuedCount = 78
                    }
                };

                yield return new object[]
                {
                    new UserCancelledTestData
                    {
                        CostPerUnit = new Resource(13, 14, 28, 25, 3),
                        TotalCost = new Resource(13, 14, 28, 25, 3) * 769,
                        InstantTrainCount = 132,                        
                        TrainCount = 769,
                        XForOne = 3,
                        CompletedTraining = 141,

                        ExpectedRefundAmount = new Resource(13, 14, 28, 25, 3) * 564,
                        ExpectedCityReceivedCount = 273,
                        ExpectedQueuedCount = 893
                    }
                };
            }
        }

        [Theory, PropertyAutoNSubstituteData("UserCancelledData")]
        public void UserCancelled_WhenUserHasDifferentTechnologies_ShouldRefundCorrectAmount(
            UserCancelledTestData testData,
            [FrozenMock] Formula formula,
            [FrozenMock] UnitFactory unitFactory,
            [Frozen] IWorld world,
            ICity city,
            IStructure structure)
        {
            ushort trainedCount = 0;

            formula.GetInstantTrainCount(structure).ReturnsForAnyArgs(testData.InstantTrainCount);
            formula.GetXForOneCount(structure.Technologies).Returns(testData.XForOne);            
            formula.UnitTrainCost(city, 100, 0).Returns(testData.CostPerUnit);
            formula.GetActionCancelResource(DateTime.MinValue, null)
                   .ReturnsForAnyArgs(c => c.Arg<Resource>());

            city.DefaultTroop.When(m => m.AddUnit(FormationType.Normal, 100, Arg.Any<ushort>()))
                .Do(args =>
                {
                    trainedCount += (ushort)args[2];
                });

            city.Resource.HasEnough(testData.TotalCost).ReturnsForAnyArgs(true);
            city.Id.Returns<uint>(1);
            
            structure.City.Returns(city);

            IStructure outStructure;
            ICity outCity;

            city.TryGetStructure(10, out outStructure).Returns(args =>
            {
                args[1] = structure;
                return true;
            });            
            
            world.TryGetObjects(1, 10, out outCity, out outStructure).Returns(args =>
            {
                args[2] = city;
                args[3] = structure;
                return true;
            });

            var gameObjectLocator = new GameObjectLocatorStub(city);
            var locker = new LockerStub(gameObjectLocator);

            var action = new UnitTrainActiveAction(1, 10, 100, testData.TrainCount, unitFactory, locker, world, formula);
            action.WorkerObject = Substitute.For<ICanDo>();

            action.Execute();

            action.ActionCount.Should().Be(testData.ExpectedQueuedCount);

            city.Resource.Received(1).Subtract(testData.TotalCost);

            for (var i = 0; i < testData.CompletedTraining; i++)
            {
                action.Callback(null);
            }

            action.UserCancelled();

            city.Resource.Received(1).Add(testData.ExpectedRefundAmount);

            trainedCount.Should().Be(testData.ExpectedCityReceivedCount);
        }

        public class UserCancelledTestData
        {
            public ushort TrainCount { get; set; }

            public Resource CostPerUnit { get; set; }
            
            public Resource TotalCost { get; set; }
            
            public int XForOne { get; set; }

            public int InstantTrainCount { get; set; }

            public int CompletedTraining { get; set; }

            public ushort ExpectedQueuedCount { get; set; }

            public Resource ExpectedRefundAmount { get; set; }

            public ushort ExpectedCityReceivedCount { get; set; }
        }
    }
}
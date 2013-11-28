using System;
using System.Collections.Generic;
using FluentAssertions;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
using Game.Setup;
using Ninject;
using Xunit;

namespace Testing.DatabaseTests
{
    public class DbLoaderActionFactoryTest
    {
        [Fact]
        public void CreateScheduledPassiveActionShouldReturnAction()
        {
            using (IKernel kernel = new StandardKernel())
            {
                kernel.Bind<ScheduledPassiveAction>().ToSelf();

                var loader = new DbLoaderActionFactory(kernel);

                var action = loader.CreateScheduledPassiveAction(typeof(ScheduledPassiveActionStub),
                                                                 1,
                                                                 DateTime.Now,
                                                                 DateTime.Now,
                                                                 DateTime.Now,
                                                                 false,
                                                                 "aa",
                                                                 new Dictionary<string, string>());

                action.Should().BeOfType<ScheduledPassiveActionStub>();
            }
        }

        [Fact]
        public void CreateScheduledActiveActionShouldReturnAction()
        {
            using (IKernel kernel = new StandardKernel())
            {
                kernel.Bind<ScheduledActiveActionStub>().ToSelf();

                var loader = new DbLoaderActionFactory(kernel);

                var action = loader.CreateScheduledActiveAction(typeof(ScheduledActiveActionStub),
                                                                1,
                                                                DateTime.Now,
                                                                DateTime.Now,
                                                                DateTime.Now,
                                                                1,
                                                                1,
                                                                1,
                                                                new Dictionary<string, string>());

                action.Should().BeOfType<ScheduledActiveActionStub>();
            }
        }

        [Fact]
        public void CreatePassiveActionShouldReturnAction()
        {
            using (IKernel kernel = new StandardKernel())
            {
                kernel.Bind<PassiveActionStub>().ToSelf();

                var loader = new DbLoaderActionFactory(kernel);

                var action = loader.CreatePassiveAction(typeof(PassiveActionStub),
                                                        1,
                                                        true,
                                                        new Dictionary<string, string>());

                action.Should().BeOfType<PassiveActionStub>();
            }
        }

        [Fact]
        public void CreateChainActionShouldReturnAction()
        {
            using (IKernel kernel = new StandardKernel())
            {
                kernel.Bind<ChainActionStub>().ToSelf();

                var loader = new DbLoaderActionFactory(kernel);

                var action = loader.CreateChainAction(typeof(ChainActionStub),
                                                      1,
                                                      "chain",
                                                      new PassiveActionStub(1, true, new Dictionary<string, string>()),
                                                      ActionState.Completed,
                                                      false,
                                                      new Dictionary<string, string>());

                action.Should().BeOfType<ChainActionStub>();
            }
        }

        #region Stubs

        private class ChainActionStub : ChainAction
        {
            public ChainActionStub(uint id,
                                   string chainCallback,
                                   PassiveAction current,
                                   ActionState chainState,
                                   bool isVisible,
                                   IDictionary<string, string> properties)
            {
            }

            public override ActionType Type
            {
                get
                {
                    return ActionType.CityAttackChain;
                }
            }

            public override string Properties
            {
                get
                {
                    return "properties";
                }
            }

            public override Error Validate(string[] parms)
            {
                return Error.Ok;
            }

            public override Error Execute()
            {
                return Error.Ok;
            }
        }

        private class PassiveActionStub : PassiveAction
        {
            public PassiveActionStub(uint id, bool isVisible, IDictionary<string, string> properties)
            {
            }

            public override ActionType Type
            {
                get
                {
                    return ActionType.CityPassive;
                }
            }

            public override string Properties
            {
                get
                {
                    return "properties";
                }
            }

            public override Error Validate(string[] parms)
            {
                return Error.Ok;
            }

            public override Error Execute()
            {
                return Error.Ok;
            }

            public override void UserCancelled()
            {
            }

            public override void WorkerRemoved(bool wasKilled)
            {
            }
        }

        private class ScheduledActiveActionStub : ScheduledActiveAction
        {
            public ScheduledActiveActionStub(uint id,
                                             DateTime beginTime,
                                             DateTime nextTime,
                                             DateTime endTime,
                                             int workerType,
                                             byte workerIndex,
                                             ushort actionCount,
                                             Dictionary<string, string> properties)
            {
            }

            public override ActionType Type
            {
                get
                {
                    return ActionType.CityPassive;
                }
            }

            public override string Properties
            {
                get
                {
                    return "properties";
                }
            }

            public override ConcurrencyType ActionConcurrency
            {
                get
                {
                    return ConcurrencyType.Normal;
                }
            }

            public override Error Validate(string[] parms)
            {
                return Error.Ok;
            }

            public override Error Execute()
            {
                return Error.Ok;
            }

            public override void UserCancelled()
            {
            }

            public override void WorkerRemoved(bool wasKilled)
            {
            }

            public override void Callback(object custom)
            {
            }
        }

        private class ScheduledPassiveActionStub : ScheduledPassiveAction
        {
            public ScheduledPassiveActionStub(uint id,
                                              DateTime beginTime,
                                              DateTime nextTime,
                                              DateTime endTime,
                                              bool isVisible,
                                              string nlsDescription,
                                              Dictionary<string, string> properties)
            {
            }

            public override ActionType Type
            {
                get
                {
                    return ActionType.CityAttackChain;
                }
            }

            public override string Properties
            {
                get
                {
                    return "properties";
                }
            }

            public override Error Validate(string[] parms)
            {
                return Error.Ok;
            }

            public override Error Execute()
            {
                return Error.Unexpected;
            }

            public override void UserCancelled()
            {
            }

            public override void WorkerRemoved(bool wasKilled)
            {
            }

            public override void Callback(object custom)
            {
            }
        }

        #endregion
    }
}
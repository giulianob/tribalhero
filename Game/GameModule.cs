using System;
using System.IO;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Comm;
using Game.Comm.Channel;
using Game.Comm.CmdLine_Commands;
using Game.Comm.ProcessorCommands;
using Game.Comm.Protocol;
using Game.Comm.Thrift;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Forest;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Logic.Triggers;
using Game.Map;
using Game.Module;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Game.Util.Ninject;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using Persistance;
using Thrift.Server;
using Thrift.Transport;

namespace Game
{
    public class GameModule : NinjectModule
    {
        public override void Load()
        {
            #region World/Map

            Bind<IGlobal>().To<Global>().InSingletonScope();
            Bind<IWorld, IGameObjectLocator>().To<World>().InSingletonScope();
            Bind<IRegion>().To<Region>();
            Bind<IRegionManager>().To<RegionManager>().InSingletonScope();
            Bind<IRegionLocator>().To<RegionLocator>().InSingletonScope();
            Bind<RegionObjectList>().ToSelf();
            Bind<ICityManager>().To<CityManager>().InSingletonScope();
            Bind<IMiniMapRegionManager>().To<MiniMapRegionManager>().InSingletonScope();            
            Bind<IForestManager>().To<ForestManager>().InSingletonScope();
            Bind<IForest>().To<Forest>();
            Bind<IRoadManager>().To<RoadManager>().InSingletonScope();
            Bind<IRoadPathFinder>().To<RoadPathFinder>();

            #endregion

            #region General Comms

            Bind<IChannel>().To<Channel>().InSingletonScope();
            Bind<IPolicyServer>().To<PolicyServer>().InSingletonScope();
            Bind<ITcpServer>().To<TcpServer>().InSingletonScope();
            Bind<TServer>().ToMethod(c => new TSimpleServer(new Notification.Processor(c.Kernel.Get<NotificationHandler>()), new TServerSocket(46000)))
                           .InSingletonScope();
            Bind<IProtocol>().To<PacketProtocol>();

            Bind<Chat>().ToSelf().InSingletonScope();
            Bind<IDynamicAction>().To<DynamicAction>();
            Bind<ICityTriggerManager>().To<CityTriggerManager>().InSingletonScope();

            #endregion
            
            #region Tribes

            Bind<ITribeManager>().To<TribeManager>().InSingletonScope();
            Bind<ITribe>().To<Tribe>();
            Bind<ITribeLogger>().To<TribeLogger>();

            #endregion

            #region Locking

            Bind<DefaultMultiObjectLock.Factory>().ToMethod(c => () => new TransactionalMultiObjectLock(new DefaultMultiObjectLock(),
                                                                                                        c.Kernel.Get<IDbManager>()))
                                                  .InSingletonScope();

            Bind<ILocker>().ToMethod(c =>
            {
                DefaultMultiObjectLock.Factory multiObjectLockFactory = () => new TransactionalMultiObjectLock(new DefaultMultiObjectLock(), c.Kernel.Get<IDbManager>());

                return new DefaultLocker(multiObjectLockFactory,
                                         () => new CallbackLock(multiObjectLockFactory),
                                         c.Kernel.Get<IGameObjectLocator>());
            }).InSingletonScope();

            #endregion

            #region Formulas

            Bind<Formula>().ToSelf().InSingletonScope();

            #endregion

            #region CSV Factories

            Bind<FactoriesInitializer>().ToSelf().InSingletonScope();
            Bind<ActionRequirementFactory>().ToSelf().InSingletonScope();
            Bind<IStructureCsvFactory>().To<StructureCsvFactory>().InSingletonScope();
            Bind<EffectRequirementFactory>().ToSelf().InSingletonScope();
            Bind<InitFactory>().ToSelf().InSingletonScope();            
            Bind<PropertyFactory>().ToSelf().InSingletonScope();
            Bind<IRequirementCsvFactory>().To<RequirementCsvFactory>().InSingletonScope();
            Bind<TechnologyFactory>().ToSelf().InSingletonScope();
            Bind<UnitFactory>().ToSelf().InSingletonScope();
            Bind<IObjectTypeFactory>().To<ObjectTypeFactory>().InSingletonScope();
            Bind<UnitModFactory>().ToSelf().InSingletonScope();
            Bind<MapFactory>().ToSelf().InSingletonScope();
            Bind<NameGenerator>()
                    .ToMethod(c => new NameGenerator(Path.Combine(Config.maps_folder, "strongholdnames.txt")))
                    .WhenInjectedExactlyInto<StrongholdConfigurator>()
                    .InSingletonScope();

            #endregion

            #region Database

            Bind<IDbManager>().ToProvider(new DbManagerProvider()).InSingletonScope();

            #endregion

            #region Battle

            Bind<IBattleReport>().To<BattleReport>();

            Bind<IBattleManagerFactory>().To<BattleManagerFactory>();

            Bind<IBattleReportWriter>().To<SqlBattleReportWriter>();

            Bind<ICombatUnitFactory>().To<CombatUnitFactory>().InSingletonScope();

            Bind<ICombatList>().To<CombatList>().NamedLikeFactoryMethod((ICombatListFactory p) => p.GetCombatList());

            Bind<IBattleFormulas>().To<BattleFormulas>().InSingletonScope();

            #endregion

            #region Processor

            Bind<CommandLineProcessor>().ToMethod(c => new CommandLineProcessor(c.Kernel.Get<AssignmentCommandLineModule>(),
                                                                                c.Kernel.Get<PlayerCommandLineModule>(),
                                                                                c.Kernel.Get<CityCommandLineModule>(),
                                                                                c.Kernel.Get<ResourcesCommandLineModule>(),
                                                                                c.Kernel.Get<TribeCommandLineModule>(),
                                                                                c.Kernel.Get<StrongholdCommandLineModule>(),
                                                                                c.Kernel.Get<RegionCommandsLineModule>(),
                                                                                c.Kernel.Get<BarbarianTribeCommandsLineModule>()))
                                        .InSingletonScope();

            Bind<Processor>().ToMethod(c => new Processor(c.Kernel.Get<AssignmentCommandsModule>(),
                                                          c.Kernel.Get<BattleCommandsModule>(),
                                                          c.Kernel.Get<EventCommandsModule>(),
                                                          c.Kernel.Get<ChatCommandsModule>(),
                                                          c.Kernel.Get<CommandLineCommandsModule>(),
                                                          c.Kernel.Get<LoginCommandsModule>(),
                                                          c.Kernel.Get<MarketCommandsModule>(),
                                                          c.Kernel.Get<MiscCommandsModule>(),
                                                          c.Kernel.Get<PlayerCommandsModule>(),
                                                          c.Kernel.Get<RegionCommandsModule>(),
                                                          c.Kernel.Get<StructureCommandsModule>(),
                                                          c.Kernel.Get<TribeCommandsModule>(),
                                                          c.Kernel.Get<TribesmanCommandsModule>(),
                                                          c.Kernel.Get<StrongholdCommandsModule>(),
                                                          c.Kernel.Get<ProfileCommandsModule>(),
                                                          c.Kernel.Get<TroopCommandsModule>()))
                             .InSingletonScope();

            #endregion
            
            #region Utils

            Bind<IScheduler>().To<ThreadedScheduler>().InSingletonScope();
            Bind<ITileLocator>().To<TileLocator>();
            Bind<GetRandom>().ToMethod(c => new Random().Next);
            Bind<Procedure>().ToSelf().InSingletonScope();
            Bind<BattleProcedure>().ToSelf().InSingletonScope();
            Bind<StrongholdBattleProcedure>().ToSelf().InSingletonScope();
            Bind<CityBattleProcedure>().ToSelf().InSingletonScope();
            Bind<BarbarianTribeBattleProcedure>().ToSelf().InSingletonScope();
            Bind<Random>().ToSelf().InSingletonScope();
            Bind<ISystemVariableManager>().To<SystemVariableManager>().InSingletonScope();

            #endregion

            #region Stronghold

            Bind<IStrongholdManager>().To<StrongholdManager>().InSingletonScope();

            Bind<IStrongholdConfigurator>().To<StrongholdConfigurator>().InSingletonScope();
            Bind<IStronghold>().To<Stronghold>();
            if (Config.stronghold_bypass_activation)
            {
                Bind<IStrongholdActivationCondition>().To<DummyActivationCondition>();
            }
            else
            {
                Bind<IStrongholdActivationCondition>().To<StrongholdActivationCondition>();
            }
            Bind<StrongholdActivationChecker>().ToSelf().InSingletonScope();
            Bind<StrongholdChecker>().ToSelf().InSingletonScope();

            Bind<IStrongholdFactory>().To<StrongholdFactory>();
            Bind<IStrongholdManagerLogger>().To<StrongholdManagerLogger>();

            #endregion

            #region Barbarian Tribe

            Bind<IBarbarianTribeManager>().To<BarbarianTribeManager>().InSingletonScope();
            Bind<IBarbarianTribeConfigurator>().To<BarbarianTribeConfigurator>().InSingletonScope();
            Bind<IBarbarianTribe>().To<BarbarianTribe>();
            Bind<IBarbarianTribeFactory>().To<BarbarianTribeFactory>();
            Bind<BarbarianTribeChecker>().ToSelf().InSingletonScope();

            #endregion

            #region City

            Bind<IReferenceManager>().To<ReferenceManager>();
            Bind<ICityChannel>().To<CityChannel>().InSingletonScope();
            Bind<ICityFactory>().To<CityFactory>().InSingletonScope();
            Bind<IGameObjectFactory>().To<GameObjectFactory>().InSingletonScope();

            Bind<ICity>().To<City>();
            Bind<ITroopManager>().To<TroopManager>();
            Bind<IActionWorker>().To<ActionWorker>();
            Bind<ITechnologyManager>().To<TechnologyManager>();
            Bind<IUnitTemplate>().To<UnitTemplate>();
            Bind<IStructure>().To<Structure>();            

            #endregion

            #region Conventions

            // Binds any interface that ends with Factory to auto factory if it doesn't have any implementations
            var explicitFactoryBindings =
                    Bindings.Where(t => t.Service.Name.EndsWith("Factory"))
                            .ToLookup(k => k.Service.AssemblyQualifiedName, v => v.Service);
            this.Bind(
                      x =>
                      x.FromThisAssembly()
                       .SelectAllInterfaces()
                       .EndingWith("Factory")
                       .Where(t => !explicitFactoryBindings.Contains(t.AssemblyQualifiedName))
                       .BindToFactory(() => new FactoryMethodNameProvider()));

            #endregion
        }
    }
}
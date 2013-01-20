using System;
using System.IO;
using System.Linq;
using System.Text;
using Game.Battle;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Comm;
using Game.Comm.CmdLine_Commands;
using Game.Comm.ProcessorCommands;
using Game.Comm.Protocol;
using Game.Comm.Thrift;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Logic;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Module;
using Game.Setup;
using Game.Util.Locking;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Factory;
using Ninject.Extensions.Logging.Log4net.Infrastructure;
using Ninject.Modules;
using Persistance;
using Persistance.Managers;
using Thrift.Server;
using Thrift.Transport;

namespace Game
{
    public class GameModule : NinjectModule
    {
        public override void Load()
        {
            #region World/Map

            Bind<IWorld, IGameObjectLocator>().To<World>().InSingletonScope();

            Bind<IRegionManager>().To<RegionManager>().InSingletonScope();
            Bind<ICityManager>().To<CityManager>().InSingletonScope();
            Bind<ICityRegionManager>().To<CityRegionManager>().InSingletonScope();

            #endregion

            #region General Comms

            Bind<IPolicyServer>().To<PolicyServer>().InSingletonScope();
            Bind<ITcpServer>().To<TcpServer>().InSingletonScope();
            Bind<TServer>()
                    .ToMethod(
                              c =>
                              new TSimpleServer(new Notification.Processor(c.Kernel.Get<NotificationHandler>()),
                                                new TServerSocket(46000)));
            Bind<IProtocol>().To<PacketProtocol>();
            Bind<ChatCommandsModule>().ToMethod(c =>
                {
                    var writer = new StreamWriter("chat.log", true, Encoding.UTF8);
                    writer.AutoFlush = true;
                    return new ChatCommandsModule(writer);
                });
            Bind<Chat>().ToMethod(c => new Chat(Global.Channel));

            #endregion

            #region Action

            Bind<IActionWorker>().ToMethod(c => new ActionWorker());            

            #endregion

            #region Tribes

            Bind<ITribeManager>().To<TribeManager>().InSingletonScope();
            Bind<ITribe>().To<Tribe>();

            #endregion

            #region Locking

            Bind<DefaultMultiObjectLock.Factory>().ToMethod(c => () => new TransactionalMultiObjectLock(new DefaultMultiObjectLock()));

            Bind<ILocker>().ToMethod(c =>
                {
                    DefaultMultiObjectLock.Factory multiObjectLockFactory =
                            () => new TransactionalMultiObjectLock(new DefaultMultiObjectLock());
                    return new DefaultLocker(multiObjectLockFactory,
                                             () => new CallbackLock(multiObjectLockFactory),
                                             c.Kernel.Get<IGameObjectLocator>());
                }).InSingletonScope();

            #endregion

            #region Formulas

            Bind<Formula>().ToSelf().InSingletonScope();
            Bind<BattleFormulas>().ToSelf().InSingletonScope();

            #endregion

            #region CSV Factories

            Bind<ActionRequirementFactory>()
                    .ToMethod(
                              ctx =>
                              new ActionRequirementFactory(Path.Combine(Config.csv_compiled_folder, "action.csv")))
                    .InSingletonScope();
            Bind<StructureFactory>()
                    .ToMethod(ctx => new StructureFactory(Path.Combine(Config.csv_compiled_folder, "structure.csv")))
                    .InSingletonScope();
            Bind<EffectRequirementFactory>()
                    .ToMethod(
                              ctx =>
                              new EffectRequirementFactory(Path.Combine(Config.csv_compiled_folder,
                                                                        "effect_requirement.csv")))
                    .InSingletonScope();
            Bind<InitFactory>()
                    .ToMethod(ctx => new InitFactory(Path.Combine(Config.csv_compiled_folder, "init.csv")))
                    .InSingletonScope();
            Bind<PropertyFactory>()
                    .ToMethod(ctx => new PropertyFactory(Path.Combine(Config.csv_compiled_folder, "property.csv")))
                    .InSingletonScope();
            Bind<RequirementFactory>()
                    .ToMethod(ctx => new RequirementFactory(Path.Combine(Config.csv_compiled_folder, "layout.csv")))
                    .InSingletonScope();
            Bind<TechnologyFactory>()
                    .ToMethod(
                              ctx =>
                              new TechnologyFactory(Path.Combine(Config.csv_compiled_folder, "technology.csv"),
                                                    Path.Combine(Config.csv_folder, "technology_effects.csv")))
                    .InSingletonScope();
            Bind<UnitFactory>()
                    .ToMethod(ctx => new UnitFactory(Path.Combine(Config.csv_compiled_folder, "unit.csv")))
                    .InSingletonScope();
            Bind<ObjectTypeFactory>()
                    .ToMethod(ctx => new ObjectTypeFactory(Path.Combine(Config.csv_compiled_folder, "object_type.csv")))
                    .InSingletonScope();
            Bind<UnitModFactory>()
                    .ToMethod(ctx => new UnitModFactory(Path.Combine(Config.csv_compiled_folder, "unit_modifier.csv")))
                    .InSingletonScope();
            Bind<MapFactory>()
                    .ToMethod(ctx => new MapFactory(Path.Combine(Config.maps_folder, "CityLocations.txt")))
                    .InSingletonScope();

            #endregion

            #region Database

            Bind<IDbManager>()
                    .ToMethod(
                              context =>
                              new MySqlDbManager(new Log4NetLoggerFactory().GetLogger(typeof(IDbManager)),
                                                 Config.database_host,
                                                 Config.database_username,
                                                 Config.database_password,
                                                 Config.database_database,
                                                 Config.database_timeout,
                                                 Config.database_verbose))
                    .InSingletonScope();

            #endregion

            #region Battle

            Bind<ICity>().To<City>();

            Bind<IBattleReport>().To<BattleReport>();

            Bind<IBattleManagerFactory>().To<BattleManagerFactory>();

            Bind<IBattleReportWriter>().To<SqlBattleReportWriter>();

            Bind<ICombatUnitFactory>().To<CombatUnitFactory>().InSingletonScope();

            Bind<ICombatList>().To<CombatList>().NamedLikeFactoryMethod((ICombatListFactory p) => p.GetCombatList());

            Bind<SimpleStubGenerator>().ToSelf().InSingletonScope();

            #endregion

            #region Processor

            Bind<CommandLineProcessor>().ToMethod(c =>
                {
                    var writer = new StreamWriter("commandline.log", true, Encoding.UTF8) {AutoFlush = true};
                    return new CommandLineProcessor(writer,
                                                    c.Kernel.Get<AssignmentCommandLineModule>(),
                                                    c.Kernel.Get<PlayerCommandLineModule>(),
                                                    c.Kernel.Get<CityCommandLineModule>(),
                                                    c.Kernel.Get<ResourcesCommandLineModule>(),
                                                    c.Kernel.Get<TribeCommandLineModule>(),
                                                    c.Kernel.Get<StrongholdCommandLineModule>());
                }).InSingletonScope();
            Bind<Processor>()
                    .ToMethod(
                              c =>
                              new Processor(c.Kernel.Get<AssignmentCommandsModule>(),
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
            Bind<RadiusLocator>().ToSelf().InSingletonScope();
            Bind<TileLocator>().ToMethod(c => new TileLocator(new Random().Next)).InSingletonScope();
            Bind<ReverseTileLocator>().ToMethod(c => new ReverseTileLocator(new Random().Next)).InSingletonScope();
            Bind<Procedure>().ToSelf().InSingletonScope();
            Bind<BattleProcedure>().ToSelf().InSingletonScope();
            Bind<StrongholdBattleProcedure>().ToSelf().InSingletonScope();
            Bind<CityBattleProcedure>().ToSelf().InSingletonScope();
            Bind<BarbarianTribeBattleProcedure>().ToSelf().InSingletonScope();

            #endregion

            #region Stronghold

            Bind<IStrongholdManager>().To<StrongholdManager>().InSingletonScope();

            Bind<IStrongholdConfigurator>().To<StrongholdConfigurator>().InSingletonScope();
            Bind<IStronghold>().To<Stronghold>();
            if (Config.stronghold_bypass_activation) 
                Bind<IStrongholdActivationCondition>().To<DummyActivationCondition>();
            else
                Bind<IStrongholdActivationCondition>().To<StrongholdActivationCondition>();
            Bind<StrongholdActivationChecker>().ToSelf().InSingletonScope();
            Bind<VictoryPointChecker>().ToSelf().InSingletonScope();

            Bind<IStrongholdFactory>().To<StrongholdFactory>();

            #endregion

            #region Barbarian Tribe

            Bind<IBarbarianTribeManager>().To<BarbarianTribeManager>().InSingletonScope();
            Bind<IBarbarianTribeConfigurator>().To<BarbarianTribeConfigurator>().InSingletonScope();
            Bind<IBarbarianTribe>().To<BarbarianTribe>();
            Bind<IBarbarianTribeFactory>().To<BarbarianTribeFactory>();
            Bind<BarbarianTribeChecker>().ToSelf().InSingletonScope();

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
                       .BindToFactory());

            #endregion
        }
    }
}
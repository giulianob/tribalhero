using System.IO;
using System.Linq;
using Game.Battle;
using Game.Comm;
using Game.Comm.Channel;
using Game.Data;
using Game.Setup;
using Ninject;
using Ninject.Extensions.Logging.Log4net.Infrastructure;
using Ninject.Modules;
using Ninject.Parameters;
using Persistance;
using Persistance.Managers;

namespace Game
{
    class GameModule : NinjectModule
    {
        public override void Load()
        {
            #region General Comms
            Bind<IPolicyServer>().To<PolicyServer>().InSingletonScope();
            Bind<ITcpServer>().To<TcpServer>().InSingletonScope();
            #endregion

            #region CSV Factories
            Bind<ActionFactory>().ToMethod(ctx => new ActionFactory(Path.Combine(Config.csv_compiled_folder, "action.csv"))).InSingletonScope();
            Bind<StructureFactory>().ToMethod(ctx => new StructureFactory(Path.Combine(Config.csv_compiled_folder, "structure.csv"))).InSingletonScope();
            Bind<EffectRequirementFactory>().ToMethod(ctx => new EffectRequirementFactory(Path.Combine(Config.csv_compiled_folder, "effect_requirement.csv"))).InSingletonScope();
            Bind<InitFactory>().ToMethod(ctx => new InitFactory(Path.Combine(Config.csv_compiled_folder, "init.csv"))).InSingletonScope();
            Bind<PropertyFactory>().ToMethod(ctx => new PropertyFactory(Path.Combine(Config.csv_compiled_folder, "property.csv"))).InSingletonScope();
            Bind<RequirementFactory>().ToMethod(ctx => new RequirementFactory(Path.Combine(Config.csv_compiled_folder, "layout.csv"))).InSingletonScope();
            Bind<TechnologyFactory>().ToMethod(ctx => new TechnologyFactory(Path.Combine(Config.csv_compiled_folder, "technology.csv"), Path.Combine(Config.csv_folder, "technology_effects.csv"))).InSingletonScope();
            Bind<UnitFactory>().ToMethod(ctx => new UnitFactory(Path.Combine(Config.csv_compiled_folder, "unit.csv"))).InSingletonScope();
            Bind<ObjectTypeFactory>().ToMethod(ctx => new ObjectTypeFactory(Path.Combine(Config.csv_compiled_folder, "object_type.csv"))).InSingletonScope();
            Bind<UnitModFactory>().ToMethod(ctx => new UnitModFactory(Path.Combine(Config.csv_compiled_folder, "unit_modifier.csv"))).InSingletonScope();
            Bind<MapFactory>().ToMethod(ctx => new MapFactory(Path.Combine(Config.maps_folder, "CityLocations.txt"))).InSingletonScope();
            Bind<CombatUnitFactory>().ToSelf().InSingletonScope();
            #endregion

            #region Database
            Bind<IDbManager>()
                    .ToMethod(context => new MySqlDbManager(
                                                 new Log4NetLoggerFactory().GetLogger(typeof(IDbManager)),
                                                 Config.database_host,
                                                 Config.database_username,
                                                 Config.database_password,
                                                 Config.database_database,
                                                 Config.database_timeout,
                                                 Config.database_verbose)
                    )
                    .InSingletonScope();

            #endregion

            #region Battle
            Bind<IBattleChannel>()
                    .To<BattleChannel>()
                    .WhenInjectedInto<BattleManager>()
                    .WithConstructorArgument("city", ctx => ctx.Request.ParentRequest.Parameters.OfType<ConstructorArgument>().First(p => p.Name == "owner").GetValue(ctx, ctx.Request.Target));

            Bind<ICity>().To<City>();

            Bind<IBattleReport>().To<BattleReport>();
            
            Bind<BattleManager.Factory>().ToMethod(ctx => delegate(City city)
                {
                    var bm = Kernel.Get<BattleManager>(new ConstructorArgument("owner", city));
                    bm.BattleReport.Battle = bm;
                    return bm;
                });

            #endregion
        }
    }
}
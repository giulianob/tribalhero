using System.Linq;
using Game.Battle;
using Game.Comm.Channel;
using Game.Data;
using Game.Setup;
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

            #endregion
        }
    }
}
using Game.Setup;
using Ninject.Activation;
using Ninject.Extensions.Logging.Log4net.Infrastructure;
using Persistance;
using Persistance.Managers;

namespace Game.Database
{
    public class DbManagerProvider : Provider<IDbManager>
    {
        protected override IDbManager CreateInstance(IContext context)
        {
            return new MySqlDbManager(new Log4NetLoggerFactory().GetLogger(typeof(IDbManager)),
                                      Config.database_host,
                                      Config.database_username,
                                      Config.database_password,
                                      Config.database_database,
                                      Config.database_timeout,
                                      Config.database_max_connections,
                                      Config.database_verbose);
        }
    }
}
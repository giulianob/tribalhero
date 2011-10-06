using Game.Setup;
using Ninject;
using Ninject.Extensions.Interception;
using Persistance;

namespace Game.Interceptor
{
    class TransactionStartInterceptor : SimpleInterceptor
    {
        protected override void AfterInvoke(IInvocation invocation)
        {
            Ioc.Kernel.Get<IDbManager>().GetThreadTransaction();
        }
    }
}
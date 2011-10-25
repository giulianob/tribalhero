using Game.Setup;
using Ninject;
using Ninject.Extensions.Interception;
using Persistance;

namespace Game.Interceptor.TransactionEnd
{
    class TransactionEndInterceptor : SimpleInterceptor
    {
        protected override void AfterInvoke(IInvocation invocation)
        {
            var transaction = Ioc.Kernel.Get<IDbManager>().GetThreadTransaction(true);
            if (transaction == null)
                return;
            transaction.Dispose();
        }
    }
}

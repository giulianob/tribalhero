using Game.Setup;
using Ninject;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Attributes;
using Ninject.Extensions.Interception.Request;
using Persistance;

namespace Game.Interceptor
{
    class BlahAttribute : InterceptAttribute
    {
        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            return new BlahInterceptor();
        }
    }

    class BlahInterceptor : SimpleInterceptor
    {
        protected override void BeforeInvoke(IInvocation invocation)
        {
            var x = 3;
        }
    }

    class TransactionStartAttribute : InterceptAttribute
    {
        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            return new TransactionStartInterceptor();
        }
    }

    class TransactionEndAttribute : InterceptAttribute
    {
        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            return new TransactionEndInterceptor();
        }
    }

    class TransactionStartInterceptor : SimpleInterceptor
    {
        protected override void AfterInvoke(IInvocation invocation)
        {
            Ioc.Kernel.Get<IDbManager>().GetThreadTransaction();
        }
    }

    class TransactionEndInterceptor : SimpleInterceptor
    {
        protected override void BeforeInvoke(IInvocation invocation)
        {
            var transaction = Ioc.Kernel.Get<IDbManager>().GetThreadTransaction(true);
            if (transaction == null)
                return;
            transaction.Dispose();
        }
    }
}

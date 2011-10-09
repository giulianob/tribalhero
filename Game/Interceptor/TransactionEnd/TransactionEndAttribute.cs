using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Attributes;
using Ninject.Extensions.Interception.Request;

namespace Game.Interceptor.TransactionEnd
{
    class TransactionEndAttribute : InterceptAttribute
    {
        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            return new TransactionEndInterceptor();
        }
    }
}
using Ninject.Extensions.Interception;

namespace Game.Interceptor.ForceProxyReturn
{
    class ForceProxyReturnInterceptor : SimpleInterceptor
    {
        protected override void AfterInvoke(IInvocation invocation)
        {
            if (invocation.ReturnValue == invocation.Request.Target)
            {
                invocation.ReturnValue = invocation.Request.Proxy;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Attributes;
using Ninject.Extensions.Interception.Request;

namespace Game.Interceptor.ForceProxyReturn
{
    class ForceProxyReturnAttribute : InterceptAttribute
    {
        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            return new ForceProxyReturnInterceptor();
        }
    }
}

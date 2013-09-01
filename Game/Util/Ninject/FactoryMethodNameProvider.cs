using System;
using System.Linq;
using System.Reflection;
using Ninject.Extensions.Factory;

namespace Game.Util.Ninject
{
    /// <summary>
    /// Supports the FactoryReturns attribute to tell which concrete type to pass back
    /// </summary>
    public class FactoryMethodNameProvider : StandardInstanceProvider
    {
        protected override Type GetType(MethodInfo methodInfo, object[] arguments)
        {
            var returnAttribute = methodInfo.GetCustomAttributes(typeof(FactoryReturnsAttribute), false).FirstOrDefault();

            return returnAttribute != null ? ((FactoryReturnsAttribute)returnAttribute).ReturnType : base.GetType(methodInfo, arguments);
        }
    }
}
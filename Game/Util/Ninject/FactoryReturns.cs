using System;

namespace Game.Util.Ninject
{
    public class FactoryReturnsAttribute : Attribute
    {
        public Type ReturnType { get; set; }

        public FactoryReturnsAttribute(Type returnType)
        {
            ReturnType = returnType;
        }
    }
}
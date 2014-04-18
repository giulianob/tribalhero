using System;

namespace Game.Setup.DependencyInjection
{
    public interface IKernel
    {
        T Get<T>() where T: class;

        object Get(Type type);
    }
}
using System;
using SimpleInjector;

namespace Game.Setup.DependencyInjection
{
    /// <summary>
    ///     A static container for the kernel.
    /// </summary>
    public class Ioc : IKernel
    {
        private static IKernel kernel;

        private static readonly object lck = new object();

        [Obsolete("Inject the dependency instead.")]
        public static IKernel Kernel
        {
            get
            {
                return kernel;
            }
            set
            {
                lock (lck)
                {
                    if (kernel != null)
                    {
                        throw new NotSupportedException("The static container already has a kernel associated with it!");
                    }

                    kernel = value;
                }
            }
        }

        private readonly Container container;

        public Ioc(Container container)
        {
            this.container = container;
        }

        public T Get<T>() where T : class
        {
            return container.GetInstance<T>();
        }

        public object Get(Type type)
        {
            return container.GetInstance(type);
        }
    }
}
using System;
using Ninject;

namespace Game.Setup
{
    /// <summary>
    /// A static container for the kernel.
    /// </summary>
    public static class Ioc
    {
        #region Fields

        private static IKernel _kernel;
        private static object lck = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the kernel that is used in the application.
        /// </summary>
        public static IKernel Kernel
        {
            get
            {
                return _kernel;
            }
            set
            {
                lock (lck)
                {
                    if (_kernel != null)
                    {
                        throw new NotSupportedException("The static container already has a kernel associated with it!");
                    }

                    _kernel = value;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Injects the specified instance by using the container's kernel.
        /// </summary>
        /// <param name="instance">The instance to inject.</param>
        public static void Inject(object instance)
        {
            if (_kernel == null)
            {
                throw new InvalidOperationException(
                        String.Format("The type {0} requested an injection, but no kernel has been registered for the application." + instance.GetType()));
            }

            lock (lck)
            {
                _kernel.Inject(instance);
            }
        }

        #endregion
    }
}
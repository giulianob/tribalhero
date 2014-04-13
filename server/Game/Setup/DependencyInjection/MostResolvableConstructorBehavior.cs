using System;
using System.Linq;
using System.Reflection;
using SimpleInjector;
using SimpleInjector.Advanced;

namespace Game.Setup.DependencyInjection
{
    public class MostResolvableConstructorBehavior : IConstructorResolutionBehavior
    {
        private readonly Container container;

        public MostResolvableConstructorBehavior(Container container)
        {
            this.container = container;
        }

        private bool IsCalledDuringRegistrationPhase
        {
            get
            {
                return !this.container.IsLocked();
            }
        }

        public ConstructorInfo GetConstructor(Type serviceType,
                                              Type implementationType)
        {
            try
            {
                return (from ctor in implementationType.GetConstructors()
                        let parameters = ctor.GetParameters()
                        orderby parameters.Length descending
                        where this.IsCalledDuringRegistrationPhase || parameters.All(this.CanBeResolved)
                        select ctor)
                        .First();
            }
            catch(Exception e)
            {
                throw new Exception(string.Format("Failed to resolve constructor for type {0}", implementationType.Name), e);
            }
        }

        private bool CanBeResolved(ParameterInfo p)
        {
            return this.container.GetRegistration(p.ParameterType) != null;
        }
    }
}
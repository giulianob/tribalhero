using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Ploeh.AutoFixture.Kernel;

namespace Testing
{
    public static class FixtureHelper
    {
        public static IFixture Create()
        {
            return new Fixture().Customize(new AutoNSubstituteCustomization());            
        }

        public static IFixture PickGreedyConstructor<T>(this IFixture fixture)
        {
            fixture.Customize<T>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            return fixture;
        }
    }
}
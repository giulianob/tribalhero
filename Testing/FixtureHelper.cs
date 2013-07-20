using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;

namespace Testing
{
    public class FixtureHelper
    {
        public static IFixture Create()
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            fixture.OmitAutoProperties = true;
            return fixture;
        } 
    }
}
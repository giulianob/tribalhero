using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;

namespace Testing
{
    public class FixtureHelper
    {
        public static IFixture Create()
        {
            return new Fixture().Customize(new AutoNSubstituteCustomization());            
        } 
    }
}
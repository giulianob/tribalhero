using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Common.Testing
{
    public class InlineAutoNSubstituteDataAttribute : CompositeDataAttribute
    {
        public InlineAutoNSubstituteDataAttribute(params object[] values)
                : base(new DataAttribute[] {new InlineDataAttribute(values), new AutoNSubstituteDataAttribute()})
        {
        }
    }
}
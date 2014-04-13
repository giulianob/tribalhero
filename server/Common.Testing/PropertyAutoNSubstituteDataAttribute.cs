using System;
using System.Collections.Generic;
using Xunit.Extensions;

namespace Common.Testing
{
    public class PropertyAutoNSubstituteDataAttribute : PropertyDataAttribute
    {
        public PropertyAutoNSubstituteDataAttribute(string propertyName)
                : base(propertyName)
        {
        }

        public override IEnumerable<object[]> GetData(System.Reflection.MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            foreach (var values in base.GetData(methodUnderTest, parameterTypes))
            {
                // The params returned by the base class are the first m params, 
                // and the rest of the params can be satisfied by AutoFixture using
                // its InlineAutoDataAttribute class.
                var iada = new InlineAutoNSubstituteDataAttribute(values);
                foreach (var parameters in iada.GetData(methodUnderTest, parameterTypes))
                {
                    yield return parameters;
                }
            }
        }
    }
}
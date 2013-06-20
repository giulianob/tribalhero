using FluentAssertions;
using Game.Util.Ninject;
using Ninject;
using Ninject.Extensions.Factory;
using Xunit;

namespace Testing.NinjectTests
{
    public class FactoryMethodNameProviderTests
    {
        [Fact]
        public void WhenMethodHasAttribute_ShouldInjectCorrectType()
        {
            using (IKernel kernel = new StandardKernel())
            {
                kernel.Bind<ITestFactory>().ToFactory(() => new FactoryMethodNameProvider());

                var testFactory = kernel.Get<ITestFactory>();

                var firstType = testFactory.CreateFirstType("someName");
                firstType.Should().BeOfType<FirstType>();
                firstType.Name.Should().Be("someName");
            }
        }

        public interface ITestFactory
        {
            [FactoryReturns(typeof(FirstType))]
            ISomeType CreateFirstType(string name);
        }

        public interface ISomeType
        {
            string Name { get; set; }
        }

        public class FirstType : ISomeType
        {
            public string Name { get; set; }

            public FirstType(string name)
            {
                Name = name;
            }
        }
    }
}
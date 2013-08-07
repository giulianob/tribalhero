using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Map;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.MapTests
{
    public class ObjectListTests
    {
        [Theory, AutoNSubstituteData]
        public void Add_WhenObjectDoesNotYetExist_AddsObject(
                ISimpleGameObject simpleGameObject,
                RegionObjectList objectList)
        {
            objectList.Add(simpleGameObject, 10, 13);

            objectList.Get(10, 13).Should().Equal(simpleGameObject);
        }

        [Theory, AutoNSubstituteData]
        public void Add_WhenAnObjectAlreadyExistsInIndex_AddsObjectToExistingIndex(
                ISimpleGameObject simpleGameObject1,
                ISimpleGameObject simpleGameObject2,
                RegionObjectList objectList)
        {
            objectList.Add(simpleGameObject1, 10, 13);
            objectList.Add(simpleGameObject2, 10, 13);

            objectList.Get(10, 13).Should().Equal(simpleGameObject1, simpleGameObject2);
        }

    }
}
using Common.Testing;
using FluentAssertions;
using Game.Data.Troop;
using Xunit.Extensions;

namespace Testing.TroopTests
{
    public class SimpleStubTests
    {
        [Theory, AutoNSubstituteData]
        public void RemoveFromFormation_WhenDoesntHaveFormation_ReturnsFailureAndDoesNotModifyTroop(
                IFormation sourceFormation,
                SimpleStub unitsToRemove,
                SimpleStub troopStub)
        {
            troopStub.AddUnit(FormationType.Normal, 100, 200);

            unitsToRemove.AddUnit(FormationType.Attack, 100, 10);

            troopStub.RemoveFromFormation(FormationType.Attack, unitsToRemove).Should().BeFalse();
            troopStub.TotalCount.Should().Be(200);
        }

        [Theory, AutoNSubstituteData]
        public void RemoveFromFormation_WhenHasEnoughUnits_ReturnsTrueAndRemovesUnitsFromCorrectFormation(
                IFormation sourceFormation,
                SimpleStub unitsToRemove,
                SimpleStub troopStub)
        {
            troopStub.AddUnit(FormationType.Normal, 100, 200);
            troopStub.AddUnit(FormationType.Normal, 101, 100);

            unitsToRemove.AddUnit(FormationType.Attack, 100, 10);
            unitsToRemove.AddUnit(FormationType.Defense, 101, 10);            

            troopStub.RemoveFromFormation(FormationType.Normal, unitsToRemove).Should().BeTrue();
            troopStub[FormationType.Normal][100].Should().Be(190);
            troopStub[FormationType.Normal][101].Should().Be(90);
        }

        [Theory, AutoNSubstituteData]
        public void RemoveFromFormation_WhenDoesNotHaveEnoughUnits_ReturnsFalseAndDoesNotModifyTroop(
                IFormation sourceFormation,
                SimpleStub unitsToRemove,
                SimpleStub troopStub)
        {
            troopStub.AddUnit(FormationType.Normal, 100, 200);
            troopStub.AddUnit(FormationType.Normal, 101, 100);

            unitsToRemove.AddUnit(FormationType.Attack, 100, 10);
            unitsToRemove.AddUnit(FormationType.Defense, 101, 200);            

            troopStub.RemoveFromFormation(FormationType.Normal, unitsToRemove).Should().BeFalse();
            troopStub[FormationType.Normal][100].Should().Be(200);
            troopStub[FormationType.Normal][101].Should().Be(100);
        }

        [Theory, AutoNSubstituteData]
        public void RemoveFromFormation_WhenSourceHasSameUnitsInDifferentFormationsAndTroopDoesNotHaveEnough_ReturnsFalseAndDoesNotModifyTroop(
                IFormation sourceFormation,
                SimpleStub unitsToRemove,
                SimpleStub troopStub)
        {
            troopStub.AddUnit(FormationType.Normal, 100, 200);

            unitsToRemove.AddUnit(FormationType.Attack, 100, 150);
            unitsToRemove.AddUnit(FormationType.Defense, 100, 150);

            troopStub.RemoveFromFormation(FormationType.Normal, unitsToRemove).Should().BeFalse();
            troopStub[FormationType.Normal][100].Should().Be(200);
        }

        [Theory, AutoNSubstituteData]
        public void RemoveFromFormation_WhenTroopHasUnitsButNotInSpecifiedFormation_ReturnsFalseAndDoesNotModifyTroop(
                IFormation sourceFormation,
                SimpleStub unitsToRemove,
                SimpleStub troopStub)
        {
            troopStub.AddUnit(FormationType.Defense, 100, 200);

            unitsToRemove.AddUnit(FormationType.Attack, 100, 150);

            troopStub.RemoveFromFormation(FormationType.Normal, unitsToRemove).Should().BeFalse();
            troopStub[FormationType.Defense][100].Should().Be(200);
        }

        [Theory, AutoNSubstituteData]
        public void RemoveFromFormation_WhenSourceHasSameUnitsInDifferentFormationsAndTroopDoesHasEnough_ReturnsTrueAndRemovesUnits(
                IFormation sourceFormation,
                SimpleStub unitsToRemove,
                SimpleStub troopStub)
        {
            troopStub.AddUnit(FormationType.Normal, 100, 310);

            unitsToRemove.AddUnit(FormationType.Attack, 100, 150);
            unitsToRemove.AddUnit(FormationType.Defense, 100, 150);

            troopStub.RemoveFromFormation(FormationType.Normal, unitsToRemove).Should().BeTrue();
            troopStub[FormationType.Normal][100].Should().Be(10);
        }
    }
}
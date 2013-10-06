#region

using System.Collections.Generic;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Xunit.Extensions;

#endregion

namespace Testing.TroopTests
{
    public class BattleProcedureTest
    {
        [Theory, AutoNSubstituteData]
        public void MoveFromBattleToNormal(CityBattleProcedure cityBattleProcedure, IGlobal global, TroopStub stub)
        {
            Global.Current = global;
            Global.Current.FireEvents.Returns(false);

            stub.AddFormation(FormationType.Normal);
            stub.AddFormation(FormationType.Garrison);
            stub.AddFormation(FormationType.InBattle);

            stub.AddUnit(FormationType.Normal, 101, 10);

            cityBattleProcedure.MoveUnitFormation(stub, FormationType.Normal, FormationType.InBattle);
            cityBattleProcedure.MoveUnitFormation(stub, FormationType.InBattle, FormationType.Normal);

            Assert.True(stub[FormationType.Normal].Type == FormationType.Normal);
            Assert.True(stub[FormationType.InBattle].Type == FormationType.InBattle);
        }

        [Theory, AutoNSubstituteData]
        public void SenseOfUrgency_WhenCityHasNoSenseOfUrgencyTechnologies_DoesntHealStructures(
                ICity city,
                BattleProcedure battleProcedure)
        {
            city.Technologies.GetEffects(EffectCode.SenseOfUrgency).Returns(new List<Effect>());

            battleProcedure.SenseOfUrgency(city, 1000);

            city.Received(0).GetEnumerator();
        }

        [Theory, AutoNSubstituteData]
        public void SenseOfUrgency_WhenCityHasNoSenseOfUrgencyTechnologies_HealsHurtStructuresThatArentInBattle(
                ICity city,
                IStructure inBattleStructure,
                IStructure fullyHealedStructure,
                IStructure hurtStructure,
                BattleProcedure battleProcedure)
        {
            inBattleStructure.State = GameObjectStateFactory.BattleState(0);
            inBattleStructure.Stats.Hp.Returns(50m);
            inBattleStructure.Stats.Base.Battle.MaxHp.Returns(100m);

            fullyHealedStructure.State = GameObjectStateFactory.NormalState();
            fullyHealedStructure.Stats.Hp.Returns(75m);
            fullyHealedStructure.Stats.Base.Battle.MaxHp.Returns(75m);

            hurtStructure.State = GameObjectStateFactory.NormalState();
            hurtStructure.Stats.Hp.Returns(100m);
            hurtStructure.Stats.Base.Battle.MaxHp.Returns(200m);

            city.GetEnumerator().Returns(x =>
                                         new List<IStructure> {inBattleStructure, fullyHealedStructure, hurtStructure}.GetEnumerator());
            city.Technologies.GetEffects(EffectCode.SenseOfUrgency).Returns(new List<Effect>
            {
                    new Effect {Id = EffectCode.SenseOfUrgency, Value = new object[] {50}}
            });

            battleProcedure.SenseOfUrgency(city, 50);

            inBattleStructure.Stats.Hp.Should().Be(50m);
            hurtStructure.Stats.Hp.Should().Be(125m);
            fullyHealedStructure.Stats.Hp.Should().Be(75m);

            hurtStructure.Received(1).BeginUpdate();
            hurtStructure.Received(1).EndUpdate();

            fullyHealedStructure.Received(0).BeginUpdate();
            fullyHealedStructure.Received(0).EndUpdate();

            inBattleStructure.Received(0).BeginUpdate();
            inBattleStructure.Received(0).EndUpdate();
        }

        [Theory, AutoNSubstituteData]
        public void SenseOfUrgency_WhenCityHasMultipleSenseOfUrgencyTechnologies_ShouldStackUpTo100Percent(
                ICity city,
                IStructure structure,
                BattleProcedure battleProcedure)
        {
            structure.State = GameObjectStateFactory.NormalState();
            structure.Stats.Hp.Returns(50m);
            structure.Stats.Base.Battle.MaxHp.Returns(200m);

            city.GetEnumerator().Returns(x =>
                                         new List<IStructure> {structure}.GetEnumerator());
            city.Technologies.GetEffects(EffectCode.SenseOfUrgency).Returns(new List<Effect>
            {
                    new Effect {Id = EffectCode.SenseOfUrgency, Value = new object[] {50}},
                    new Effect {Id = EffectCode.SenseOfUrgency, Value = new object[] {25}},
                    new Effect {Id = EffectCode.SenseOfUrgency, Value = new object[] {50}}
            });

            battleProcedure.SenseOfUrgency(city, 50);

            structure.Stats.Hp.Should().Be(100m);
        }
    }
}
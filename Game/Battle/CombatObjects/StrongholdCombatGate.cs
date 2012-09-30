using System;
using Game.Data.Stronghold;
using Game.Setup;

namespace Game.Battle.CombatObjects
{
    public class StrongholdCombatGate : StrongholdCombatStructure
    {
        public StrongholdCombatGate(uint id, uint battleId, ushort type, byte lvl, decimal hp, IStronghold stronghold, StructureFactory structureFactory, BattleFormulas battleFormulas)
                : base(id, battleId, type, lvl, hp, stronghold, structureFactory, battleFormulas)
        {
        }

        public override void TakeDamage(decimal dmg, out Data.Resource returning, out int attackPoints)
        {
            base.TakeDamage(dmg, out returning, out attackPoints);

            // Intead of setting it like this make sure you do
            // Stronghold.Gate = hp;
            throw new Exception("Fix this");
        }
    }
}
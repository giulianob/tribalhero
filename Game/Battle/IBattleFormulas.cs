using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stats;
using Game.Data.Stronghold;
using Game.Data.Troop;

namespace Game.Battle
{
    public interface IBattleFormulas
    {
        decimal GetDmgWithMissChance(int attackersUpkeep, int defendersUpkeep, decimal dmg, IBattleRandom random);

        int GetUnitsPerStructure(byte level);

        decimal GetAttackerDmgToDefender(ICombatObject attacker, ICombatObject target, uint round);

        double GetDmgModifier(ICombatObject attacker, ICombatObject target, uint round);

        int GetLootPerRoundForCity(ICity city);

        Resource GetRewardResource(ICombatObject attacker, ICombatObject defender);

        short GetStamina(ITroopStub stub, ICity city);

        short GetStamina(ITroopStub stub, IStronghold targetStronghold);

        short GetStamina(ITroopStub stub, IBarbarianTribe barbarianTribe);

        ushort GetStaminaReinforced(ICity city, ushort stamina, uint round);

        ushort GetStaminaRoundEnded(ICity city, ushort stamina, uint round);

        short GetStaminaStructureDestroyed(short stamina, ICombatObject combatStructure);

        ushort GetStaminaDefenseCombatObject(ICity city, ushort stamina, uint round);

        bool UnitStatModCheck(IBaseBattleStats stats, TroopBattleGroup group, string value);

        BattleStats LoadStats(IBaseBattleStats stats, ICity city, TroopBattleGroup group);

        BattleStats LoadStats(IStructure structure);

        BattleStats LoadStats(ushort type, byte lvl, ICity city, TroopBattleGroup group);

        Resource GetBonusResources(ITroopObject troop, int originalCount, int remainingCount);

        int GetNumberOfHits(ICombatObject currentAttacker);

        decimal SplashReduction(CityCombatObject defender, decimal dmg, int attackIndex);
    }
}
namespace Game.Logic.Actions
{
    public interface IActionFactory
    {
        AttackChainAction CreateAttackChainAction(uint cityId, byte stubId, uint targetCityId, uint targetStructureId, AttackMode mode);
        DefenseChainAction CreateDefenseChainAction(uint cityId, byte stubId, uint targetCityId, AttackMode mode);
    }
}

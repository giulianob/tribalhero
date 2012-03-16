using System.Collections.Generic;
using Game.Data;

namespace Game.Logic.Actions
{
    public interface IActionFactory
    {
        AttackChainAction CreateAttackChainAction(uint cityId, byte stubId, uint targetCityId, uint targetStructureId, AttackMode mode);

        StructureBuildActiveAction CreateStructureBuildActiveAction(uint cityId, ushort type, uint x, uint y, byte level);

        ResourceSendActiveAction CreateResourceSendActiveAction(uint cityId, uint structureId, uint targetCityId, Resource resource);

        BattlePassiveAction CreateBattlePassiveAction(uint cityId);

        UnitTrainActiveAction CreateUnitTrainActiveAction(uint cityId, uint structureId, ushort type, ushort count);

        StarvePassiveAction CreateStarvePassiveAction(uint cityId);

        ResourceBuyActiveAction CreateResourceBuyActiveAction(uint cityId, uint structureId, ushort price, ushort quantity, ResourceType resourceType);

        ResourceGatherActiveAction CreateResourceGatherActiveAction(uint cityId, uint objectId);

        EngageDefensePassiveAction CreateEngageDefensePassiveAction(uint cityId, byte stubId);

        EngageAttackPassiveAction CreateEngageAttackPassiveAction(uint cityId, byte stubId, uint targetCityId, AttackMode mode);

        StructureSelfDestroyPassiveAction CreateStructureSelfDestroyPassiveAction(uint cityId, uint objectId);

        StructureChangePassiveAction CreateStructureChangePassiveAction(uint cityId, uint objectId, int seconds, ushort newType, byte newLvl);

        TechnologyUpgradeActiveAction CreateTechnologyUpgradeActiveAction(uint cityId, uint structureId, uint techId);

        CityRadiusChangePassiveAction CreateCityRadiusChangePassiveAction();

        TechnologyCreatePassiveAction CreateTechnologyCreatePassiveAction();

        CityCreatePassiveAction CreateCityCreatePassiveAction(uint cityId, uint x, uint y, string cityName);

        StructureSelfDestroyActiveAction CreateStructureSelfDestroyActiveAction(uint cityId, uint objectId);

        TechnologyDeletePassiveAction CreateTechnologyDeletePassiveAction();

        StructureDowngradePassiveAction CreateStructureDowngradePassiveAction(uint cityId, uint structureId);

        ForestCampBuildActiveAction CreateForestCampBuildActiveAction(uint cityId, uint lumbermillId, uint forestId, ushort campType, byte labors);

        StructureUpgradeActiveAction CreateStructureUpgradeActiveAction(uint cityId, uint structureId);

        PropertyCreatePassiveAction CreatePropertyCreatePassiveAction();

        ObjectRemovePassiveAction CreateObjectRemovePassiveAction(uint cityId, uint objectId, bool wasKilled, List<uint> cancelActions);

        ResourceSellActiveAction CreateResourceSellActiveAction(uint cityId, uint structureId, ushort price, ushort quantity, ResourceType resourceType);

        ForestCampHarvestPassiveAction CreateForestCampHarvestPassiveAction(uint cityId, uint forestId);

        DefenseChainAction CreateDefenseChainAction(uint cityId, byte stubId, uint targetCityId, AttackMode mode);

        TroopMovePassiveAction CreateTroopMovePassiveAction(uint cityId, uint troopObjectId, uint x, uint y, bool isReturningHome, bool isAttacking);

        StructureDowngradeActiveAction CreateStructureDowngradeActiveAction(uint cityId, uint structureId);

        StructureChangeActiveAction CreateStructureChangeActiveAction(uint cityId, uint structureId, uint type, byte lvl);

        LaborMoveActiveAction CreateLaborMoveActiveAction(uint cityId, uint structureId, bool cityToStructure, ushort count);

        UnitUpgradeActiveAction CreateUnitUpgradeActiveAction(uint cityId, uint structureId, ushort type);

        CityPassiveAction CreateCityPassiveAction(uint cityId);

        CityPassiveAction CreateRetreatChainAction(uint cityId, byte stubId);
    }
}

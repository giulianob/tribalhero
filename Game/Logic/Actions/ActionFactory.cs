using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Forest;
using Game.Data.Troop;
using Game.Logic.Actions.ResourceActions;

namespace Game.Logic.Actions
{
    public interface IActionFactory
    {
        ForestCreatorAction CreateForestCreatorAction();

        ForestDepleteAction CreateForestDepleteAction(IForest forest, DateTime time);

        CityAttackChainAction CreateCityAttackChainAction(uint cityId, ITroopObjectInitializer troopObjectInitializer, uint targetCityId, uint targetStructureId);

        StructureBuildActiveAction CreateStructureBuildActiveAction(uint cityId, ushort type, uint x, uint y, byte level);

        ResourceSendActiveAction CreateResourceSendActiveAction(uint cityId,
                                                                uint structureId,
                                                                uint targetCityId,
                                                                Resource resource);

        CityBattlePassiveAction CreateCityBattlePassiveAction(uint cityId);

        UnitTrainActiveAction CreateUnitTrainActiveAction(uint cityId, uint structureId, ushort type, ushort count);

        StarvePassiveAction CreateStarvePassiveAction(uint cityId);

        ResourceBuyActiveAction CreateResourceBuyActiveAction(uint cityId,
                                                              uint structureId,
                                                              ushort price,
                                                              ushort quantity,
                                                              ResourceType resourceType);

        ResourceGatherActiveAction CreateResourceGatherActiveAction(uint cityId, uint objectId);

        CityEngageDefensePassiveAction CreateCityEngageDefensePassiveAction(uint cityId, uint troopObjectId, FormationType formationType);

        CityEngageAttackPassiveAction CreateCityEngageAttackPassiveAction(uint cityId,
                                                                          uint troopObjectId,
                                                                          uint targetCityId);

        StructureSelfDestroyPassiveAction CreateStructureSelfDestroyPassiveAction(uint cityId, uint objectId);

        StructureChangePassiveAction CreateStructureChangePassiveAction(uint cityId,
                                                                        uint objectId,
                                                                        int seconds,
                                                                        ushort newType,
                                                                        byte newLvl);

        TechnologyUpgradeActiveAction CreateTechnologyUpgradeActiveAction(uint cityId, uint structureId, uint techId);

        CityRadiusChangePassiveAction CreateCityRadiusChangePassiveAction();

        TechnologyCreatePassiveAction CreateTechnologyCreatePassiveAction();

        CityCreatePassiveAction CreateCityCreatePassiveAction(uint cityId, uint x, uint y, string cityName);

        StructureSelfDestroyActiveAction CreateStructureSelfDestroyActiveAction(uint cityId, uint objectId);

        StructureDowngradePassiveAction CreateStructureDowngradePassiveAction(uint cityId, uint structureId);

        ForestCampBuildActiveAction CreateForestCampBuildActiveAction(uint cityId,
                                                                      uint lumbermillId,
                                                                      uint forestId,
                                                                      ushort campType,
                                                                      byte labors);

        StructureUpgradeActiveAction CreateStructureUpgradeActiveAction(uint cityId, uint structureId);

        PropertyCreatePassiveAction CreatePropertyCreatePassiveAction();

        ObjectRemovePassiveAction CreateObjectRemovePassiveAction(uint cityId,
                                                                  uint objectId,
                                                                  bool wasKilled,
                                                                  List<uint> cancelActions);

        ResourceSellActiveAction CreateResourceSellActiveAction(uint cityId,
                                                                uint structureId,
                                                                ushort price,
                                                                ushort quantity,
                                                                ResourceType resourceType);

        ForestCampHarvestPassiveAction CreateForestCampHarvestPassiveAction(uint cityId, uint forestId);

        CityDefenseChainAction CreateCityDefenseChainAction(uint cityId,
                                                            ITroopObjectInitializer troopObjectInitializer,
                                                            uint targetCityId);

        StrongholdDefenseChainAction CreateStrongholdDefenseChainAction(uint cityId,
                                                                        ITroopObjectInitializer troopObjectInitializer,
                                                                        uint targetStrongholdId);

        TroopMovePassiveAction CreateTroopMovePassiveAction(uint cityId,
                                                            uint troopObjectId,
                                                            uint x,
                                                            uint y,
                                                            bool isReturningHome,
                                                            bool isAttacking);

        StructureDowngradeActiveAction CreateStructureDowngradeActiveAction(uint cityId, uint structureId);

        StructureChangeActiveAction CreateStructureChangeActiveAction(uint cityId, uint structureId, uint type, byte lvl);

        LaborMoveActiveAction CreateLaborMoveActiveAction(uint cityId,
                                                          uint structureId,
                                                          bool cityToStructure,
                                                          ushort count);

        UnitUpgradeActiveAction CreateUnitUpgradeActiveAction(uint cityId, uint structureId, ushort type);

        CityPassiveAction CreateCityPassiveAction(uint cityId);

        RetreatChainAction CreateRetreatChainAction(uint cityId, ushort stubId);

        TribeContributeActiveAction CreateTribeContributeActiveAction(uint cityId, uint structureId, Resource resource);

        StrongholdEngageGateAttackPassiveAction CreateStrongholdEngageGateAttackPassiveAction(uint cityId,
                                                                                              uint troopObjectId,
                                                                                              uint targetStrongholdId);

        StrongholdEngageMainAttackPassiveAction CreateStrongholdEngageMainAttackPassiveAction(uint cityId,
                                                                                              uint troopObjectId,
                                                                                              uint targetStrongholdId);

        StrongholdGateBattlePassiveAction CreateStrongholdGateBattlePassiveAction(uint strongholdId);

        StrongholdMainBattlePassiveAction CreateStrongholdMainBattlePassiveAction(uint strongholdId);

        StrongholdAttackChainAction CreateStrongholdAttackChainAction(uint cityId,
                                                                      ITroopObjectInitializer troopObjectInitializer,
                                                                      uint targetStrongholdId,
                                                                      bool forceAttack);

        BarbarianTribeEngageAttackPassiveAction CreateBarbarianTribeEngageAttackPassiveAction(uint cityId,
                                                                                              uint troopObjectId,
                                                                                              uint targetObjectId);

        BarbarianTribeAttackChainAction CreateBarbarianTribeAttackChainAction(uint cityId, uint targetObjectId, ITroopObjectInitializer troopObjectInitializer);

        BarbarianTribeBattlePassiveAction CreateBarbarianTribeBattlePassiveAction(uint barbarianTribeId);
    }
}
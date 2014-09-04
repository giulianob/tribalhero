using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Forest;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Actions.ResourceActions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Module;
using Game.Setup;
using Game.Setup.DependencyInjection;
using Game.Util.Locking;
using Persistance;

namespace Game.Logic.Actions
{
    public class ActionFactory : IActionFactory
    {
        private readonly IKernel kernel;

        public ActionFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public ForestCreatorAction CreateForestCreatorAction()
        {
            return new ForestCreatorAction(kernel.Get<IDbManager>(), kernel.Get<IForestManager>(), kernel.Get<IScheduler>());
        }

        public ForestDepleteAction CreateForestDepleteAction(IForest forest, DateTime time)
        {
            return new ForestDepleteAction(forest, time, kernel.Get<IForestManager>(), kernel.Get<IRegionManager>(), kernel.Get<ILocker>());
        }

        public CityAttackChainAction CreateCityAttackChainAction(uint cityId, ITroopObjectInitializer troopObjectInitializer, uint targetCityId, Position target)
        {
            return new CityAttackChainAction(cityId, troopObjectInitializer, targetCityId, target, kernel.Get<IActionFactory>(), kernel.Get<Procedure>(), kernel.Get<ILocker>(), kernel.Get<IGameObjectLocator>(), kernel.Get<CityBattleProcedure>(), kernel.Get<BattleProcedure>(), kernel.Get<Formula>());
        }

        public StructureBuildActiveAction CreateStructureBuildActiveAction(uint cityId, ushort type, uint x, uint y, byte level)
        {
            return new StructureBuildActiveAction(cityId,
                                                  type,
                                                  x,
                                                  y,
                                                  level,
                                                  kernel.Get<IObjectTypeFactory>(),
                                                  kernel.Get<IWorld>(),
                                                  kernel.Get<Formula>(),
                                                  kernel.Get<IRequirementCsvFactory>(),
                                                  kernel.Get<IStructureCsvFactory>(),
                                                  kernel.Get<ILocker>(),
                                                  kernel.Get<Procedure>(),
                                                  kernel.Get<IRoadPathFinder>(),
                                                  kernel.Get<ITileLocator>(),
                                                  kernel.Get<CallbackProcedure>(),
                                                  kernel.Get<InstantProcedure>());
        }

        public ResourceSendActiveAction CreateResourceSendActiveAction(uint cityId, uint structureId, uint targetCityId, Resource resource)
        {
            return new ResourceSendActiveAction(cityId, structureId, targetCityId, resource, kernel.Get<ITileLocator>(), kernel.Get<IWorld>(), kernel.Get<Formula>(), kernel.Get<ILocker>());
        }

        public CityBattlePassiveAction CreateCityBattlePassiveAction(uint cityId)
        {
            return new CityBattlePassiveAction(cityId, kernel.Get<IActionFactory>(), kernel.Get<BattleProcedure>(), kernel.Get<ILocker>(), kernel.Get<IGameObjectLocator>(), kernel.Get<IDbManager>(), kernel.Get<Formula>(), kernel.Get<CityBattleProcedure>(), kernel.Get<IWorld>(), kernel.Get<ITroopObjectInitializerFactory>());
        }

        public UnitTrainActiveAction CreateUnitTrainActiveAction(uint cityId, uint structureId, ushort type, ushort count)
        {
            return new UnitTrainActiveAction(cityId, structureId, type, count, kernel.Get<UnitFactory>(), kernel.Get<ILocker>(), kernel.Get<IWorld>(), kernel.Get<Formula>());
        }

        public StarvePassiveAction CreateStarvePassiveAction(uint cityId)
        {
            return new StarvePassiveAction(cityId, kernel.Get<IGameObjectLocator>(), kernel.Get<ILocker>());
        }

        public ResourceBuyActiveAction CreateResourceBuyActiveAction(uint cityId, uint structureId, ushort price, ushort quantity, ResourceType resourceType)
        {
            return new ResourceBuyActiveAction(cityId, structureId, price, quantity, resourceType, kernel.Get<ILocker>(), kernel.Get<IWorld>(), kernel.Get<Formula>());
        }

        public ResourceGatherActiveAction CreateResourceGatherActiveAction(uint cityId, uint objectId)
        {
            return new ResourceGatherActiveAction(cityId, objectId, kernel.Get<ILocker>(), kernel.Get<IObjectTypeFactory>(), kernel.Get<IWorld>(), kernel.Get<IActionFactory>());
        }

        public ResourceWithdrawActiveAction CreateResourceWithdrawActiveAction(uint cityId, uint objectId, Resource resource)
        {
            return new ResourceWithdrawActiveAction(cityId,
                                                    objectId,
                                                    resource,
                                                    kernel.Get<ILocker>(),
                                                    kernel.Get<IObjectTypeFactory>(),
                                                    kernel.Get<IWorld>(),
                                                    kernel.Get<IActionFactory>());
        }

        public CityEngageDefensePassiveAction CreateCityEngageDefensePassiveAction(uint cityId, uint troopObjectId, FormationType formationType)
        {
            return new CityEngageDefensePassiveAction(cityId, troopObjectId, formationType, kernel.Get<BattleProcedure>(), kernel.Get<CityBattleProcedure>(), kernel.Get<IGameObjectLocator>());
        }

        public CityEngageAttackPassiveAction CreateCityEngageAttackPassiveAction(uint cityId, uint troopObjectId, uint targetCityId)
        {
            return new CityEngageAttackPassiveAction(cityId, troopObjectId, targetCityId, kernel.Get<IBattleFormulas>(), kernel.Get<IGameObjectLocator>(), kernel.Get<CityBattleProcedure>(), kernel.Get<IStructureCsvFactory>(), kernel.Get<IDbManager>(), kernel.Get<IStaminaMonitorFactory>());
        }

        public StructureSelfDestroyPassiveAction CreateStructureSelfDestroyPassiveAction(uint cityId, uint objectId)
        {
            return new StructureSelfDestroyPassiveAction(cityId, objectId, kernel.Get<IWorld>(), kernel.Get<ILocker>());
        }

        public StructureChangePassiveAction CreateStructureChangePassiveAction(uint cityId, uint objectId, int seconds, ushort newType, byte newLvl)
        {
            return new StructureChangePassiveAction(cityId, objectId, seconds, newType, newLvl, kernel.Get<Formula>(), kernel.Get<IWorld>(), kernel.Get<ILocker>(), kernel.Get<Procedure>(), kernel.Get<CallbackProcedure>(), kernel.Get<IStructureCsvFactory>());
        }

        public TechnologyUpgradeActiveAction CreateTechnologyUpgradeActiveAction(uint cityId, uint structureId, uint techId)
        {
            return new TechnologyUpgradeActiveAction(cityId, structureId, techId, kernel.Get<IWorld>(), kernel.Get<Formula>(), kernel.Get<ILocker>(), kernel.Get<TechnologyFactory>(), kernel.Get<CallbackProcedure>(),kernel.Get<InstantProcedure>());
        }

        public CityRadiusChangePassiveAction CreateCityRadiusChangePassiveAction()
        {
            return new CityRadiusChangePassiveAction();
        }

        public TechnologyCreatePassiveAction CreateTechnologyCreatePassiveAction()
        {
            return new TechnologyCreatePassiveAction(kernel.Get<TechnologyFactory>(), kernel.Get<CallbackProcedure>());
        }

        public CityCreatePassiveAction CreateCityCreatePassiveAction(uint cityId, uint x, uint y, string cityName)
        {
            return new CityCreatePassiveAction(cityId, x, y, cityName, kernel.Get<IActionFactory>(), kernel.Get<ICityRemoverFactory>(), kernel.Get<Formula>(), kernel.Get<IWorld>(), kernel.Get<ILocker>(), kernel.Get<IObjectTypeFactory>(), kernel.Get<IStructureCsvFactory>(), kernel.Get<ICityFactory>(), kernel.Get<CityProcedure>(), kernel.Get<IBarbarianTribeManager>(), kernel.Get<CallbackProcedure>());
        }

        public CityMovePassiveAction CreateCityMovePassiveAction(uint cityId, Resource resource, int structureUpgrades, int technologyUpgrades)
        {
            return new CityMovePassiveAction(cityId,
                                                resource,
                                                structureUpgrades,
                                                technologyUpgrades,
                                                kernel.Get<IActionFactory>(),
                                                kernel.Get<ILocker>(),
                                                kernel.Get<CallbackProcedure>(),
                                                kernel.Get<IStructureCsvFactory>(),
                                                kernel.Get<CityProcedure>(),
                                                kernel.Get<IWorld>(),
                                                kernel.Get<TechnologyFactory>(),
                                                kernel.Get<ITileLocator>(),
                                                kernel.Get<IObjectTypeFactory>(),
                                                kernel.Get<IDbManager>());
        }

        public StructureSelfDestroyActiveAction CreateStructureSelfDestroyActiveAction(uint cityId, uint objectId)
        {
            return new StructureSelfDestroyActiveAction(cityId, objectId, kernel.Get<ILocker>(), kernel.Get<IWorld>());
        }

        public StructureDowngradePassiveAction CreateStructureDowngradePassiveAction(uint cityId, uint structureId)
        {
            return new StructureDowngradePassiveAction(cityId, structureId, kernel.Get<ILocker>(), kernel.Get<IStructureCsvFactory>(), kernel.Get<Procedure>(), kernel.Get<CallbackProcedure>());
        }

        public ForestCampBuildActiveAction CreateForestCampBuildActiveAction(uint cityId, uint lumbermillId, uint forestId, ushort campType, ushort labors)
        {
            return new ForestCampBuildActiveAction(cityId, lumbermillId, forestId, campType, labors, kernel.Get<Formula>(), kernel.Get<IWorld>(), kernel.Get<IObjectTypeFactory>(), kernel.Get<IStructureCsvFactory>(), kernel.Get<IForestManager>(), kernel.Get<ILocker>(), kernel.Get<ITileLocator>(), kernel.Get<CallbackProcedure>());
        }

        public StructureUpgradeActiveAction CreateStructureUpgradeActiveAction(uint cityId, uint structureId)
        {
            return new StructureUpgradeActiveAction(cityId, structureId, kernel.Get<IStructureCsvFactory>(), kernel.Get<Formula>(), kernel.Get<IWorld>(), kernel.Get<Procedure>(), kernel.Get<ILocker>(), kernel.Get<IRequirementCsvFactory>(), kernel.Get<IObjectTypeFactory>(), kernel.Get<CallbackProcedure>(), kernel.Get<InstantProcedure>());
        }

        public PropertyCreatePassiveAction CreatePropertyCreatePassiveAction()
        {
            return new PropertyCreatePassiveAction(kernel.Get<ILocker>());
        }

        public ObjectRemovePassiveAction CreateObjectRemovePassiveAction(uint cityId, uint objectId, bool wasKilled, List<uint> cancelActions)
        {
            return new ObjectRemovePassiveAction(cityId, objectId, wasKilled, cancelActions, kernel.Get<IGameObjectLocator>(), kernel.Get<ILocker>(), kernel.Get<Procedure>(), kernel.Get<CallbackProcedure>(), kernel.Get<IDbManager>());
        }

        public ResourceSellActiveAction CreateResourceSellActiveAction(uint cityId, uint structureId, ushort price, ushort quantity, ResourceType resourceType)
        {
            return new ResourceSellActiveAction(cityId, structureId, price, quantity, resourceType, kernel.Get<ILocker>(), kernel.Get<IWorld>(), kernel.Get<Formula>());
        }

        public ForestCampHarvestPassiveAction CreateForestCampHarvestPassiveAction(uint cityId, uint forestId)
        {
            return new ForestCampHarvestPassiveAction(cityId, forestId, kernel.Get<IScheduler>(), kernel.Get<IWorld>(), kernel.Get<IForestManager>(), kernel.Get<ILocker>());
        }

        public CityDefenseChainAction CreateCityDefenseChainAction(uint cityId, ITroopObjectInitializer troopObjectInitializer, uint targetCityId)
        {
            return new CityDefenseChainAction(cityId, troopObjectInitializer, targetCityId, kernel.Get<BattleProcedure>(), kernel.Get<IActionFactory>(), kernel.Get<ILocker>(), kernel.Get<IWorld>(), kernel.Get<Procedure>());
        }

        public StrongholdDefenseChainAction CreateStrongholdDefenseChainAction(uint cityId, ITroopObjectInitializer troopObjectInitializer, uint targetStrongholdId)
        {
            return new StrongholdDefenseChainAction(cityId, troopObjectInitializer, targetStrongholdId, kernel.Get<IActionFactory>(), kernel.Get<Procedure>(), kernel.Get<ILocker>(), kernel.Get<IGameObjectLocator>(), kernel.Get<BattleProcedure>(), kernel.Get<StrongholdBattleProcedure>());
        }

        public TroopMovePassiveAction CreateTroopMovePassiveAction(uint cityId, uint troopObjectId, uint x, uint y, bool isReturningHome, bool isAttacking)
        {
            return new TroopMovePassiveAction(cityId, troopObjectId, x, y, isReturningHome, isAttacking, kernel.Get<Formula>(), kernel.Get<ITileLocator>(), kernel.Get<IGameObjectLocator>(), kernel.Get<ILocker>());
        }

        public StructureDowngradeActiveAction CreateStructureDowngradeActiveAction(uint cityId, uint structureId)
        {
            return new StructureDowngradeActiveAction(cityId, structureId, kernel.Get<IObjectTypeFactory>(), kernel.Get<IStructureCsvFactory>(), kernel.Get<IWorld>(), kernel.Get<ILocker>(), kernel.Get<Formula>());
        }

        public StructureChangeActiveAction CreateStructureChangeActiveAction(uint cityId, uint structureId, uint type, byte lvl)
        {
            return new StructureChangeActiveAction(cityId, structureId, type, lvl, kernel.Get<IStructureCsvFactory>(), kernel.Get<Formula>(), kernel.Get<IWorld>(), kernel.Get<Procedure>(), kernel.Get<ILocker>(), kernel.Get<CallbackProcedure>());
        }

        public LaborMoveActiveAction CreateLaborMoveActiveAction(uint cityId, uint structureId, bool cityToStructure, ushort count)
        {
            return new LaborMoveActiveAction(cityId, structureId, cityToStructure, count, kernel.Get<Formula>(), kernel.Get<Procedure>(), kernel.Get<ILocker>(), kernel.Get<IGameObjectLocator>());
        }

        public UnitUpgradeActiveAction CreateUnitUpgradeActiveAction(uint cityId, uint structureId, ushort type)
        {
            return new UnitUpgradeActiveAction(cityId, structureId, type, kernel.Get<ILocker>(), kernel.Get<IWorld>(), kernel.Get<Formula>(), kernel.Get<UnitFactory>());
        }

        public CityPassiveAction CreateCityPassiveAction(uint cityId)
        {
            return new CityPassiveAction(cityId, kernel.Get<IObjectTypeFactory>(), kernel.Get<ILocker>(), kernel.Get<Formula>(), kernel.Get<IActionFactory>(), kernel.Get<Procedure>(), kernel.Get<IGameObjectLocator>(), kernel.Get<IBattleFormulas>());
        }

        public RetreatChainAction CreateRetreatChainAction(uint cityId, ITroopObjectInitializer troopObjectInitializer)
        {
            return new RetreatChainAction(cityId, troopObjectInitializer, kernel.Get<IActionFactory>(), kernel.Get<IWorld>(), kernel.Get<Procedure>(), kernel.Get<ILocker>());
        }

        public TribeContributeActiveAction CreateTribeContributeActiveAction(uint cityId, uint structureId, Resource resource)
        {
            return new TribeContributeActiveAction(cityId, structureId, resource, kernel.Get<IGameObjectLocator>(), kernel.Get<Formula>(), kernel.Get<ILocker>());
        }

        public StrongholdEngageGateAttackPassiveAction CreateStrongholdEngageGateAttackPassiveAction(uint cityId, uint troopObjectId, uint targetStrongholdId)
        {
            return new StrongholdEngageGateAttackPassiveAction(cityId, troopObjectId, targetStrongholdId, kernel.Get<IBattleFormulas>(), kernel.Get<IGameObjectLocator>(), kernel.Get<StrongholdBattleProcedure>(), kernel.Get<IDbManager>(), kernel.Get<IStaminaMonitorFactory>());
        }

        public StrongholdEngageMainAttackPassiveAction CreateStrongholdEngageMainAttackPassiveAction(uint cityId, uint troopObjectId, uint targetStrongholdId)
        {
            return new StrongholdEngageMainAttackPassiveAction(cityId, troopObjectId, targetStrongholdId, kernel.Get<IGameObjectLocator>(), kernel.Get<StrongholdBattleProcedure>());
        }

        public StrongholdGateBattlePassiveAction CreateStrongholdGateBattlePassiveAction(uint strongholdId)
        {
            return new StrongholdGateBattlePassiveAction(strongholdId, kernel.Get<StrongholdBattleProcedure>(), kernel.Get<ILocker>(), kernel.Get<IGameObjectLocator>(), kernel.Get<IDbManager>(), kernel.Get<Formula>(), kernel.Get<IWorld>());
        }

        public StrongholdMainBattlePassiveAction CreateStrongholdMainBattlePassiveAction(uint strongholdId)
        {
            return new StrongholdMainBattlePassiveAction(strongholdId, kernel.Get<BattleProcedure>(), kernel.Get<StrongholdBattleProcedure>(), kernel.Get<ILocker>(), kernel.Get<IGameObjectLocator>(), kernel.Get<IDbManager>(), kernel.Get<Formula>(), kernel.Get<IWorld>(), kernel.Get<IStrongholdManager>(), kernel.Get<IActionFactory>(), kernel.Get<ITroopObjectInitializerFactory>());
        }

        public StrongholdAttackChainAction CreateStrongholdAttackChainAction(uint cityId, ITroopObjectInitializer troopObjectInitializer, uint targetStrongholdId, bool forceAttack)
        {
            return new StrongholdAttackChainAction(cityId, troopObjectInitializer, targetStrongholdId, forceAttack, kernel.Get<IActionFactory>(), kernel.Get<Procedure>(), kernel.Get<ILocker>(), kernel.Get<IGameObjectLocator>(), kernel.Get<BattleProcedure>(), kernel.Get<StrongholdBattleProcedure>());
        }

        public BarbarianTribeEngageAttackPassiveAction CreateBarbarianTribeEngageAttackPassiveAction(uint cityId, uint troopObjectId, uint targetObjectId)
        {
            return new BarbarianTribeEngageAttackPassiveAction(cityId, troopObjectId, targetObjectId, kernel.Get<IBattleFormulas>(), kernel.Get<IGameObjectLocator>(), kernel.Get<BarbarianTribeBattleProcedure>(), kernel.Get<Formula>(), kernel.Get<IDbManager>(), kernel.Get<IStaminaMonitorFactory>());
        }

        public BarbarianTribeAttackChainAction CreateBarbarianTribeAttackChainAction(uint cityId, uint targetObjectId, ITroopObjectInitializer troopObjectInitializer)
        {
            return new BarbarianTribeAttackChainAction(cityId, targetObjectId, troopObjectInitializer, kernel.Get<IActionFactory>(), kernel.Get<Procedure>(), kernel.Get<ILocker>(), kernel.Get<IGameObjectLocator>(), kernel.Get<BattleProcedure>());
        }

        public BarbarianTribeBattlePassiveAction CreateBarbarianTribeBattlePassiveAction(uint barbarianTribeId)
        {
            return new BarbarianTribeBattlePassiveAction(barbarianTribeId, kernel.Get<ILocker>(), kernel.Get<IGameObjectLocator>(), kernel.Get<IDbManager>(), kernel.Get<Formula>(), kernel.Get<BarbarianTribeBattleProcedure>(), kernel.Get<IWorld>(), kernel.Get<ISimpleStubGeneratorFactory>());
        }
    }
}
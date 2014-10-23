
package src.Objects.Actions {
    import src.Objects.Effects.EffectReqManager;
    import src.Objects.GameObject;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ForestInfo.Buttons.*;
    import src.UI.Sidebars.ObjectInfo.Buttons.*;
    import src.UI.Sidebars.TroopInfo.Buttons.*;

    /**
	* ...
	* @author Default
	*/
	public class Action {
		
		public static const groups: Object = [
			{ order: 1, name: "Build and Upgrade", actions: [ StructureUpgradeButton, BuildButton ] },
			{ order: 2, name: "Train Units", actions: [ TrainButton] },
			{ order: 3, name: "Upgrade Units", actions: [ UnitUpgradeButton ] },
			{ order: 4, name: "Convert Structure", actions: [ StructureChangeButton ] },
			{ order: 5, name: "Research", actions: [ TechnologyButton ] },
			{ order: 6, name: "Other", actions: [ DefaultActionButton, ForestCampBuildButton, LaborMoveButton, BuildRoadButton, DestroyRoadButton, MarketButton, ViewDestinationButton, ViewBattleButton, ViewUnitsButton, ForestCampRemoveButton, StructureUserDowngradeButton, SendResourcesButton, StructureSelfDestroyButton, ResourceGatherButton, TribeContributeButton, SendAttackButton, SendReinforcementButton] },
		];
		
		public static const CONCURRENCY_STANDALONE: int = 1;
		public static const CONCURRENCY_NORMAL: int = 2;
		public static const CONCURRENCY_CONCURRENT: int = 3;
					
		public static const actionConcurrency: Object = {
			101: CONCURRENCY_NORMAL, // STRUCTURE BUILD
			102: CONCURRENCY_STANDALONE, // STRUCTURE UPGRADE
			103: CONCURRENCY_STANDALONE, // STRUCTURE CHANGE
			106: CONCURRENCY_CONCURRENT, // LABOR MOVE
			107: CONCURRENCY_NORMAL, // STRUCTURE DOWNGRADE
			109: CONCURRENCY_STANDALONE, // STRUCTURE_SELF_DESTROY
			305: CONCURRENCY_NORMAL, // RESOURCE SEND
			306: CONCURRENCY_NORMAL, // RESOURCE BUY
			307: CONCURRENCY_NORMAL, // RESOURCE SELL
			308: CONCURRENCY_CONCURRENT, // FOREST CAMP BUILD
			309: CONCURRENCY_NORMAL, // FOREST CAMP REMOVE
			311: CONCURRENCY_CONCURRENT, // RESOURCE GATHER
			402: CONCURRENCY_NORMAL, // TECH UPGRADE
			510: CONCURRENCY_CONCURRENT, // ROAD BUILD
			511: CONCURRENCY_CONCURRENT, // ROAD DESTROY
			601: CONCURRENCY_CONCURRENT, // UNIT TRAIN
			602: CONCURRENCY_NORMAL, // UNIT UPGRADE
			801: CONCURRENCY_CONCURRENT, // DEFAULT ACTION
			1018: CONCURRENCY_CONCURRENT// TRIBE CONTRIBUTE
		};
		
		public static const CATEGORY_UNSPECIFIED: int = 0;
		public static const CATEGORY_ATTACK: int = 1;
		public static const CATEGORY_DEFENSE: int = 2;
		
		public static const actionCategory: Object = {
			250: CATEGORY_ATTACK,
			251: CATEGORY_DEFENSE,
            252: CATEGORY_DEFENSE,
			253: CATEGORY_ATTACK,
			254: CATEGORY_DEFENSE,			
            710: CATEGORY_ATTACK
		};
		
		public static const costsToCancelActions: Array = [STRUCTURE_BUILD, STRUCTURE_UPGRADE, STRUCTURE_CHANGE, TECHNOLOGY_UPGRADE, UNIT_TRAIN, UNIT_UPGRADE, RESOURCE_SEND, RESOURCE_BUY, RESOURCE_SELL, TRIBE_CONTRIBUTE];
		
		public static const OPTION_UNCANCELABLE: int = 1;
		
		public static const STATE_COMPLETED: int = 0;
		public static const STATE_STARTED: int = 1;
		public static const STATE_FAILED: int = 2;
		public static const STATE_FIRED: int = 3;
		public static const STATE_RESCHEDULED: int = 4;
		
		//the actions visible to the client need to be here
        public static const STRUCTURE_BUILD: int = 101;
        public static const STRUCTURE_UPGRADE: int = 102;
        public static const STRUCTURE_CHANGE: int = 103;
        public static const LABOR_MOVE: int = 106;        
		public static const STRUCTURE_USERDOWNGRADE: int = 107;
		public static const STRUCTURE_SELF_DESTROY: int = 109;
		public static const RESOURCE_SEND: int = 305;
        public static const RESOURCE_BUY: int = 306;
        public static const RESOURCE_SELL: int = 307;
		public static const FOREST_CAMP_BUILD: int = 308;
		public static const FOREST_CAMP_REMOVE: int = 309;
		public static const RESOURCES_GATHER: int = 311;
        public static const TECHNOLOGY_UPGRADE: int = 402;
		public static const ROAD_BUILD: int = 510;
		public static const ROAD_DESTROY: int = 511;
        public static const UNIT_TRAIN: int = 601;
        public static const UNIT_UPGRADE: int = 602;
		public static const BARBARIAN_TRIBE_ATTACK_CHAIN: int = 710;
		public static const DEFAULT_ACTION: int = 801;
		public static const TRIBE_CONTRIBUTE: int = 1018;
		
		public var actionType: int;
		public var effectReq: EffectReqManager;
		public var effectReqInherit: int;
		public var index: int;
		public var options: int;
		
		public function Action(actionType: int = 0) 
		{
			this.actionType = actionType;			
		}
		
		public function getMissingRequirements(parentObj: SimpleGameObject, effects: Array): Array
		{
			if (!(parentObj is GameObject)) return [];
			if (effectReq == null) return [];
			
			return effectReq.validate(parentObj as GameObject, effects);
		}
	}
	
}
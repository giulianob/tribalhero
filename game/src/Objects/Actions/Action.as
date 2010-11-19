
package src.Objects.Actions {
	import src.Objects.Effects.EffectReqManager;
	import src.Objects.GameObject;
	import src.UI.Sidebars.ObjectInfo.Buttons.*;
	import src.UI.Sidebars.ForestInfo.Buttons.*;
	import src.UI.Sidebars.TroopInfo.Buttons.*;
	
	/**
	* ...
	* @author Default
	*/
	public class Action {
		
		public static const groups: Object = [
			{ name: "Build", icon: ICON_HAMMER, actions: [ BuildButton ] },
			{ name: "Train", icon: ICON_SINGLE_SWORD, actions: [ TrainButton] },
			{ name: "Upgrade", icon: ICON_UPGRADE, actions: [ StructureUpgradeButton, StructureChangeButton, UnitUpgradeButton ] },				
			{ name: "Research", icon: ICON_BOOK, actions: [ TechnologyButton ] },			
			{ name: "Other", icon: ICON_QUESTION, actions: [ ForestCampBuildButton, LaborMoveButton, BuildRoadButton, DestroyRoadButton, MarketButton, ViewDestinationButton, ViewBattleButton, ViewUnitsButton, ForestCampRemoveButton, StructureUserDowngradeButton, SendResourcesButton ] },
		];
		
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
		public static const RESOURCE_SEND: int = 305;
        public static const RESOURCE_BUY: int = 306;
        public static const RESOURCE_SELL: int = 307;
		public static const FOREST_CAMP_BUILD: int = 308;
		public static const FOREST_CAMP_REMOVE: int = 309;
        public static const TECHNOLOGY_UPGRADE: int = 402;
		public static const ROAD_BUILD: int = 510;
		public static const ROAD_DESTROY: int = 511;
        public static const UNIT_TRAIN: int = 601;
        public static const UNIT_UPGRADE: int = 602;
        public static const BATTLE: int = 701;
		
		public var actionType: int;
		public var effectReq: EffectReqManager;
		public var effectReqInherit: int;
		public var index: int;
		public var maxCount: int;
		public var options: int;
		
		public function Action(actionType: int = 0) 
		{
			this.actionType = actionType;			
		}
		
		public function validate(parentObj: GameObject, effects: Array): Array
		{
			if (effectReq == null) return new Array();
				
			return effectReq.validate(parentObj, effects);
		}
	}
	
}
package src.Objects.Actions {
	import src.Objects.Effects.EffectReqManager;
	import src.Objects.GameObject;
	import src.UI.Sidebars.ObjectInfo.Buttons.*;
	import src.UI.Sidebars.TroopInfo.Buttons.ViewUnitsButton;
	
	/**
	* ...
	* @author Default
	*/
	public class Action {
		
		public static const groups: Object = [
			{ name: "Buildings", actions: [ BuildButton ] },
			{ name: "Units", actions: [ TrainButton] },
			{ name: "Upgrades", actions: [ StructureUpgradeButton, StructureChangeButton, UnitUpgradeButton ] },				
			{ name: "Technologies", actions: [ TechnologyButton ] },			
			{ name: "Others", actions: [ LaborMoveButton, MarketButton, ViewBattleButton, ViewUnitsButton ] },
		];
		
		//the actions visible to the client need to be here
        public static const STRUCTURE_BUILD: int = 101;
        public static const STRUCTURE_UPGRADE: int = 102;
        public static const STRUCTURE_CHANGE: int = 103;
        public static const LABOR_MOVE: int = 105;        
        public static const RESOURCE_BUY: int = 306;
        public static const RESOURCE_SELL: int = 307;
        public static const TECHNOLOGY_UPGRADE: int = 402;
        public static const UNIT_TRAIN: int = 601;
        public static const UNIT_UPGRADE: int = 602;
        public static const BATTLE: int = 701;
		
		public var actionType: int;
		public var effectReq: EffectReqManager;
		public var effectReqInherit: int;
		public var index: int;
		public var maxCount: int;
		
		public function Action(actionType: int) 
		{
			this.actionType = actionType;
		}
		
		public function validate(parentObj: GameObject, effects: Array): Array
		{
			if (effectReq == null)
				return null;
				
			return effectReq.validate(parentObj, effects);
		}
	}
	
}
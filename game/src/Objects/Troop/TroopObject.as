package src.Objects.Troop {

	import src.Objects.Factories.TroopFactory;
	import src.Objects.*;

	public class TroopObject extends GameObject {

		public var speed: int;
		public var attackRadius: int;
		public var stubId: int;
		public var targetX: int;
		public var targetY: int;
		public var troop: TroopStub;

		public var template: UnitTemplateManager = new UnitTemplateManager();
		
		private var radiusManager: RadiusManager;

		public function TroopObject(type: int, objX: int, objY: int, playerId: int, cityId: int, objectId: int) {
			super(type, objX, objY, playerId, cityId, objectId);
			
			radiusManager = new RadiusManager(this);			
		}

		public function getNiceStubId(includeParenthesis: Boolean = false) : String {
			if (stubId == 1) {
				if (includeParenthesis) return "(Local Troop)";
				else return "Local Troop";
			}
			else return (includeParenthesis?"(":"") + stubId.toString()  + (includeParenthesis?")":"");
		}
		
		override public function dispose():void 
		{
			super.dispose();
			
			radiusManager.hideRadius();
		}
		
		override public function setSelected(bool:Boolean = false):void
		{
			super.setSelected(bool);

			if (bool) 
				radiusManager.showRadius(attackRadius);
			else 
				radiusManager.hideRadius();
		}

		public function ToSprite(): Object
		{
			return TroopFactory.getSprite();
		}
	}

}

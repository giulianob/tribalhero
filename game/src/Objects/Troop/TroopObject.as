package src.Objects.Troop {

	import src.Objects.Factories.TroopFactory;
	import src.Objects.*;
	import src.Objects.States.GameObjectState;

	public class TroopObject extends GameObject {

		public var speed: Number;
		public var attackRadius: int;
		public var stubId: int;
		public var targetX: int;
		public var targetY: int;
		public var troop: TroopStub;

		public var template: UnitTemplateManager = new UnitTemplateManager();
		
		private var radiusManager: RadiusManager;

		public function TroopObject(type: int, state: GameObjectState, objX: int, objY: int, playerId: int, cityId: int, objectId: int) {
			super(type, state, objX, objY, playerId, cityId, objectId);
			
			radiusManager = new RadiusManager(this);			
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

    }

}

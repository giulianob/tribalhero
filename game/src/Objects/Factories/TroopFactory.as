package src.Objects.Factories {

	import flash.display.DisplayObjectContainer;
	import flash.utils.getDefinitionByName;
	import src.Global;
	import src.Objects.States.GameObjectState;
	import src.Objects.Troop.TroopObject;
	import src.Objects.Troop.TroopStub;

	/**
	 * ...
	 * @author Default
	 */
	public class TroopFactory {

		public function TroopFactory() {
		}

		public static function getStateSprite(state: int, size: int = 1): DisplayObjectContainer
		{
			var name: String = "";
			switch (state) {
				case TroopStub.BATTLE:
					name = "TROOP_ATTACK";
				break;
				case TroopStub.BATTLE_STATIONED:
					name = "TROOP_DEFENSE";
				break;
				default:
					name = "TROOP_IDLE";
				break;
			}

			var objRef: Class = getDefinitionByName(name + "_" + size) as Class;

			var sprite: DisplayObjectContainer = new objRef() as DisplayObjectContainer;

			return sprite;
		}

		public static function getSprite(centered: Boolean = false): DisplayObjectContainer
		{
			var objRef: Class = getDefinitionByName("DEFAULT_TROOP") as Class;

			var sprite: DisplayObjectContainer = new objRef() as DisplayObjectContainer;

			if (centered)
			{
				// Troop objs only have 1 child
				sprite.getChildAt(0).x = 0;
				sprite.getChildAt(0).y = 0;
			}

			return sprite;
		}

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, playerId: int, cityId: int, objectId: int): TroopObject
		{
			var troopObject: TroopObject = new TroopObject(type, state, objX, objY, playerId, cityId, objectId);
			
			troopObject.spriteContainer.addChild(ObjectFactory.makeIntoShadow(getSprite()));
			troopObject.spriteContainer.addChild(getSprite());
			
			troopObject.setOnSelect(Global.map.selectObject);
			
			return troopObject;
		}
	}
}


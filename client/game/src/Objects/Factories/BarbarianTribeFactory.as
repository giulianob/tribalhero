package src.Objects.Factories {

    import src.Global;
    import src.Objects.BarbarianTribe;
    import src.Objects.SimpleGameObject;
    import src.Objects.States.GameObjectState;

    public class BarbarianTribeFactory {

		public function BarbarianTribeFactory() {
		}

        public static function getSpriteName(): String {
            return "DEFAULT_BARBARIAN_TRIBE_STRUCTURE";
        }

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, groupId: int, objectId: int, level: int, count: int): SimpleGameObject
		{
            var typeName: String = getSpriteName();
			var obj: BarbarianTribe = new BarbarianTribe(type, state, objX, objY, size, groupId, objectId, level, count);
            obj.setSprite(SpriteFactory.getStarlingImage(typeName), SpriteFactory.getMapPosition(typeName));
			obj.setOnSelect(Global.map.selectObject);
			
			return obj;
		}
	}
}


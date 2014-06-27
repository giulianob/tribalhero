package src.Objects.Factories {

    import src.Global;
    import src.Objects.Forest;
    import src.Objects.States.GameObjectState;

    public class ForestFactory {

		public function ForestFactory() {
		}

        public static function getSpriteName(): String {
            return "FOREST_LVL_4";
        }

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, groupId: int, objectId: int): Forest
		{
            var typeName: String = getSpriteName();

			var forestObj: Forest = new Forest(type, state, objX, objY, size, groupId, objectId);
            forestObj.setSprite(SpriteFactory.getStarlingImage(typeName), SpriteFactory.getMapPosition(typeName));
			forestObj.setOnSelect(Global.map.selectObject);
			return forestObj;
		}
	}
}


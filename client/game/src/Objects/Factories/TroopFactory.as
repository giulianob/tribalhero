package src.Objects.Factories {

    import flash.geom.Point;

    import src.FlashAssets;

    import src.Global;
    import src.Objects.States.GameObjectState;
    import src.Objects.Theme;
    import src.Objects.Troop.TroopObject;

    import starling.display.*;

    public class TroopFactory {

		public function TroopFactory() {
		}

        public static function getSpriteName(theme: String): String {
            var typeName: String = theme + "_TROOP";

            // Fall back to default theme if current theme does not have gfx
            if (theme != Theme.DEFAULT_THEME_ID && !FlashAssets.doesSpriteExist(typeName)) {
                typeName = getSpriteName("DEFAULT");
            }
	    
	        return typeName;
        }

		public static function getInstance(theme: String, type: int, state: GameObjectState, objX: int, objY: int, size: int, playerId: int, cityId: int, objectId: int): TroopObject
		{
            var typeName: String = getSpriteName(theme);
            var position: Point = SpriteFactory.getMapPosition(typeName);
            var defaultSprite: Image = SpriteFactory.getStarlingImage(typeName);
			var troopObject: TroopObject = new TroopObject(theme, type, state, defaultSprite, position, objX, objY, size, playerId, cityId, objectId);
            troopObject.setSprite(SpriteFactory.getStarlingImage(typeName), position);
			troopObject.setOnSelect(Global.map.selectObject);
			
			return troopObject;
		}
	}
}


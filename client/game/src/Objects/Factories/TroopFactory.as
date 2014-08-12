package src.Objects.Factories {

    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;
    import flash.geom.Point;

    import src.Assets;
    import src.Global;
    import src.Objects.States.GameObjectState;
    import src.Objects.Theme;
    import src.Objects.Troop.TroopObject;

    public class TroopFactory {

		public function TroopFactory() {
		}

        public static function getSprite(theme: String, withPosition: String = ""): DisplayObjectContainer
		{
            var typeName: String = getSpriteName(theme);
            // Fall back to default theme if current theme does not have gfx
            if (theme != Theme.DEFAULT_THEME_ID && !Assets.doesSpriteExist(typeName)) {
                typeName = getSpriteName("DEFAULT");
            }

            var image: DisplayObject = Assets.getInstance(typeName, withPosition);

            var sprite: Sprite = new Sprite();

            sprite.addChild(image);

            return sprite;
		}

        public static function getSpriteName(theme: String): String {
            return theme + "_TROOP";
        }

		public static function getInstance(theme: String, type: int, state: GameObjectState, objX: int, objY: int, size: int, playerId: int, cityId: int, objectId: int): TroopObject
		{
            var defaultSprite: DisplayObjectContainer = getSprite(theme, "map");
            var defaultPosition: Point = Assets.getPosition(getSpriteName(theme), "map");
			var troopObject: TroopObject = new TroopObject(theme, type, state, defaultSprite, defaultPosition, objX, objY, size, playerId, cityId, objectId);

			troopObject.setOnSelect(Global.map.selectObject);
			
			return troopObject;
		}
	}
}


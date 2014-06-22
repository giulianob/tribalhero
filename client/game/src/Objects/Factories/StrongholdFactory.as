package src.Objects.Factories {

    import flash.display.Bitmap;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;

    import src.FlashAssets;
    import src.Constants;
    import src.Global;
    import src.Objects.States.GameObjectState;
    import src.Objects.Stronghold.Stronghold;

    public class StrongholdFactory {

		public function StrongholdFactory() {
		}

        public static function getSpriteName(theme: String): String {
            return theme.toUpperCase() + "_STRONGHOLD_STRUCTURE";
        }

		public static function getSprite(theme: String, withPosition: String = ""): DisplayObjectContainer
		{
			var image: Bitmap = FlashAssets.getInstance(getSpriteName(theme), withPosition);

            var sprite: Sprite = new Sprite();

            sprite.addChild(image);

			return sprite;
		}

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, groupId: int, objectId: int, level: int, tribeId: int, gateMax: int, themeId: String): Stronghold
		{
			var strongholdObj: Stronghold = new Stronghold(type, state, objX, objY, size, groupId, objectId, level, tribeId, gateMax, themeId);

			strongholdObj.setSprite(getSprite(themeId, "map"), FlashAssets.getPosition(getSpriteName(themeId), "map"));

			strongholdObj.setOnSelect(Global.map.selectObject);
			
			return strongholdObj;
		}
	}
}


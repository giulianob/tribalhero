package src.Objects.Factories {

    import src.FlashAssets;
    import src.Global;
    import src.Objects.States.GameObjectState;
    import src.Objects.Stronghold.Stronghold;
    import src.Objects.Theme;

    public class StrongholdFactory {

		public function StrongholdFactory() {
		}

        public static function getSpriteName(theme: String, fallbackToDefaultTheme: Boolean = false): String {
            var typeName: String = theme.toUpperCase() + "_STRONGHOLD_STRUCTURE";
            if (fallbackToDefaultTheme && theme != Theme.DEFAULT_THEME_ID && !FlashAssets.doesSpriteExist(typeName)) {
                return getSpriteName("DEFAULT");
            }

            return typeName;
        }

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, groupId: int, objectId: int, level: int, tribeId: int, gateMax: int, themeId: String): Stronghold
		{
            var typeName: String = getSpriteName(themeId, true);
			var strongholdObj: Stronghold = new Stronghold(type, state, objX, objY, size, groupId, objectId, level, tribeId, gateMax, themeId);
            strongholdObj.setSprite(SpriteFactory.getStarlingImage(typeName), SpriteFactory.getMapPosition(typeName));
			strongholdObj.setOnSelect(Global.map.selectObject);
			
			return strongholdObj;
		}
	}
}


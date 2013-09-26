package src.Objects.Factories {

    import flash.display.Bitmap;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;

    import src.Assets;
    import src.Global;
    import src.Objects.States.GameObjectState;
    import src.Objects.Stronghold.Stronghold;

    public class StrongholdFactory {

		public function StrongholdFactory() {
		}

		public static function getSprite(withPosition: String): DisplayObjectContainer
		{
			var image: Bitmap = Assets.getInstance("STRONGHOLD_STRUCTURE", withPosition);

            var sprite: Sprite = new Sprite();
            sprite.addChild(image);

			return sprite;
		}

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, groupId: int, objectId: int, level: int, tribeId: int, gateMax: int): Stronghold
		{
			var strongholdObj: Stronghold = new Stronghold(type, state, objX, objY, size, groupId, objectId, level, tribeId, gateMax);

			strongholdObj.spriteContainer.addChild(getSprite("map"));

			strongholdObj.setOnSelect(Global.map.selectObject);
			
			return strongholdObj;
		}
	}
}


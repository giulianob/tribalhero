package src.Objects.Troop {
    import System.Linq.Enumerable;
    import System.Linq.Option.Option;

    import flash.display.Bitmap;
    import flash.display.DisplayObjectContainer;
    import flash.geom.Point;

    import src.Assets;

    import src.Constants;
    import src.Global;
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
        private var defaultSprite: DisplayObjectContainer;
        private var defaultPosition: Point;
        private var isOverWall: Boolean;

		public function TroopObject(type: int, state: GameObjectState, defaultSprite: DisplayObjectContainer, defaultPosition: Point, objX: int, objY: int, size: int, playerId: int, cityId: int, objectId: int) {
			super(type, state, objX, objY, size, playerId, cityId, objectId);
            this.defaultSprite = defaultSprite;
            this.defaultPosition = defaultPosition;

            setSprite(defaultSprite, defaultPosition);

            mapPriority = Constants.mapObjectPriority.troopObject;

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

        override public function moveFrom(prevPosition: Point): void {
            if (!isOverWall) {
                super.moveFrom(prevPosition);
            }
        }

        override public function setVisibilityPriority(isHighestPriority: Boolean, objectsInTile: Array): void {
            super.setVisibilityPriority(isHighestPriority, objectsInTile);

            var wallObjectResult: Option = Enumerable.from(objectsInTile).ofType(WallObject).firstOrNone();
            if (wallObjectResult.isSome) {
                if (isOverWall) {
                    return;
                }

                var wallObject: WallObject = wallObjectResult.value;
                var wallSprite: Bitmap = wallObject.getTroopOverlappingAsset();
                setSprite(wallSprite, new Point());
                isOverWall = true;
                return;
            }

            if (isOverWall) {
                setSprite(defaultSprite, defaultPosition);
                isOverWall = false;
            }
        }
    }
}
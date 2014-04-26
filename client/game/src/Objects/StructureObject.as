package src.Objects {

    import src.Constants;
    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.States.BattleState;
    import src.Objects.States.GameObjectState;

    public class StructureObject extends GameObject {
		
		public var properties: Array = [];
		public var level: int;
		public var labor: int;
		public var hp: int;
		
		public var wallManager: WallManager;
		public var radiusManager: RadiusManager;
        public var theme: String;
		
		public function StructureObject(theme: String, type: int, state: GameObjectState, objX: int, objY: int, size: int, playerId: int, cityId: int, objectId: int, level: int, wallRadius: int) {
			super(type, state, objX, objY, size, playerId, cityId, objectId);
			
			this.level = level;
            this.theme = theme;

            mapPriority = Constants.mapObjectPriority.structureObject;

            wallManager = new WallManager(this, wallRadius);
			radiusManager = new RadiusManager(this);
		}

        public function get isMainBuilding():Boolean
        {
            return objectId == 1;
        }
		
		override public function copy(obj:SimpleObject):void 
		{
			super.copy(obj);
			var gameObj: StructureObject = obj as StructureObject;
			level = gameObj.level;
            theme = gameObj.theme;
			wallManager.draw(gameObj.wallManager.radius);
		}
				
		override public function setSelected(bool:Boolean = false):void 
		{
			super.setSelected(bool);
			
			var prototype: StructurePrototype = getPrototype();
			if (prototype != null) {
				if (bool) radiusManager.showRadius(prototype.radius);
				else radiusManager.hideRadius();
			}
		}
		
		public function clearProperties():void
		{
			properties = [];
		}
		
		public function addProperty(value: * ):void
		{
			properties.push(value);
		}

        override public function equalsOnMap(obj:SimpleObject):Boolean
		{
			if (!(obj is StructureObject))
				return false;
				
			return type == (obj as StructureObject).type && level == (obj as StructureObject).level;						
		}
		
		public function getPrototype(): StructurePrototype 
		{
			return StructureFactory.getPrototype(type, level);
		}
			
		override public function dispose():void
		{
			super.dispose();
			
			radiusManager.hideRadius();
			wallManager.clear();
		}
	}
	
}
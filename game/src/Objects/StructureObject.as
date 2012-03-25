﻿package src.Objects {
	
	import flash.events.Event;
	import src.Objects.Factories.StructureFactory;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.SimpleGameObject;
	import src.Objects.States.GameObjectState;
	
	public class StructureObject extends GameObject {
		
		public var properties: Array = new Array();		
		public var level: int;
		public var labor: int;
		public var hp: int;
		
		public var wallManager: WallManager;
		public var radiusManager: RadiusManager;
		
		public function StructureObject(type: int, state: GameObjectState, objX: int, objY: int, playerId: int, cityId: int, objectId: int, level: int, wallRadius: int) {
			super(type, state, objX, objY, playerId, cityId, objectId);
			
			this.level = level;
			
			wallManager = new WallManager(this, wallRadius);
			radiusManager = new RadiusManager(this);
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
			properties = new Array();
		}
		
		public function addProperty(value: * ):void
		{
			properties.push(value);
		}	
		
		public function ToSprite(): Object
		{
			return StructureFactory.getSprite(type, level);
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
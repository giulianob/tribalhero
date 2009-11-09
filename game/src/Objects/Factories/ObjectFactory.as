﻿package src.Objects.Factories {
	import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
	import flash.display.Sprite;
	import src.Objects.SimpleGameObject;
	import src.Objects.SimpleObject;
	import src.Objects.Factories.*;
	import src.Objects.StructureObject;
	import src.Objects.TroopObject;
	import src.UI.SmartMovieClip;
	
	import flash.utils.getDefinitionByName;	
	
	public class ObjectFactory {
		
		public static const TYPE_UNIT: int = 1;
		public static const TYPE_STRUCTURE: int = 2;
		public static const TYPE_TROOP_OBJ: int = 3;
		
		public static function getClassType(type: int) : int
		{
			if (type >= 1000)
				return TYPE_STRUCTURE;
			else if (type == 100)
				return TYPE_TROOP_OBJ;
			else
				return TYPE_UNIT;
		}
		
		public static function getPrototype(type: int, level: int): *
		{
			var classType: int = getClassType(type);
			
			if (classType == TYPE_STRUCTURE)
				return StructureFactory.getPrototype(type, level);
			else if (classType == TYPE_UNIT)
				return UnitFactory.getPrototype(type, level);
			else
				return null;			
		}
		
		public static function getInstance(type: int, level: int): SimpleObject
		{
			var classType: int = getClassType(type);
			
			if (classType == TYPE_STRUCTURE)
				return StructureFactory.getInstance(type, level) as SimpleObject;
			else if (classType == TYPE_TROOP_OBJ)
				return TroopFactory.getInstance() as SimpleObject;
			else
				return null;
		}
		
		public static function getSpriteEx(type: int, level: int, centered: Boolean = false): DisplayObjectContainer
		{
			if (type >= 1000)			
				return StructureFactory.getSprite(type, level, centered);			
			else if (type == 100)
				return TroopFactory.getSprite(centered);
			else
				return UnitFactory.getSprite(type, level);
		}
		
		public static function getIcon(name: String) : DisplayObjectContainer
		{
			var objRef: Class;
			
			try {
				objRef = getDefinitionByName(name) as Class;	
			}
			catch (error: Error)
			{
				return new Sprite();
			}
			
			return new objRef() as DisplayObjectContainer;
		}
		
		public static function getSprite(obj: SimpleGameObject, centered: Boolean = false, encased: Boolean = false):DisplayObjectContainer
		{
			var sprite: DisplayObjectContainer;
			if (obj is StructureObject)			
				sprite = StructureFactory.getSprite((obj as StructureObject).type, (obj as StructureObject).level, centered);
			else if (obj is TroopObject)
				sprite = TroopFactory.getSprite(centered);
			else
				return null;
				
			if (encased) 
			{
				var holder: SmartMovieClip = new SmartMovieClip();
				holder.addChild(sprite);
				return holder;
			}
			else
				return sprite;
		}
		
		public static function getSimpleGameObject(name: String): SimpleGameObject {
			var obj: SimpleGameObject = new SimpleGameObject();			
			
			var objRef: Class = getDefinitionByName(name) as Class;
			obj.addChild(new objRef() as DisplayObject);
			
			return obj;
		}		
		
		public static function getSimpleObject(name: String): SimpleObject {
			var obj: SimpleObject = new SimpleObject();			
			
			var objRef: Class = getDefinitionByName(name) as Class;
			obj.addChild(new objRef() as DisplayObject);
			
			return obj;
		}
	}
	
}
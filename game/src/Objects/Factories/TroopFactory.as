package src.Objects.Factories {

	import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
	import flash.display.MovieClip;
	import flash.geom.Rectangle;
	import flash.utils.getDefinitionByName;
	import src.Objects.Troop.TroopObject;
	import src.UI.SmartMovieClip;

	/**
	* ...
	* @author Default
	*/
	public class TroopFactory {
		
		public function TroopFactory() {			
		}
		
		public static function getSprite(centered: Boolean = false): DisplayObjectContainer
		{
			var objRef: Class = getDefinitionByName("DEFAULT_TROOP") as Class;
			
			var sprite: DisplayObjectContainer = new objRef() as DisplayObjectContainer;
			
			if (centered)
			{
				var item: DisplayObject;
				for (var i: int = 0; i < sprite.numChildren; i++)
				{
					item = sprite.getChildAt(i);
					var rect: Rectangle = item.getRect(item);
					item.x -= rect.x;
					item.y -= rect.y;
				}
			}
			
			return sprite;
		}
		
		public static function getInstance(): Object
		{				
			var obj:Object = getSprite();
			
			var troopObject: TroopObject = new TroopObject();
			troopObject.addChild(obj as DisplayObject);
			
			return troopObject;					
		}
	}	
}
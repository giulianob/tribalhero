package src.Map
{
	import flash.display.*;
	import flash.events.*;
	import flash.geom.*;
	import src.*;
	import src.Map.*;
	import src.Objects.Factories.*;
	import src.UI.Tooltips.*;
	import src.Util.BinaryList.*;

	public class CityRegion extends Sprite
	{
		public static const DOT_SPRITE: String = "DOT_SPRITE_BLACK";
		public static const DOT_SPRITE_OWN: String = "DOT_SPRITE";		
		public static const DIFFICULTY_COLORS: Array = [
		{ r: 200, g: 200, b: 200 },
		{ r: 0, g: 156, b: 20 },
		{ r: 255, g: 223, b: 0},
		{ r: 255, g: 169, b: 0 },
		{ r: 210, g: 0, b: 0 }
		];
		public var id: int;
		private var globalX: int;
		private var globalY: int;
		private var objects: Array = new Array();

		public function CityRegion(id: int)
		{
			this.id = id;

			globalX = (id % Constants.miniMapRegionW) * Constants.cityRegionW;
			globalY = int(id / Constants.miniMapRegionW) * (Constants.cityRegionH / 2);
		}

		public function disposeData():void
		{
			for each(var obj: * in objects)			
				Global.gameContainer.miniMap.objContainer.removeObject(obj);

			objects = new Array();
		}
		
		private function createRegionObject(sprite: String, type: int, groupId: int, objectId: int, objX: int, objY: int) : CityRegionObject {		
			var coord: Point = MapUtil.getMiniMapScreenCoord(objX, objY);

			var regionObject: CityRegionObject = new CityRegionObject(type, groupId, objectId);
			regionObject.setX(objX);
			regionObject.setY(objY);
			var img: DisplayObject = ObjectFactory.getIcon(sprite);
			regionObject.addChild(img);

			Global.gameContainer.miniMap.objContainer.addObject(regionObject);
			
			return regionObject;
		}
		
		public function addCityObject(level: int, playerId: int, cityId: int, objectId: int, objCityValue: int, objX: int, objY : int) : CityRegionObject
		{
			var gameObj: CityRegionObject;
			
			// If it's our city, we just show a special flag
			if (Global.map.cities.get(cityId)) {
				gameObj = createRegionObject(DOT_SPRITE_OWN, ObjectFactory.TYPE_CITY, cityId, objectId, objX, objY);
				
				if (!gameObj) 
					return null;
			}
			else {
				gameObj = createRegionObject(DOT_SPRITE, ObjectFactory.TYPE_CITY, cityId, objectId, objX, objY);
				if (!gameObj)
					return null;
				
				// Apply the difficulty transformation to the tile				
				var percDiff: Number = Number(objCityValue) / Math.max(1.0, Number(Global.gameContainer.selectedCity.value));
				var difficultyIdx: int;
				if (percDiff <= 0.2) difficultyIdx = 0;
				else if (percDiff <= 0.75) difficultyIdx = 1;
				else if (percDiff <= 1.5) difficultyIdx = 2;
				else if (percDiff <= 1.9) difficultyIdx = 3;
				else difficultyIdx = 4;

				gameObj.transform.colorTransform = new ColorTransform(1, 1, 1, 1, DIFFICULTY_COLORS[difficultyIdx].r, DIFFICULTY_COLORS[difficultyIdx].g, DIFFICULTY_COLORS[difficultyIdx].b);
			}
			
			gameObj.addEventListener(MouseEvent.MOUSE_OVER, onCityObjectMouseOver);

			gameObj.extraProps.value = objCityValue;
			gameObj.extraProps.playerId = playerId;
			gameObj.extraProps.level = level;
			
			objects.push(gameObj);

			return gameObj;
		}
		
		public function addForestObject(level: int, groupId: int, objectId: int, objX: int, objY : int) : CityRegionObject
		{
			//add object to map and objects list
			var gameObj: CityRegionObject = createRegionObject("MINIMAP_FOREST_ICON", ObjectFactory.TYPE_FOREST, groupId, objectId, objX, objY);
			if (!gameObj)
				return null;
				
			gameObj.alpha = 0.5;
			gameObj.addEventListener(MouseEvent.MOUSE_OVER, onForestObjectMouseOver);
			
			gameObj.extraProps.level = level;
			
			objects.push(gameObj);

			return gameObj;
		}
		
		public function addTroopObject(objTroopId: int, playerId: int, cityId: int, objectId: int, objX: int, objY : int, resort: Boolean = true) : CityRegionObject
		{
			//add object to map and objects list
			var gameObj: CityRegionObject = createRegionObject("MINIMAP_TROOP_ICON", ObjectFactory.TYPE_TROOP_OBJ, cityId, objectId, objX, objY);
			if (!gameObj)
				return null;
				
			gameObj.addEventListener(MouseEvent.MOUSE_OVER, onTroopObjectMouseOver);

			gameObj.extraProps.troopId = objTroopId;
			objects.push(gameObj);

			return gameObj;
		}		
	
		public function onCityObjectMouseOver(e: MouseEvent) : void {
			new MinimapInfoTooltip(e.target is CityRegionObject ? e.target as CityRegionObject : e.target.parent);
		}
		
		public function onForestObjectMouseOver(e: MouseEvent) : void {
			new MinimapInfoTooltip(e.target is CityRegionObject ? e.target as CityRegionObject : e.target.parent);
		}		
		
		public function onTroopObjectMouseOver(e: MouseEvent) : void {
			new MinimapInfoTooltip(e.target is CityRegionObject ? e.target as CityRegionObject : e.target.parent);
		}				

		public function moveWithCamera(camera: Camera):void
		{
			x = globalX - camera.miniMapX - int(Constants.miniMapTileW / 2);
			y = globalY - camera.miniMapY - int(Constants.miniMapTileH / 2);
		}		
		public static function sortOnId(a:CityRegion, b:CityRegion):Number
		{
			var aId:Number = a.id;
			var bId:Number = b.id;

			if(aId > bId) {
				return 1;
			} else if(aId < bId) {
				return -1;
			} else  {
				return 0;
			}
		}

		public static function compare(a: CityRegion, value: int):int
		{
			return a.id - value;
		}
	}
}


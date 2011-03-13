package src.Map
{
	import flash.display.*;
	import flash.events.MouseEvent;
	import flash.filters.GlowFilter;
	import flash.geom.ColorTransform;
	import flash.geom.Point;
	import src.Global;
	import src.Objects.Factories.ObjectFactory;
	import src.UI.Tooltips.MinimapInfoTooltip;
	import src.Util.BinaryList.*;
	import src.Util.Util;
	import src.Constants;
	import src.Map.Map;
	import src.Map.Camera;
	import src.Objects.SimpleGameObject;

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
		private var objects: BinaryList = new BinaryList(objContainerSortOnCityIdAndObjId, objContainerCompareCityIdAndObjId);
		private var map: Map;

		public function CityRegion(id: int, map: Map)
		{
			this.id = id;

			this.map = map;

			globalX = (id % Constants.miniMapRegionW) * Constants.cityRegionW;
			globalY = int(id / Constants.miniMapRegionW) * (Constants.cityRegionH / 2);
		}

		public function disposeData():void
		{
			for each(var objContainer: * in objects.each())			
				src.Global.gameContainer.miniMap.objContainer.removeObject(objContainer.gameObj);

			objects.clear();
		}

		public function sortObjects():void
		{
			objects.sort();
		}
		
		private function getSimpleGameObject(sprite: String, type: int, playerId: int, cityId: int, objectId: int, level: int, objX: int, objY: int) : SimpleGameObject {
			var existingObj: * = objects.get([cityId, objectId]);
			
			if (existingObj != null) //don't add if obj already exists
			{
				Util.log("Obj id " + cityId + " " + objectId + " already exists in city region " + id);
				return null;
			}
				
			//add object to map and objects list
			var gameObj: SimpleGameObject = ObjectFactory.getSimpleGameObject(sprite);
			gameObj.name = "City Region Obj " + cityId + " " + objectId;
			gameObj.init(map, playerId, cityId, objectId, type);

			var coord: Point = MapUtil.getMiniMapScreenCoord(objX, objY);

			gameObj.setProperties(level, 0, coord.x, coord.y);

			Global.gameContainer.miniMap.objContainer.addObject(gameObj);			
			
			return gameObj;
		}
		
		private function getWrappedGameObject(gameObj: SimpleGameObject) : * {
			var objContainer: * = new Object();
			objContainer.gameObj = gameObj;
			return objContainer;
		}
		
		public function addCityObject(level: int, type: int, playerId: int, cityId: int, objectId: int, objX: int, objY : int, resort: Boolean = true) : SimpleGameObject
		{
			var gameObj: SimpleGameObject;
			
			// If it's our city, we just show a special flag
			if (map.cities.get(cityId)) {
				gameObj = getSimpleGameObject(DOT_SPRITE_OWN, type, playerId, cityId, objectId, level, objX, objY);
				
				if (!gameObj) 
					return null;
			}
			else {
				gameObj = getSimpleGameObject(DOT_SPRITE, type, playerId, cityId, objectId, level, objX, objY);
				if (!gameObj)
					return null;
				
				// Apply the difficulty transformation to the tile
				var levelDelta: int = level - Global.gameContainer.selectedCity.MainBuilding.level;
				var difficultyIdx: int;
				if (levelDelta <= -3)
					difficultyIdx = 0;
				else if (levelDelta <= 0)
					difficultyIdx = 1;
				else if (levelDelta <= 2)
					difficultyIdx = 2;
				else if (levelDelta <= 3)
					difficultyIdx = 3;
				else 
					difficultyIdx = 4;

				gameObj.transform.colorTransform = new ColorTransform(1, 1, 1, 1, DIFFICULTY_COLORS[difficultyIdx].r, DIFFICULTY_COLORS[difficultyIdx].g, DIFFICULTY_COLORS[difficultyIdx].b);
			}
			
			gameObj.addEventListener(MouseEvent.MOUSE_OVER, onCityObjectMouseOver);

			var objContainer: * = getWrappedGameObject(gameObj);
			
			objects.add(objContainer, resort);

			return gameObj;
		}
		
		public function addForestObject(level: int, objectId: int, objX: int, objY : int, resort: Boolean = true) : SimpleGameObject
		{
			//add object to map and objects list
			var gameObj: SimpleGameObject = getSimpleGameObject("MINIMAP_FOREST_ICON", ObjectFactory.TYPE_FOREST, 0, 0, objectId, level, objX, objY);
			if (!gameObj)
				return null;
				
			gameObj.alpha = 0.5;
			gameObj.addEventListener(MouseEvent.MOUSE_OVER, onForestObjectMouseOver);

			var objContainer: * = getWrappedGameObject(gameObj);
			
			objects.add(objContainer, resort);

			return gameObj;
		}
		
		public function addTroopObject(objTroopId: int, playerId: int, cityId: int, objectId: int, objX: int, objY : int, resort: Boolean = true) : SimpleGameObject
		{
			//add object to map and objects list
			var gameObj: SimpleGameObject = getSimpleGameObject("MINIMAP_TROOP_ICON", ObjectFactory.TYPE_TROOP_OBJ, playerId, cityId, objectId, 0, objX, objY);
			if (!gameObj)
				return null;
				
			gameObj.addEventListener(MouseEvent.MOUSE_OVER, onTroopObjectMouseOver);

			var objContainer: * = getWrappedGameObject(gameObj);
			objContainer.troopId = objTroopId;
			objects.add(objContainer, resort);

			return gameObj;
		}		

		public function removeObject(cityId: int, objectId: int, dispose: Boolean = true): SimpleGameObject
		{
			var objContainer: * = objects.remove([cityId, objectId]);

			if (objContainer == null) return null;

			Global.gameContainer.miniMap.objContainer.removeObject(objContainer.gameObj, 0, dispose);

			return objContainer.gameObj;
		}
		
		public function getContainerFromGameObject(gameObj: SimpleGameObject) : * 
		{
			return objects.get([gameObj.cityId, gameObj.objectId]);
		}

		public function getObject(cityId: int, objectId: int): SimpleGameObject
		{
			var objContainer: * = objects.get([cityId, objectId]);
			if (!objContainer)
				return null;
				
			return objContainer.gameObj;
		}

		public function onCityObjectMouseOver(e: MouseEvent) : void {
			new MinimapInfoTooltip(0, getContainerFromGameObject(e.target.parent));
		}
		
		public function onForestObjectMouseOver(e: MouseEvent) : void {
			new MinimapInfoTooltip(1, getContainerFromGameObject(e.target.parent));
		}		
		
		public function onTroopObjectMouseOver(e: MouseEvent) : void {
			new MinimapInfoTooltip(2, getContainerFromGameObject(e.target.parent));
		}				

		public function moveWithCamera(camera: Camera):void
		{
			x = globalX - camera.miniMapX - int(Constants.miniMapTileW / 2);
			y = globalY - camera.miniMapY - int(Constants.miniMapTileH / 2);
		}

		public static function objContainerSortOnCityIdAndObjId(a: *, b: *):Number {
			var aCityId:Number = a.gameObj.cityId;
			var bCityId:Number = b.gameObj.cityId;

			var aObjId:Number = a.gameObj.objectId;
			var bObjId:Number = b.gameObj.objectId;

			if (aCityId > bCityId) return 1;
			else if (aCityId < bCityId) return -1;
			else if (aObjId > bObjId) return 1;
			else if (aObjId < bObjId) return -1;
			else return 0;
		}
		
		public static function objContainerCompareCityIdAndObjId(a: *, value: Array):int
		{
			var cityDelta: int = a.gameObj.cityId - value[0];
			var idDelta: int = a.gameObj.objectId - value[1];

			if (cityDelta != 0) 
				return cityDelta;

			if (idDelta != 0) return idDelta;
			else return 0;
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


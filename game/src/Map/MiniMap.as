package src.Map 
{
	import flash.display.MovieClip;
	import flash.display.SpreadMethod;
	import flash.display.Sprite;
	import src.Constants;
	import src.Map.Camera;
	import src.Objects.ObjectContainer;
	import src.UI.SmartMovieClip;
	
	/**
	* ...
	* @author Giuliano Barberi
	*
	*/
	public class MiniMap extends Sprite
	{
		private var map: Map;
		
		private var regionSpace: Sprite;
		private var regions: CityRegionList;
		private var pendingRegions: Array = new Array();
		
		public var objContainer: ObjectContainer;
		
		public function MiniMap(map: Map)
		{
			mouseEnabled = false;
			mouseChildren = false;
			
			regions = new CityRegionList(map);
			this.map = map;
			
			regionSpace = new Sprite();
			objContainer = new ObjectContainer(map, false);
			
			var tilesW: int = Constants.screenW / Constants.tileW;
			var tilesH: int = Constants.screenH / Constants.tileH;
			
			var screenRect: Sprite = new Sprite();
			screenRect.graphics.lineStyle(1, 0xFFFFFF);
			screenRect.graphics.drawRect(0, 0, tilesW * Constants.miniMapTileW, tilesH * Constants.miniMapTileH);
			
			x = (Constants.miniMapScreenW / 2) - (screenRect.width/2);
			y = (Constants.miniMapScreenH / 2) - (screenRect.height/2);
			
			addChild(screenRect);
			
			addChild(regionSpace);
			
			addChild(objContainer);
		}
		
		public function addCityRegion(id:int) : CityRegion
		{
			if (Constants.debug >= 2)
				trace("Adding region: " + id);
				
			var newRegion: CityRegion = new CityRegion(id, map);
			
			for (var i:int = pendingRegions.length - 1; i >= 0; i--)
			{
				if (pendingRegions[i] == id)
				{
					pendingRegions.splice(i, 1);
				}
			}
			
			regions.add(newRegion);
			newRegion.moveWithCamera(map.gameContainer.camera);
			regionSpace.addChild(newRegion);
			
			return newRegion;
		}						
		
		public function parseRegions():void
		{
			if (Constants.debug >= 4)
				trace("on move: " + map.gameContainer.camera.miniMapX + "," + map.gameContainer.camera.miniMapY);
			
			//calculate which regions we need to render
			var requiredRegions: Array = new Array();
			
			var camX: int = map.gameContainer.camera.miniMapX - x;
			var camY: int = map.gameContainer.camera.miniMapY - y;
						
			const offsetx:int = 50;
			const offsety:int = 30;
			var requiredId: int = 0;
			
			//top middle
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW/2, camY);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//top middle (upcache)
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW/2, camY - offsety);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
				
			//bottom middle
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW/2, camY + Constants.miniMapScreenH);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//bottom middle (down)
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW/2, camY + Constants.miniMapScreenH + offsety);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			
			//left middle
			requiredId = MapUtil.getCityRegionId(camX, camY + Constants.miniMapScreenH/2);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//left middle (left)
			requiredId = MapUtil.getCityRegionId(camX - offsetx, camY + Constants.miniMapScreenH/2);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
				
			//right middle
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW, camY + Constants.miniMapScreenH/2);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//right middle (right)
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW + offsetx, camY + Constants.miniMapScreenH/2);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
				
			//top-left (up cache)
			requiredId = MapUtil.getCityRegionId(camX, camY - offsety);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//top-left (left cache)
			requiredId = MapUtil.getCityRegionId(camX - offsetx, camY);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//top-left
			requiredId = MapUtil.getCityRegionId(camX, camY);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			
			//bottom-left (left)
			requiredId = MapUtil.getCityRegionId(camX - offsetx, camY + Constants.miniMapScreenH);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//bottom-left (down)
			requiredId = MapUtil.getCityRegionId(camX, camY + Constants.miniMapScreenH + offsety);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//bottom-left
			requiredId = MapUtil.getCityRegionId(camX, camY + Constants.miniMapScreenH);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			
			//top-right (up)
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW, camY - offsety);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//top-right (right)
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW + offsetx, camY);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//top-right
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW, camY);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			
			//bottom-right (down)
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW, camY + Constants.miniMapScreenH + offsety);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//bottom-right (right)
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW + offsetx, camY + Constants.miniMapScreenH);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//bottom-right
			requiredId = MapUtil.getCityRegionId(camX + Constants.miniMapScreenW, camY + Constants.miniMapScreenH);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			
			//remove any outdated regions from regions we have		
			for (var i: int = regions.size() - 1; i >= 0; i--)
			{												
				var region: CityRegion = regions.getByIndex(i);
				
				var found: int = -1;
				for (var a:int= 0; a < requiredRegions.length; a++)
				{
					if (region.id == requiredRegions[a])
					{
						found = a;
						break;
					}
				}
				
				if (found >= 0)
				{
					//adjust the position of this region
					region.moveWithCamera(map.gameContainer.camera);
					
					if (Constants.debug >= 4)
						trace("Moved: " + region.id + " " + region.x + "," + region.y);
						
					//remove it from required regions since we already have it
					requiredRegions.splice(found, 1);
				}
				else
				{
					//region is outdated, remove it from buffer				
					if (Constants.debug >= 4)
						trace("Discarded: " + region.id);					
					region.disposeData();
					regionSpace.removeChild(region);
					regions.removeByIndex(i);					
				}
			}
			
			//remove any pending regions from the required regions list we need
			//and add any regions we are going to be asking the server to the pending regions list
			for (i = requiredRegions.length - 1; i >= 0; i--)
			{
				found = -1;
				
				for (a = 0; a < pendingRegions.length; a++)
				{
					if (pendingRegions[a] == requiredRegions[i])
					{
						found = i;
						break;
					}
				}
				
				if (found >= 0)
				{
					requiredRegions.splice(found, 1);
				}
				else
				{
					pendingRegions.push(requiredRegions[i]);
				}
			}									
			
			//regions that we still need, query server
			if (requiredRegions.length > 0)
			{
				if (Constants.debug >= 3)
					trace("Required:" + requiredRegions);
					
				map.mapComm.Region.getCityRegion(requiredRegions);
			}
		}
		
	}
	
}
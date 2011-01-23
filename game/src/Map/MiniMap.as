﻿package src.Map
{
	import flash.display.Sprite;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import mx.messaging.ConsumerMessageDispatcher;
	import org.aswing.ASColor;
	import org.aswing.graphics.Graphics2D;
	import org.aswing.graphics.SolidBrush;
	import src.Constants;
	import src.Global;
	import src.Objects.ObjectContainer;

	/**
	 * ...
	 * @author Giuliano Barberi
	 *
	 */
	public class MiniMap extends Sprite
	{
		public static const NAVIGATE_TO_POINT: String = "NAVIGATE_TO_POINT";

		private var regionSpace: Sprite;
		private var regions: CityRegionList;
		private var pendingRegions: Array = new Array();

		public var objContainer: ObjectContainer;

		private var screenRect: Sprite;
		private var mapHolder: Sprite;
		private var bg: Sprite;
		private var mapMask: Sprite;

		private var miniMapWidth: int;
		private var miniMapHeight: int;

		private var lastClick: Number;
		private var lastClickPoint: Point = new Point();

		public function MiniMap(width: int, height: int)
		{
			regions = new CityRegionList();

			regionSpace = new Sprite();

			objContainer = new ObjectContainer(false);

			mapHolder = new Sprite();

			screenRect = new Sprite();
			
			mapHolder.addChild(screenRect);
			mapHolder.addChild(regionSpace);
			mapHolder.addChild(objContainer);

			bg = new Sprite();

			addEventListener(MouseEvent.CLICK, onNavigate);

			mapMask = new Sprite();

			addChild(bg);
			addChild(mapHolder);
			addChild(mapMask);

			mask = mapMask;

			resize(width, height);
		}

		public function redraw() : void {
			// Redraw screen rectangle
			var tilesW: Number = (Constants.screenW * Global.gameContainer.camera.getZoomFactorOverOne()) / Constants.tileW + 0.5;
			var tilesH: Number = (Constants.screenH * Global.gameContainer.camera.getZoomFactorOverOne()) / Constants.tileH + 0.5;

			if (tilesW * Constants.miniMapTileW < this.miniMapWidth && tilesH * Constants.miniMapTileH < this.miniMapHeight) {			
				screenRect.graphics.clear();
				screenRect.graphics.lineStyle(1, 0xFFFFFF);
				screenRect.graphics.drawRect(0, 0, tilesW * Constants.miniMapTileW, tilesH * Constants.miniMapTileH);
			}

			// Resize map
			mapHolder.x = (this.miniMapWidth / 2) - (screenRect.width / 2);
			mapHolder.y = (this.miniMapHeight / 2) - (screenRect.height / 2);

			bg.graphics.clear();
			var g: Graphics2D = new Graphics2D(bg.graphics);
			bg.alpha = 0.6;
			g.fillRoundRect(new SolidBrush(ASColor.BLACK), 0, 0, this.miniMapWidth, this.miniMapHeight, 10);

			mapMask.graphics.clear();
			g = new Graphics2D(mapMask.graphics);
			g.fillRoundRect(new SolidBrush(ASColor.BLACK), 0, 0, this.miniMapWidth, this.miniMapHeight, 10);			
		}

		private function onNavigate(e: MouseEvent) : void {

			var currentMousePoint: Point = new Point(stage.mouseX, stage.mouseY);
			if (Point.distance(currentMousePoint, lastClickPoint) > 10 || (new Date().time) - lastClick > 350) {
				lastClick = new Date().time;
				lastClickPoint = currentMousePoint;
				return;
			}

			lastClick = new Date().time;
			//Calculate where the user clicked in real map position
			var camX: int = Global.gameContainer.camera.miniMapX - mapHolder.x;
			var camY: int = Global.gameContainer.camera.miniMapY - mapHolder.y;

			var local: Point = this.globalToLocal(new Point(e.stageX, e.stageY));
			var centeredOffsetX: int = local.x;
			var centeredOffsetY: int = local.y;

			var mapX: int = ((camX + centeredOffsetX) / Constants.miniMapTileW) * Constants.tileW;
			var mapY: int = ((camY + centeredOffsetY) / Constants.miniMapTileH) * Constants.tileH;

			var event: MouseEvent = new MouseEvent(NAVIGATE_TO_POINT, true, false, mapX, mapY);
			dispatchEvent(event);
		}

		public function resize(width: int, height: int) : void {
			this.miniMapWidth = width;
			this.miniMapHeight = height;
			
			redraw();
		}

		public function setScreenRectHidden(hidden: Boolean) : void {
			screenRect.visible = !hidden;
		}

		public function addCityRegion(id:int) : CityRegion
		{
			if (Constants.debug >= 2)
			trace("Adding region: " + id);

			var newRegion: CityRegion = new CityRegion(id, Global.map);

			for (var i:int = pendingRegions.length - 1; i >= 0; i--)
			{
				if (pendingRegions[i] == id)
				{
					pendingRegions.splice(i, 1);
				}
			}

			regions.add(newRegion);
			newRegion.moveWithCamera(Global.gameContainer.camera);
			regionSpace.addChild(newRegion);

			return newRegion;
		}

		public function parseRegions():void
		{
			if (Constants.debug >= 4) {
				trace("on move: " + Global.gameContainer.camera.miniMapX + "," + Global.gameContainer.camera.miniMapY);
			}

			//calculate which regions we need to render
			var requiredRegions: Array = new Array();

			var camX: int = Global.gameContainer.camera.miniMapX - mapHolder.x;
			var camY: int = Global.gameContainer.camera.miniMapY - mapHolder.y;

			var regionsW: int = Math.ceil(miniMapWidth / Constants.cityRegionW);
			var regionsH: int = Math.ceil(miniMapHeight / (Constants.cityRegionH / 2));

			for (var c: int = 0; c <= regionsW; c++) {
				for (var r: int = 0; r <= regionsH; r++) {
					var requiredId: int = MapUtil.getCityRegionId(camX + Constants.cityRegionW * c, camY + (Constants.cityRegionH / 2) * r);
					if (requiredId == -1 || requiredRegions.indexOf(requiredId) > -1) continue;
					requiredRegions.push(requiredId);
				}
			}

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
					region.moveWithCamera(Global.gameContainer.camera);

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

				Global.mapComm.Region.getCityRegion(requiredRegions);
			}
		}
	}

}


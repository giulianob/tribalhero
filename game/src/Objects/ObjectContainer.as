
package src.Objects {

	import flash.display.DisplayObject;
	import flash.display.Sprite;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import src.Constants;
	import src.Global;
	import src.Map.Camera;
	import src.Map.MapUtil;
	import src.UI.Dialog.ObjectSelectDialog;
	import src.UI.Tooltips.TextTooltip;
	import src.Util.BinaryList.BinaryList;
	import src.Util.Util;

	import src.Objects.GameObject;

	public class ObjectContainer extends Sprite {

		public static const NORMAL: int = 2;
		public static const LOWER: int = 1;
		public static const UPPER: int = 3;

		private var objSpace: Sprite;
		private var bottomSpace: Sprite;
		private var topSpace: Sprite;

		public var objects: BinaryList = new BinaryList(SimpleObject.sortOnXandY, SimpleObject.compareXAndY);

		private var dimmedObjects: Array = new Array();
		private var highlightedObject: GameObject;

		private var originClick: Point = new Point(0, 0);
		private var ignoreClick: Boolean = false;

		private var multipleObjTooltip: TextTooltip = null;
		
		private var mouseDisabled: Boolean;

		public function ObjectContainer(mouseEnabled: Boolean = true)
		{
			bottomSpace = new Sprite();
			addChild(bottomSpace);

			objSpace = new Sprite();
			addChild(objSpace);

			topSpace = new Sprite();
			addChild(topSpace);

			this.mouseEnabled = mouseEnabled;			

			if (mouseEnabled)
			{
				addEventListener(Event.ADDED_TO_STAGE, addToStage);
				addEventListener(Event.REMOVED_FROM_STAGE, removeFromStage);
			}

			bottomSpace.name = "Object Bottom Space";
			objSpace.name = "Object Space";
			topSpace.name = "Object Top Space";
		}

		public function addToStage(e: Event) : void {
			objSpace.addEventListener(MouseEvent.MOUSE_MOVE, eventMouseMove);
			objSpace.addEventListener(MouseEvent.CLICK, eventMouseClick);
			objSpace.addEventListener(MouseEvent.MOUSE_DOWN, eventMouseDown);
			objSpace.addEventListener(MouseEvent.MOUSE_OUT, eventMouseOut);
		}

		public function removeFromStage(e: Event) : void {			
			objSpace.removeEventListener(MouseEvent.MOUSE_MOVE, eventMouseMove);
			objSpace.removeEventListener(MouseEvent.CLICK, eventMouseClick);
			objSpace.removeEventListener(MouseEvent.MOUSE_DOWN, eventMouseDown);
			objSpace.removeEventListener(MouseEvent.MOUSE_OUT, eventMouseOut);
		}
		
		public function disableMouse(disabled: Boolean) : void {
			mouseDisabled = disabled;
		}

		public function eventMouseClick(e: MouseEvent):void
		{
			if (mouseDisabled) return;
			
			if (highlightedObject && !ignoreClick)
			{
				var idxs: Array = Util.binarySearchRange(objects.each(), SimpleObject.compareXAndY, [highlightedObject.getX(), highlightedObject.getY()]);
				if (idxs.length > 1)
				{
					var multiObjects: Array = new Array();
					for each(var idx: int in idxs)
					multiObjects.push(objects.getByIndex(idx));

					var objectSelection: ObjectSelectDialog = new ObjectSelectDialog(multiObjects,
					function(sender: ObjectSelectDialog):void {
						Global.map.selectObject((sender as ObjectSelectDialog).selectedObject, true, true);
						sender.getFrame().dispose();
					}
					);

					objectSelection.show();
				}
				else
				{
					Global.map.selectObject(highlightedObject, true, true);
				}
				resetHighlightedObject();
				e.stopImmediatePropagation();
			}

			ignoreClick = false;
		}

		public function eventMouseDown(e: MouseEvent):void
		{
			if (mouseDisabled) return;
			
			originClick = new Point(e.stageX, e.stageY);
		}

		public function eventMouseOut(e: MouseEvent):void
		{
			if (mouseDisabled) return;
			
			resetHighlightedObject();
			resetDimmedObjects();
			ignoreClick = false;
		}

		public function eventMouseMove(e: MouseEvent):void
		{
			if (mouseDisabled) return;
			
			if (e.buttonDown)
			{
				if (Point.distance(new Point(e.stageX, e.stageY), originClick) > 4)
				ignoreClick = true;
			}

			var tilePos: Point = MapUtil.getActualCoord(e.stageX * Global.gameContainer.camera.getZoomFactorOverOne() + Global.gameContainer.camera.x, e.stageY * Global.gameContainer.camera.getZoomFactorOverOne() + Global.gameContainer.camera.y);

			if (tilePos.x < 0 || tilePos.y < 0) return;

			tilePos = MapUtil.getMapCoord(tilePos.x, tilePos.y);
			
			resetDimmedObjects();
			resetHighlightedObject();

			var overlapping: Array = new Array();
			var found: Boolean = false;
			var highestObj: SimpleObject = null;
			var obj: SimpleObject;
			var objects: Array = new Array();
			
			var i: int;
			for (i = 0; i < objSpace.numChildren; i++)
			{
				obj = objSpace.getChildAt(i) as SimpleObject;

				if (!obj || !obj.visible) continue;

				if (obj is GameObject) 
				{
					// If mouse is over this object's tile then it automatically gets chosen as the best object
					var objMapPos: Point = MapUtil.getMapCoord((obj as GameObject).getX(), (obj as GameObject).getY());
					if (objMapPos.x == tilePos.x && objMapPos.y == tilePos.y)
					{					
						highestObj = obj as GameObject;
						found = true;
						break;
					}
				}

				if (obj.hitTestPoint(e.stageX, e.stageY, true))
				{
					if ((!highestObj && obj is GameObject) || (obj is GameObject && obj.getY() < highestObj.getY()))
					highestObj = obj as GameObject;

					objects.push(obj);
				}
			}

			if (!found && objects.length == 1 && objects[0] is SimpleGameObject)
			{
				highestObj = objects[0] as SimpleObject;
				found = true;
			}

			if (!found && !highestObj) return;			

			var highestObjMapPos: Point = MapUtil.getMapCoord(highestObj.getX(), highestObj.getY());
			
			// Apply dimming to objects that might be over the one currently moused over
			for (i = 0; i < objSpace.numChildren; i++)
			{
				obj = objSpace.getChildAt(i) as SimpleObject;

				if (!obj.visible) continue;

				if (obj == highestObj || obj.getY() <= highestObj.getY()) continue;
				
				if (Math.abs(highestObj.getX() - obj.getX()) < Constants.tileW)
				{
					// Split this into 2 ifs for performance
					objMapPos = MapUtil.getMapCoord(obj.getX(), obj.getY());				
					if (MapUtil.distance(highestObjMapPos.x, highestObjMapPos.y, objMapPos.x, objMapPos.y) == 1)
					{
						dimmedObjects.push(obj);
						obj.alpha = 0.25;
					}
				}
			}

			var idxs: Array = Util.binarySearchRange(this.objects.each(), SimpleObject.compareXAndY, [highestObj.getX(), highestObj.getY()]);
			if (idxs.length > 1)
			{
				multipleObjTooltip = new TextTooltip(idxs.length + " objects in this space. Click to view all");
				multipleObjTooltip.show(highestObj);
			}

			if (highestObj is GameObject)
			{
				highlightedObject = highestObj as GameObject;
				(highestObj as GameObject).setHighlighted(true);
			}
		}

		public function resetObjects():void
		{
			resetHighlightedObject();
			resetDimmedObjects();
		}

		public function resetHighlightedObject():void
		{
			if (highlightedObject)
			{
				if (multipleObjTooltip != null)
				{
					multipleObjTooltip.hide();
					multipleObjTooltip = null;
				}
				highlightedObject.setHighlighted(false);
				highlightedObject = null;
			}
		}

		public function resetDimmedObjects():void
		{
			if (dimmedObjects)
			{
				for each (var dimmedObj: SimpleObject in dimmedObjects)
				dimmedObj.alpha = 1;
			}

			dimmedObjects = new Array();
		}

		private function getLayer(layer: int): Sprite
		{
			switch(layer)
			{
				case LOWER:
					return bottomSpace;
				break;
				case UPPER:
					return topSpace;
				break;
				default:
					return objSpace;
				break;
			}
		}

		public function addObject(obj: IScrollableObject, layer: int = 0):void
		{
			getLayer(layer).addChildAt(DisplayObject(obj), calculateDepth(obj, getLayer(layer)));

			if (layer == 0 && obj is SimpleGameObject)
			{
				objects.add(obj);				
				showBestObject(obj.getX(), obj.getY());
			}
		}

		public function removeObject(obj: IScrollableObject, layer: int = 0, dispose: Boolean = true):void
		{
			if (obj == null)
			return;

			getLayer(layer).removeChild(DisplayObject(obj));

			if (layer == 0 && obj is SimpleGameObject)
			{
				var idxs: Array = Util.binarySearchRange(objects.each(), SimpleObject.compareXAndY, [obj.getX(), obj.getY()]);

				for each (var idx: int in idxs)
				{
					var currObj: SimpleGameObject = objects.getByIndex(idx) as SimpleGameObject;

					if (SimpleGameObject.compareCityIdAndObjId(obj as SimpleGameObject, [currObj.cityId, currObj.objectId]) == 0)
					{
						if (dispose) (obj as SimpleGameObject).dispose();
						objects.removeByIndex(idx);
						break;
					}
				}

				showBestObject(obj.getX(), obj.getY());
			}
		}

		private function showBestObject(x: int, y: int):void
		{
			//figure out which object is the best to show on the map if multiple obj exist on this tile
			var idxs: Array = Util.binarySearchRange(objects.each(), SimpleObject.compareXAndY, [x, y]);

			var bestObj: SimpleGameObject = null;
			var currObj: SimpleGameObject = null;

			for each (var idx: int in idxs)
			{
				currObj = objects.getByIndex(idx) as SimpleGameObject;

				if (bestObj == null)
				{
					bestObj = currObj;
					continue;
				}

				if (currObj is StructureObject)
				{
					bestObj = currObj;
					break;
				}
			}

			if (currObj == null)
			return;

			for each (idx in idxs)
			{
				currObj = objects.getByIndex(idx) as SimpleGameObject;
				currObj.visible = (SimpleGameObject.compareCityIdAndObjId(bestObj, [currObj.cityId, currObj.objectId]) == 0);
			}
		}
		
		public function moveWithCamera(x: int, y: int):void
		{
			var camera: Camera = new Camera(x, y);
			moveLayerWithCamera(camera, topSpace);
			moveLayerWithCamera(camera, bottomSpace);
			moveLayerWithCamera(camera, objSpace);
		}

		private function moveLayerWithCamera(camera: Camera, layer: Sprite):void
		{
			for (var i: int = 0; i < layer.numChildren; i++)
			{
				var obj: IScrollableObject = (layer.getChildAt(i) as IScrollableObject);

				if (obj == null)
				continue;

				obj.moveWithCamera(camera);
			}
		}

		private function calculateDepth(obj: IScrollableObject, layer: Sprite): int
		{
			return binarySearchDepth(obj.getY(), 0, layer.numChildren - 1, layer);
		}

		private function binarySearchDepth(y: int, low: int, high: int, layer: Sprite): int
		{
			while (low <= high) {
				var mid: int = (low + high) / 2;

				var currentObj: IScrollableObject = layer.getChildAt(mid) as IScrollableObject;

				if (currentObj.getY() > y)
				high = mid - 1;
				else if (currentObj.getY() < y)
				low = mid + 1;
				else
				return mid;
			}

			return low;
		}
		
		public function hasStructureAt(x: int, y: int) : Boolean {
			var objs: Array = objects.getRange([x, y]);
			for each (var obj: SimpleGameObject in objs) {
				if (obj is StructureObject) return true;
			}
			
			return false;
		}
	}

}


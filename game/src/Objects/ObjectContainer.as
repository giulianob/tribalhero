
package src.Objects {

	import flash.display.*;
	import flash.events.*;
	import flash.geom.*;
	import src.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Factories.*;
	import src.Objects.Troop.TroopObject;
	import src.UI.Dialog.*;
	import src.UI.Tooltips.*;
	import src.Util.*;
	import src.Util.BinaryList.*;


	public class ObjectContainer extends Sprite {

		public static const NORMAL: int = 2;
		public static const LOWER: int = 1;
		public static const UPPER: int = 3;

		private var objSpace: Sprite;
		private var bottomSpace: Sprite;
		private var topSpace: Sprite;

		public var objects: BinaryList = new BinaryList(SimpleObject.sortOnXandY, SimpleObject.compareXAndY);

		private var dimmedObjects: Array = new Array();
		private var highlightedObject: SimpleObject;

		private var originClick: Point = new Point(0, 0);
		private var ignoreClick: Boolean = false;

		private var objTooltip: Tooltip = null;
		
		private var mouseDisabled: Boolean;
		
		private var showObjectCount: Boolean = true;

		public function ObjectContainer(mouseEnabled: Boolean = true, showObjectCount: Boolean = true)
		{
			this.showObjectCount = showObjectCount;
			
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
				var multiObjects: Array = new Array();
				for each(var idx: int in idxs) {
					var obj: SimpleObject = objects.getByIndex(idx);
					if (obj.isSelectable())
						multiObjects.push(obj);
				}
				
				if (multiObjects.length > 1)
				{
					var objectSelection: ObjectSelectDialog = new ObjectSelectDialog(multiObjects,
					function(sender: ObjectSelectDialog):void {
						Global.map.selectObject((sender as ObjectSelectDialog).selectedObject);
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
			eventMouseMove(e);
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

			var selectableCnt: int = 0;
			var overlapping: Array = new Array();
			var found: Boolean = false;
			var highestObj: SimpleObject = null;
			var obj: SimpleObject;
			var objects: Array = new Array();
			
			var i: int;
			for (i = 0; i < objSpace.numChildren; i++)
			{
				obj = objSpace.getChildAt(i) as SimpleObject;

				if (!obj || !obj.visible || !obj.isSelectable()) continue;
				
				selectableCnt++;

				if (obj is GameObject) 
				{
					// If mouse is over this object's tile then it automatically gets chosen as the best object
					var objMapPos: Point = MapUtil.getMapCoord(obj.getX(), obj.getY());
					if (objMapPos.x == tilePos.x && objMapPos.y == tilePos.y)
					{					
						highestObj = obj;
						found = true;
						break;
					}
				}

				if (obj.hitTestPoint(e.stageX, e.stageY, true))
				{
					if (!highestObj || obj.getY() < highestObj.getY() || (highestObj is SimpleObject && obj is SimpleGameObject))
						highestObj = obj;

					objects.push(obj);
				}
			}

			if (!found && selectableCnt == 1)
			{
				highestObj = objects[0];
				found = true;
			}

			if (!found && !highestObj) 
				return;			
			
			// If we still have the same highest obj then stop here
			if (highlightedObject == highestObj) {
				// Adjust tooltip to current mouse position
				if (objTooltip) 
					objTooltip.show(highestObj);				
				return;
			}
			
			// Reset the highlighted obj since it has changed
			resetHighlightedObject();
			resetDimmedObjects();

			var highestObjMapPos: Point = MapUtil.getMapCoord(highestObj.getX(), highestObj.getY());
			
			// Apply dimming to objects that might be over the one currently moused over
			for (i = 0; i < objSpace.numChildren; i++)
			{
				obj = objSpace.getChildAt(i) as SimpleObject;

				if (!obj.visible) continue;

				if (obj == highestObj || obj.getY() <= highestObj.getY()) continue;
				
				if (Math.abs(highestObj.getX() - obj.getX()) < Constants.tileW)
				{
					objMapPos = MapUtil.getMapCoord(obj.getX(), obj.getY());				
					if (MapUtil.distance(highestObjMapPos.x, highestObjMapPos.y, objMapPos.x, objMapPos.y) <= 1)
					{
						dimmedObjects.push(obj);
						obj.alpha = 0.25;
					}
				}
			}

			var idxs: Array = Util.binarySearchRange(this.objects.each(), SimpleObject.compareXAndY, [highestObj.getX(), highestObj.getY()]);
			if (idxs.length > 1)
			{
				selectableCnt = 0;
				for each (var idx: int in idxs) {
					obj = this.objects.getByIndex(idx);
					if (obj.isSelectable())
						selectableCnt++;
				}
				
				if (selectableCnt > 1) {
					objTooltip = new TextTooltip(idxs.length + " objects in this space. Click to view all");
					objTooltip.show(highestObj);
				}
			}

			// Show tooltip for hovered obj
			if (!objTooltip) {
				if (highestObj is StructureObject) {
					var structureObj: StructureObject = highestObj as StructureObject;
					objTooltip = new StructureTooltip(structureObj, StructureFactory.getPrototype(structureObj.type, structureObj.level));					
				}				
				else if (highestObj is TroopObject) {
					var troopObj: TroopObject = highestObj as TroopObject;
					objTooltip = new TroopObjectTooltip(troopObj);					
				}								
			}
			
			highlightedObject = highestObj;
			highestObj.setHighlighted(true);			
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
				if (objTooltip != null)
				{
					objTooltip.hide();
					objTooltip = null;
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

			if (layer == 0)
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

			if (layer == 0 && obj is SimpleObject)
			{
				if (Global.map.selectedObject && Global.map.selectedObject == obj)
					Global.map.selectObject(null);
							
				var idxs: Array = Util.binarySearchRange(objects.each(), SimpleObject.compareXAndY, [obj.getX(), obj.getY()]);

				var found: Boolean = false;
				for each (var idx: int in idxs)
				{
					var currObj: SimpleObject = objects.getByIndex(idx) as SimpleObject;

					if (obj == currObj)
					{
						objects.removeByIndex(idx);
						if (dispose)
							currObj.dispose();	
						found = true;
						break;
					}
				}

				if (found)
					showBestObject(obj.getX(), obj.getY());
			}
		}

		private function showBestObject(x: int, y: int):void
		{
			//figure out which object is the best to show on the map if multiple obj exist on this tile
			var idxs: Array = Util.binarySearchRange(objects.each(), SimpleObject.compareXAndY, [x, y]);

			var bestObj: SimpleObject = null;
			var currObj: SimpleObject = null;
			
			var selectableCnt: int = 0;

			for each (var idx: int in idxs)
			{
				currObj = objects.getByIndex(idx) as SimpleObject;
				
				if (currObj && currObj.isSelectable())
					selectableCnt++;
				
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
				currObj = objects.getByIndex(idx) as SimpleObject;
				if (bestObj == currObj) {
					currObj.visible = true; 
					if (showObjectCount)
						currObj.setObjectCount(selectableCnt);
				} else {
					currObj.visible = false;
					if (showObjectCount)
						currObj.setObjectCount(0);
				}
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
			for each (var obj: SimpleObject in objs) {
				if (obj is StructureObject) 
					return true;
			}
			
			return false;
		}
	}

}


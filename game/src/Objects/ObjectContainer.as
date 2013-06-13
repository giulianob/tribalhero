
package src.Objects {

	import com.greensock.TweenMax;
	import fl.motion.Color;
	import flash.display.*;
	import flash.events.*;
	import flash.geom.*;
	import flash.utils.Dictionary;
	import src.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Factories.*;
	import src.Objects.Stronghold.Stronghold;
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
		
		private var dimmedObjects: Array = [];
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
				var idxs: Array = Util.binarySearchRange(objects.toArray(), SimpleObject.compareXAndY, [highlightedObject.objX, highlightedObject.objY]);
				var multiObjects: Array = [];
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
			var overlapping: Array = [];
			var found: Boolean = false;
			var highestObj: SimpleObject = null;
			var obj: SimpleObject;
			var objects: Array = [];
			
			var i: int;
			for (i = 0; i < objSpace.numChildren; i++)
			{
				obj = objSpace.getChildAt(i) as SimpleObject;

				if (!obj || !obj.visible || obj.alpha == 0 || !obj.isSelectable()) continue;
				
				selectableCnt++;
				
				if (obj is GameObject) 
				{
					// If mouse is over this object's tile then it automatically gets chosen as the best object
					var objMapPos: Point = MapUtil.getMapCoord(obj.objX, obj.objY);
					if (objMapPos.x == tilePos.x && objMapPos.y == tilePos.y)
					{					
						highestObj = obj;
						found = true;
						break;
					}
				}

				if (obj.hitTestPoint(e.stageX, e.stageY, true))
				{
					if (!highestObj || obj.objY < highestObj.objY || (highestObj is SimpleObject && obj is SimpleGameObject))
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
			
			var highestObjMapPos: Point = MapUtil.getMapCoord(highestObj.objX, highestObj.objY);
			
			// Apply dimming to objects that might be over the one currently moused over
			for (i = 0; i < objSpace.numChildren; i++)
			{
				obj = objSpace.getChildAt(i) as SimpleObject;

				if (!obj.visible || obj.alpha == 0) continue;

				if (obj == highestObj || obj.objY <= highestObj.objY) continue;
				
				if (Math.abs(highestObj.objX - obj.objX) < Constants.tileW)
				{
					objMapPos = MapUtil.getMapCoord(obj.objX, obj.objY);
					if (MapUtil.distance(highestObjMapPos.x, highestObjMapPos.y, objMapPos.x, objMapPos.y) <= 1)
					{
						dimmedObjects.push(obj);
						TweenMax.to(obj, 1, { alpha: 0.25 } );
					}
				}
			}
		
			var idxs: Array = Util.binarySearchRange(objects.toArray(), SimpleObject.compareXAndY, [highestObj.objX, highestObj.objY]);
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
				else if (highestObj is Stronghold) {
					var strongholdObj: Stronghold = highestObj as Stronghold;
					objTooltip = new StrongholdTooltip(strongholdObj);
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
				for each (var dimmedObj: SimpleObject in dimmedObjects) {
					TweenMax.to(dimmedObj, 1, { alpha: 1 } );
				}
			}

			dimmedObjects = [];
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
		
		public function addObject(obj: DisplayObject, layer: int = 0):void
		{
			var simpleObj: SimpleObject = obj as SimpleObject
			if (layer == 0 && simpleObj)
			{
				getLayer(layer).addChildAt(simpleObj, calculateDepth(simpleObj.objY, getLayer(layer)));
				objects.add(simpleObj);
				showBestObject(simpleObj.objX, simpleObj.objY);
			}
			else {
				getLayer(layer).addChildAt(obj, calculateDepth(obj.y, getLayer(layer)));
			}
		}

		public function removeObject(obj: DisplayObject, layer: int = 0, dispose: Boolean = true):void
		{
			if (obj == null)
				return;
			
			var childIndex: int = getLayer(layer).getChildIndex(obj);
			if (childIndex >= 0) {
				getLayer(layer).removeChildAt(childIndex);
			}
						
			var simpleObj: SimpleObject = obj as SimpleObject;
			if (layer == 0 && simpleObj)
			{								
				var idxs: Array = Util.binarySearchRange(objects.toArray(), SimpleObject.compareXAndY, [simpleObj.objX, simpleObj.objY]);

				var found: Boolean = false;
				for each (var idx: int in idxs)
				{
					var currObj: SimpleObject = objects.getByIndex(idx) as SimpleObject;

					if (simpleObj == currObj)
					{
						objects.removeByIndex(idx);
						
						if (dispose) {
							simpleObj.dispose();	
						}
						
						simpleObj.setObjectCount(0);
						
						found = true;
						break;
					}
				}

				if (found) {
					showBestObject(simpleObj.objX, simpleObj.objY);
				}
				else {
					trace("NOT FOUND");
				}
			}
		}

		private function showBestObject(x: int, y: int):void
		{
			//figure out which object is the best to show on the map if multiple obj exist on this tile
			var idxs: Array = Util.binarySearchRange(objects.toArray(), SimpleObject.compareXAndY, [x, y]);

			var bestObj: SimpleObject = null;			
			
			var selectableCnt: int = 0;

			var currObj: SimpleObject;
			
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

				if (currObj is StructureObject || currObj is Stronghold || currObj is BarbarianTribe)
				{
					bestObj = currObj;
					break;
				}
			}

			for each (idx in idxs)
			{
				currObj = objects.getByIndex(idx) as SimpleObject;
				if (currObj == null) {
					continue;
				}
				
				if (bestObj == currObj) {
					TweenMax.to(currObj, 0.75, { alpha: 1 } );
					
					if (showObjectCount) {
						currObj.setObjectCount(selectableCnt);
					}
				} else {
					TweenMax.to(currObj, 0.5, { alpha: 0 } );
					
					if (showObjectCount) {
						currObj.setObjectCount(0);
					}
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
			layer.x = -camera.x;
			layer.y = -camera.y;
		}

		private function calculateDepth(y: Number, layer: Sprite): int
		{
			return binarySearchDepth(y, 0, layer.numChildren - 1, layer);
		}
		
		private function binarySearchDepth(y: Number, low: int, high: int, layer: Sprite): int
		{
			while (low <= high) {
				var mid: int = (low + high) / 2;

				var currentObj: DisplayObject = layer.getChildAt(mid) as DisplayObject;
				var simpleObj: SimpleObject = currentObj as SimpleObject;
				var objY: Number = simpleObj == null ? currentObj.y : simpleObj.objY;
				
				if (objY > y) {
					high = mid - 1;
				}
				else if (objY < y) {
					low = mid + 1;
				}
				else {
					return mid;
				}
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


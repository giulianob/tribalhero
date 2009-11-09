
package src.Objects {
	
	import flash.display.BitmapData;
	import flash.display.DisplayObject;
	import flash.display.Shape;	
	import flash.display.Sprite;	
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Matrix;
	import flash.geom.Point;
	import flash.utils.Timer;
	import src.Constants;
	import src.Map.Camera;
	import src.Map.Map;
	import src.Map.MapUtil;
	import src.Objects.Prototypes.SimpleLayout;
	import src.UI.Dialog.Dialog;
	import src.UI.Dialog.ObjectSelectDialog;
	import src.UI.Tooltips.TextTooltip;	
	import src.Util.Util;
	
	import src.Objects.GameObject;
	
	public class ObjectContainer extends Sprite {	
		
		public static const NORMAL: int = 2;
		public static const LOWER: int = 1;
		public static const UPPER: int = 3;
		
		private var objSpace: Sprite;
		private var bottomSpace: Sprite;
		private var topSpace: Sprite;
	
		private var map: Map;
		public var objects: Array = new Array();

		private var dimmedObjects: Array = new Array();
		private var highlightedObject: GameObject;

		private var originClick: Point = new Point(0, 0);
		private var ignoreClick: Boolean = false;
		
		private var multipleObjTooltip: TextTooltip = null;
		
		public function ObjectContainer(map: Map, mouseEnabled: Boolean = true) 
		{								
			bottomSpace = new Sprite();
			addChild(bottomSpace);
			
			objSpace = new Sprite();			
			addChild(objSpace);
			
			topSpace = new Sprite();
			addChild(topSpace);			
			
			this.mouseEnabled = mouseEnabled;
			this.mouseChildren = mouseEnabled;
			
			if (mouseEnabled) 
			{				
				addEventListener(Event.ADDED_TO_STAGE, addToStage);				
				addEventListener(Event.REMOVED_FROM_STAGE, removeFromStage);
			}
			
			bottomSpace.name = "Object Bottom Space";
			objSpace.name = "Object Space";
			topSpace.name = "Object Top Space";
			
			this.map = map;
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
		
		public function eventMouseClick(e: MouseEvent):void
		{			
			if (highlightedObject && !ignoreClick)
			{
				var idxs: Array = Util.binarySearchRange(objects, SimpleObject.compareXAndY, [highlightedObject.getX(), highlightedObject.getY()]);
				if (idxs.length > 1)
				{
					var multiObjects: Array = new Array();
					for each(var idx: int in idxs)
						multiObjects.push(objects[idx]);
						
					var objectSelection: ObjectSelectDialog = new ObjectSelectDialog(multiObjects, 
						function(sender: ObjectSelectDialog):void { 
							map.selectObject((sender as ObjectSelectDialog).selectedObject); 
							sender.getFrame().dispose();
						}
					);
					
					objectSelection.show();
				}
				else
				{
					map.selectObject(highlightedObject);
				}
				resetHighlightedObject();
				e.stopImmediatePropagation();
			}		
			
			ignoreClick = false;					
		}
		
		public function eventMouseDown(e: MouseEvent):void
		{
			originClick = new Point(e.stageX, e.stageY);
		}
		
		public function eventMouseOut(e: MouseEvent):void
		{
			resetHighlightedObject();
			resetDimmedObjects();
			ignoreClick = false;
		}
						
		public function eventMouseMove(e: MouseEvent):void
		{
			if (e.buttonDown)
			{
				if (Point.distance(new Point(e.stageX, e.stageY), originClick) > 4)
					ignoreClick = true;
			}
			
			var tilePos: Point = MapUtil.getActualCoord(e.stageX + map.gameContainer.camera.x, e.stageY + map.gameContainer.camera.y);			
			
			if (tilePos.x < 0 || tilePos.y < 0)
				return;								
						
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
					
				if (obj is GameObject && (obj as GameObject).getX() == tilePos.x && (obj as GameObject).getY() == tilePos.y)
				{					
					highestObj = obj as GameObject;
					found = true;
					break;
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
			
			if (!found && !highestObj)
			{
				return;						
			}
							
			for (i = 0; i < objSpace.numChildren; i++)
			{				
				obj = objSpace.getChildAt(i) as SimpleObject;
				
				if (!obj.visible) continue;
								
				if (obj == highestObj || obj.getY() < highestObj.getY())
					continue;					
					
				if (obj.hitTestObject(highestObj))
				{
					dimmedObjects.push(obj);
					obj.alpha = 0.25;				
				}
			}		
			
			var idxs: Array = Util.binarySearchRange(this.objects, SimpleObject.compareXAndY, [highestObj.getX(), highestObj.getY()]);				
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
				objects.push(obj);						
				resortObjects();
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
				var idxs: Array = Util.binarySearchRange(objects, SimpleObject.compareXAndY, [obj.getX(), obj.getY()]);
				
				for each (var idx: int in idxs)
				{
					var currObj: SimpleGameObject = objects[idx] as SimpleGameObject;
					
					if (SimpleGameObject.compareCityIdAndObjId(obj as SimpleGameObject, [currObj.cityId, currObj.objectId]) == 0)
					{
						if (dispose) (obj as SimpleGameObject).dispose();
						objects.splice(idx, 1);	
						break;
					}
				}
				
				showBestObject(obj.getX(), obj.getY());
			}			
		}
		
		private function showBestObject(x: int, y: int):void
		{
			//figure out which object is the best to show on the map if multiple obj exist on this tile
			var idxs: Array = Util.binarySearchRange(objects, SimpleObject.compareXAndY, [x, y]);
			
			var bestObj: SimpleGameObject = null;
			var currObj: SimpleGameObject = null;
			
			for each (var idx: int in idxs)
			{
				currObj = objects[idx] as SimpleGameObject;
				
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
				currObj = objects[idx] as SimpleGameObject;	
				currObj.visible = (SimpleGameObject.compareCityIdAndObjId(bestObj, [currObj.cityId, currObj.objectId]) == 0);
			}
		}
		
		private function resortObjects():void
		{
			objects.sort(SimpleObject.sortOnXandY);
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
				
	}
	
}

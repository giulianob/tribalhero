
package src.Objects {

    import System.Linq.Enumerable;

    import com.greensock.TweenMax;

    import flash.display.*;
    import flash.events.*;
    import flash.geom.*;
    import flash.utils.Dictionary;

    import src.*;
    import src.Map.*;
    import src.Objects.Factories.*;
    import src.Objects.Stronghold.Stronghold;
    import src.Objects.Troop.TroopObject;
    import src.UI.Dialog.ObjectSelectDialog;
    import src.UI.Tooltips.*;

    public class ObjectContainer extends Sprite {

		public static const LOWER: int = 1;
		public static const UPPER: int = 3;

		private var objSpace: Sprite;
		private var bottomSpace: Sprite;
		private var topSpace: Sprite;

		private var dimmedObjects: Array = [];
		private var highlightedObject: SimpleObject;

		private var originClick: Point = new Point(0, 0);
		private var ignoreClick: Boolean = false;

		private var objTooltip: Tooltip = null;
		
		private var mouseDisabled: Boolean;
		
		private var showObjectCount: Boolean = true;

        private var objects: Dictionary = new Dictionary();

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
				var multiObjects: Array = [];

                for each (var position: Position in TileLocator.foreachMultitileObject(highlightedObject)) {
                    for each (var objectInTile: SimpleObject in objects[TileLocator.getTileIndex(position)]) {

                        if (objectInTile.isSelectable() && multiObjects.indexOf(objectInTile) === -1) {
                            multiObjects.push(objectInTile);
                        }
                    }
                }

				if (multiObjects.length > 1)
				{
					var objectSelection: ObjectSelectDialog = new ObjectSelectDialog(multiObjects, function(sender: ObjectSelectDialog):void {
						Global.map.selectObject((sender as ObjectSelectDialog).selectedObject);
						sender.getFrame().dispose();
					});

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

            var screenPos: ScreenPosition = TileLocator.getActualCoord(
                    e.stageX * Global.gameContainer.camera.getZoomFactorOverOne() + Global.gameContainer.camera.currentPosition.x,
                    e.stageY * Global.gameContainer.camera.getZoomFactorOverOne() + Global.gameContainer.camera.currentPosition.y);

            if (screenPos.x < 0 || screenPos.y < 0) return;

			var tilePos: Position = screenPos.toPosition();

			var selectableCnt: int = 0;

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
					var objMapPos: Position = obj.primaryPosition.toPosition();
					if (objMapPos.x == tilePos.x && objMapPos.y == tilePos.y)
					{					
						highestObj = obj;
						found = true;
						break;
					}
				}

				if (obj.hitTestPoint(e.stageX, e.stageY, true))
				{
					if (!highestObj || obj.primaryPosition.y < highestObj.primaryPosition.y || (highestObj is SimpleObject && obj is SimpleGameObject))
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
			
			var highestObjMapPos: Position = highestObj.primaryPosition.toPosition();
			
			// Apply dimming to objects that might be over the one currently moused over
			for (i = 0; i < objSpace.numChildren; i++)
			{
				obj = objSpace.getChildAt(i) as SimpleObject;

				if (!obj.visible || obj.alpha == 0) continue;

				if (obj == highestObj || obj.primaryPosition.y <= highestObj.primaryPosition.y) continue;
				
				if (Math.abs(highestObj.primaryPosition.x - obj.primaryPosition.x) < Constants.tileW)
				{
					objMapPos = obj.primaryPosition.toPosition();
					if (TileLocator.distance(highestObjMapPos.x, highestObjMapPos.y, highestObj.size, objMapPos.x, objMapPos.y, obj.size) <= 1)
					{
						dimmedObjects.push(obj);
						TweenMax.to(obj, 1, { alpha: 0.25 } );
					}
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
			var simpleObj: SimpleObject = obj as SimpleObject;
			if (layer == 0 && simpleObj)
			{
                simpleObj.setObjectCount(1);

				getLayer(layer).addChildAt(simpleObj, calculateDepth(simpleObj.primaryPosition.y, getLayer(layer)));

                addObjectToDictionary(simpleObj);

                var objectsInTile: Array = objects[TileLocator.getTileIndex(simpleObj.primaryPosition.toPosition())];

                // Sort the objects by priority
                objectsInTile.sortOn("mapPriority", Array.NUMERIC);

                var highestPriorityObject: SimpleObject = objectsInTile[0];

                // If we just added the highest priority obj then go through each of its tiles and grab all the objs to hide
                if (highestPriorityObject == obj) {
                    objectsInTile = [];
                    var objCount: int = 1;
                    for each (var position: Position in TileLocator.foreachMultitileObject(simpleObj)) {
                        for each (var objectToHide: SimpleObject in objects[TileLocator.getTileIndex(position)]) {
                            // Only consider the primary location of the obj so we dont count them multiple times
                            if (objectToHide === highestPriorityObject) {
                                continue;
                            }

                            objectsInTile.push(objectToHide);

                            if (objectToHide.isSelectable()) {
                                objCount++;
                            }
                        }
                    }

                    highestPriorityObject.setObjectCount(objCount);
                }
                // Increment the count of the first one if there are multiple objs in the tile
                else {
                    if (simpleObj.isSelectable()) {
                        highestPriorityObject.setObjectCount(highestPriorityObject.getObjectCount() + 1);
                    }
                }

                if (!highestPriorityObject.visible) {
                    highestPriorityObject.fadeIn();
                }

                // Hide the other objects in the tile
                for (var i: int = 0; i < objectsInTile.length; i++)
                {
                    var objectInTile: SimpleObject = objectsInTile[i];

                    if (objectInTile === highestPriorityObject) {
                        continue;
                    }

                    objectInTile.setObjectCount(1);
                    objectInTile.visible = false;
                }
			}
			else {
				getLayer(layer).addChildAt(obj, calculateDepth(obj.y, getLayer(layer)));
			}
		}

        private function addObjectToDictionary(simpleObj: SimpleObject): void
        {
            for each (var position: Position in TileLocator.foreachMultitileObject(simpleObj)) {
                var tileIndex: int = TileLocator.getTileIndex(position);
                if (objects[tileIndex] == null) {
                    objects[tileIndex] = [simpleObj];
                }
                else {
                    objects[tileIndex].push(simpleObj);
                }
            }
        }

        private function removeObjectFromDictionary(simpleObj: SimpleObject): void
        {
            for each (var position: Position in TileLocator.foreachMultitileObject(simpleObj)) {
                var tileIndex: int = TileLocator.getTileIndex(position);
                var objectsInTile: Array = objects[tileIndex];
                if (!objectsInTile) {
                    continue;
                }

                var objectIndex: int = objectsInTile.indexOf(simpleObj);
                if (objectIndex == -1) {
                    continue;
                }

                objectsInTile.splice(objectIndex, 1);

                if (objectsInTile.length == 0) {
                    delete objects[tileIndex];
                }
            }
        }

		public function removeObject(obj: DisplayObject, layer: int = 0, dispose: Boolean = true):void
		{
			if (obj == null) {
				return;
            }
			
			var childIndex: int = getLayer(layer).getChildIndex(obj);
			if (childIndex >= 0) {
				getLayer(layer).removeChildAt(childIndex);
			}

			var simpleObj: SimpleObject = obj as SimpleObject;
			if (layer == 0 && simpleObj)
			{
                if (dispose) {
                    simpleObj.dispose();
                }

                removeObjectFromDictionary(simpleObj);

                var tileIndex: int = TileLocator.getTileIndex(simpleObj.primaryPosition.toPosition());
                var objectsInTile: Array = objects[tileIndex];
                if (objectsInTile) {
                    objectsInTile.sortOn("mapPriority", Array.NUMERIC);

                    var highestPriorityObject: SimpleObject = objectsInTile[0];

                    // If the current highest obj was already visible then
                    // the obj being removed was already hidden by someone else.
                    // In this case, we can just decrement the highestObj count.
                    if (highestPriorityObject.visible) {
                        if (simpleObj.isSelectable()) {
                            highestPriorityObject.setObjectCount(highestPriorityObject.getObjectCount() - 1);
                        }

                        return;
                    }
                }

                // Otherwise, it means that we've removed an obj that was hiding other guys so we need to reset
                // each of the objs in those tiles
                var positionsToCheck: Array = TileLocator.foreachMultitileObject(simpleObj);
                for each (var eachPosition: Position in positionsToCheck) {
                    var eachObjectsInTile: Array = objects[TileLocator.getTileIndex(eachPosition)];
                    if (!eachObjectsInTile) {
                        continue;
                    }

                    eachObjectsInTile.sortOn("mapPriority", Array.NUMERIC);
                    var eachHighestPriorityObject: SimpleObject = eachObjectsInTile[0];
                    eachHighestPriorityObject.setObjectCount(getSelectableObjectCount(eachObjectsInTile));

                    if (!eachHighestPriorityObject.visible) {
                        eachHighestPriorityObject.fadeIn();
                    }

                    for (var i: int = 1; i < eachObjectsInTile.length; i++) {
                        eachObjectsInTile[i].setObjectCount(1);
                        eachObjectsInTile[i].visible = false;
                    }
                }
			}
		}

        private function getSelectableObjectCount(objectsInTile: Array): int
        {
            return Enumerable.from(objectsInTile).where(function (o: SimpleObject): Boolean {
                return o.isSelectable();
            }).count();
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
			layer.x = -camera.currentPosition.x;
			layer.y = -camera.currentPosition.y;
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
				var objY: Number = simpleObj == null ? currentObj.y : simpleObj.primaryPosition.y;

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

    }

}


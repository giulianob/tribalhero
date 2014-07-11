
package src.Objects {

    import System.Linq.Enumerable;

    import flash.display.DisplayObject;

    import flash.geom.Point;

    import starling.display.*;
    import starling.events.*;
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

		private var objSpace: Sprite;
		private var bottomSpace: Sprite;

		private var dimmedObjects: Array = [];
		private var highlightedObject: SimpleObject;

		private var originClick: Point = new Point(0, 0);

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

			if (mouseEnabled)
			{
				addEventListener(Event.ADDED_TO_STAGE, addToStage);
				addEventListener(Event.REMOVED_FROM_STAGE, removeFromStage);
			}

			bottomSpace.name = "Object Bottom Space";
			objSpace.name = "Object Space";
		}

        public function addToStage(e: Event) : void {
            objSpace.addEventListener(TouchEvent.TOUCH, onTouched);
        }

        public function removeFromStage(e: Event) : void {
            objSpace.removeEventListener(TouchEvent.TOUCH, onTouched);
        }
		
		public function disableMouse(disabled: Boolean) : void {
			mouseDisabled = disabled;
		}

        public function onTouched(e: TouchEvent): void {
            if (mouseDisabled) {
                return;
            }

            var clickEvent: Touch = e.getTouch(objSpace, TouchPhase.BEGAN);
            if (clickEvent) {
                onMouseDown(clickEvent);
                return;
            }

            if (e.getTouch(objSpace, TouchPhase.ENDED)) {
                onClicked(e);
                return;
            }

            var hoverTouch: Touch = e.getTouch(objSpace, TouchPhase.HOVER);
            if (hoverTouch) {
                onMouseMove(hoverTouch);
            }
            else {
                resetHighlightedObject();
                resetDimmedObjects();
            }
        }

		public function onClicked(e: TouchEvent):void
		{
			if (highlightedObject)
			{
				var multiObjects: Array = [];

                for each (var position: Position in TileLocator.foreachMultitileObject(highlightedObject)) {
                    for each (var objectInTile: SimpleObject in objects[getGlobalTileIndex(position)]) {

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

			onMouseMove(e.getTouch(objSpace, TouchPhase.ENDED));
		}

		public function onMouseDown(touch: Touch):void
		{
            originClick = new Point(touch.globalX, touch.globalY);
		}

		public function onMouseMove(touch: Touch):void
		{
            if (hasInteractiveFlashObjectAtPosition(touch.globalX, touch.globalY)) {
                resetDimmedObjects();
                resetHighlightedObject();

                return;
            }

            var screenPos: ScreenPosition = TileLocator.getActualCoord(
                    touch.globalX * Global.gameContainer.camera.getZoomFactorOverOne() + Global.gameContainer.camera.currentPosition.x,
                    touch.globalY * Global.gameContainer.camera.getZoomFactorOverOne() + Global.gameContainer.camera.currentPosition.y);

            if (screenPos.x < 0 || screenPos.y < 0) {
                return;
            }

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

				if (!obj || !obj.isHighestPriority || !obj.isSelectable()) continue;
				
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

				if (obj.hitTest(touch.getLocation(obj), false))
				{
					if (!highestObj || obj.primaryPosition.y < highestObj.primaryPosition.y || (highestObj is SimpleObject && obj is SimpleGameObject)) {
						highestObj = obj;
                    }

					objects.push(obj);
				}
			}

			if (!found && selectableCnt == 1)
			{
				highestObj = objects[0];
				found = true;
			}

			if (!found && !highestObj) {
                resetHighlightedObject();
                resetDimmedObjects();

                return;
            }

            // If we still have the same highest obj then stop here
            if (highlightedObject == highestObj) {
				// Adjust tooltip to current mouse position
				if (objTooltip) {
					objTooltip.show(obj);
                }
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

				if (!obj.visible) {
                    continue;
                }

				if (obj == highestObj || obj.primaryPosition.y <= highestObj.primaryPosition.y) continue;
				
				if (Math.abs(highestObj.primaryPosition.x - obj.primaryPosition.x) < Constants.tileW)
				{
					objMapPos = obj.primaryPosition.toPosition();
					if (TileLocator.distance(highestObjMapPos.x, highestObjMapPos.y, highestObj.size, objMapPos.x, objMapPos.y, obj.size) <= 1)
					{
						dimmedObjects.push(obj);
						obj.dim();
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

        private function hasInteractiveFlashObjectAtPosition(globalX: Number, globalY: Number): Boolean {
            // this is temporary while we have UI built using regular stage
            // we want to make those sprites block mouse access to starling sprites
            // We need to check each object for visibility and also check if all of its parents have mouse children enabled

            var objectsUnderPoint: Array = Global.stage.getObjectsUnderPoint(new Point(globalX, globalY));
            for each (var flashDisplayObject: flash.display.DisplayObject in objectsUnderPoint) {
                if (!flashDisplayObject.visible) {
                    continue;
                }

                var flashInteractive: flash.display.InteractiveObject = flashDisplayObject as flash.display.InteractiveObject;
                if (flashInteractive && !flashInteractive.mouseEnabled) {
                    continue;
                }

                var parent: flash.display.DisplayObjectContainer = flashDisplayObject.parent;
                var hasInvisibleParent: Boolean = false;
                while (parent) {
                    if (!parent.visible || !parent.mouseChildren) {
                        hasInvisibleParent = true;
                        break;
                    }

                    parent = parent.parent;
                }

                if (hasInvisibleParent) {
                    continue;
                }

                return true;
            }

            return false;
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
					dimmedObj.fadeIn(true);
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
				break;
				default:
					return objSpace;
				break;
			}
		}
		
		public function addObject(obj: starling.display.DisplayObject, layer: int = 0):void
		{
			var simpleObj: SimpleObject = obj as SimpleObject;

			if (layer != 0 || !simpleObj)
			{
                getLayer(layer).addChildAt(obj, calculateDepth(obj.y, Constants.mapObjectPriority.none, getLayer(layer)));
                return;
            }

            simpleObj.setObjectCount(1);

            getLayer(layer).addChildAt(simpleObj, calculateDepth(simpleObj.primaryPosition.y, simpleObj.mapPriority, getLayer(layer)));

            addObjectToDictionary(simpleObj);

            var objectsInTile: Array = objects[getGlobalTileIndex(simpleObj.primaryPosition.toPosition())];

            // Sort the objects by priority
            objectsInTile.sortOn("mapPriority", Array.NUMERIC);

            var highestPriorityObject: SimpleObject = objectsInTile[0];

            // If we just added the highest priority obj then go through each of its tiles and grab all the objs to hide
            if (highestPriorityObject == obj) {
                objectsInTile = [];
                var objCount: int = 1;
                for each (var position: Position in TileLocator.foreachMultitileObject(simpleObj)) {
                    for each (var objectToHide: SimpleObject in objects[getGlobalTileIndex(position)]) {
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

            highestPriorityObject.setVisibilityPriority(true, objectsInTile);

            // Hide the other objects in the tile
            for (var i: int = 1; i < objectsInTile.length; i++)
            {
                var objectInTile: SimpleObject = objectsInTile[i];
                objectInTile.setObjectCount(1);
                objectInTile.setVisibilityPriority(false, objectsInTile);
            }
        }

        private function addObjectToDictionary(simpleObj: SimpleObject): void
        {
            for each (var position: Position in TileLocator.foreachMultitileObject(simpleObj)) {
                var tileIndex: int = getGlobalTileIndex(position);
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
                var tileIndex: int = getGlobalTileIndex(position);
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

		public function removeObject(obj: starling.display.DisplayObject, layer: int = 0, dispose: Boolean = true):void
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

                var tileIndex: int = getGlobalTileIndex(simpleObj.primaryPosition.toPosition());
                var objectsInTile: Array = objects[tileIndex];
                if (objectsInTile) {
                    objectsInTile.sortOn("mapPriority", Array.NUMERIC);

                    var highestPriorityObject: SimpleObject = objectsInTile[0];

                    // If the current highest obj was already visible then
                    // the obj being removed was already hidden by someone else.
                    // In this case, we can just decrement the highestObj count.
                    if (highestPriorityObject.isHighestPriority) {
                        if (simpleObj.isSelectable()) {
                            highestPriorityObject.setObjectCount(highestPriorityObject.getObjectCount() - 1);
                        }

                        highestPriorityObject.setVisibilityPriority(true, objectsInTile);

                        for (var i: int = 1; i < objectsInTile.length; i++) {
                            objectsInTile[i].setVisibilityPriority(false, objectsInTile);
                        }

                        return;
                    }
                }

                // Otherwise, it means that we've removed an obj that was hiding other guys so we need to reset
                // each of the objs in those tiles
                var positionsToCheck: Array = TileLocator.foreachMultitileObject(simpleObj);
                for each (var eachPosition: Position in positionsToCheck) {
                    var eachObjectsInTile: Array = objects[getGlobalTileIndex(eachPosition)];
                    if (!eachObjectsInTile) {
                        continue;
                    }

                    eachObjectsInTile.sortOn("mapPriority", Array.NUMERIC);
                    var eachHighestPriorityObject: SimpleObject = eachObjectsInTile[0];
                    eachHighestPriorityObject.setObjectCount(getSelectableObjectCount(eachObjectsInTile));

                    eachObjectsInTile[0].setVisibilityPriority(true, eachObjectsInTile);

                    for (i = 1; i < eachObjectsInTile.length; i++) {
                        eachObjectsInTile[i].setObjectCount(1);
                        eachObjectsInTile[i].setVisibilityPriority(false, eachObjectsInTile);
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
			moveLayerWithCamera(camera, bottomSpace);
			moveLayerWithCamera(camera, objSpace);
		}

		private function moveLayerWithCamera(camera: Camera, layer: Sprite):void
		{
			layer.x = -camera.currentPosition.x;
			layer.y = -camera.currentPosition.y;
		}

		private function calculateDepth(y: Number, mapPriority: int, layer: Sprite): int
		{
			return binarySearchDepth(y, mapPriority, 0, layer.numChildren - 1, layer);
		}
		
		private function binarySearchDepth(y: Number, mapPrioty: int, low: int, high: int, layer: Sprite): int
		{
			while (low <= high) {
				var mid: int = (low + high) / 2;

				var currentObj: starling.display.DisplayObject = layer.getChildAt(mid) as starling.display.DisplayObject;
				var simpleObj: SimpleObject = currentObj as SimpleObject;
				var objY: Number = simpleObj == null ? currentObj.y : simpleObj.primaryPosition.y;
                var objPriority: int = simpleObj == null ? Constants.mapObjectPriority.displayObject : simpleObj.mapPriority;

                if (objY > y) {
					high = mid - 1;
				}
				else if (objY < y) {
					low = mid + 1;
				}
				else if (objPriority < mapPrioty)  {
                    high = mid - 1;
                }
                else if (objPriority > mapPrioty) {
                    low = mid + 1;
                }
                else {
					return mid;
				}
			}

			return low;
		}

        private function getGlobalTileIndex(position: Position): int
        {
            return (int)(position.x + position.y * Constants.mapTileW);
        }
    }

}


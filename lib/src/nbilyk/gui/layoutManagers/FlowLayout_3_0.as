/*
	Copyright (c) 2006, 2007 Nicholas Bilyk
*/

package nbilyk.gui.layoutManagers {
		
	import flash.display.DisplayObject;
	import flash.geom.Point;
	import flash.geom.Rectangle;
	import nbilyk.utils.CoordMath;
	
	public class FlowLayout_3_0 extends LayoutManager_3_0 {
		private var _hGap = 5;
		private var _vGap = 5;
		private var _defaultHAlign:int = 0;
		private var _defaultVAlign:int = 0;
		public static var LEFT:int = 0;
		public static var CENTER:int = 1;
		public static var RIGHT:int = 2;
		public static var TOP:int = 0;
		public static var MIDDLE:int = 1;
		public static var BOTTOM:int = 2;
		
		function FlowLayout_3_0(root_mc:DisplayObject) {
			super(root_mc);
		}
		
		override protected function draw():void {
			super.draw();
			this.removeInvalidObjects();
			
			var iPoint:Point = new Point(this.leftMargin, this.topMargin);
			var tallestInRow:Number = 0;
			var numObjects:Number = this.objects_arr.length;
			var positions_arr:Array = new Array(numObjects);
			var rowIndices_arr:Array = new Array();
			var hOffsets_arr:Array = new Array();
			var rowHeights_arr:Array = new Array();
			for (var i:int=0; i<numObjects; i++) {
				positions_arr[i] = iPoint.clone();
				
				var currObjDims:Point = new Point(this.objects_arr[i].width, this.objects_arr[i].height);
				currObjDims = CoordMath.convertDistance(currObjDims, this.objects_arr[i].parent, this.root_mc);
				if (currObjDims.y > tallestInRow) tallestInRow = currObjDims.y;
				
				if (i < numObjects - 1) {
					// Get the next object's dimensions to see if it should be on the next line.
					var nextObjDims:Point = new Point(this.objects_arr[i+1].width, this.objects_arr[i+1].height);
					nextObjDims = CoordMath.convertDistance(nextObjDims, this.objects_arr[i+1].parent, this.root_mc);
				}
				
				if (i == numObjects - 1 || iPoint.x + currObjDims.x + this.hGap + nextObjDims.x >= this.width - this.rightMargin) {
					
					// Horizontal alignment
					var hSpaceLeft:Number = this.width - this.rightMargin - (iPoint.x + currObjDims.x);
					if (this.defaultHAlign == CENTER) {
						hOffsets_arr.push(hSpaceLeft / 2);
					} else if (this.defaultHAlign == RIGHT) {
						hOffsets_arr.push(hSpaceLeft);
					} else {
						hOffsets_arr.push(0);
					}
					// Used in vertical Alignment
					rowHeights_arr.push(tallestInRow + vGap);
										
					rowIndices_arr.push(i);
					iPoint.x = this.leftMargin;
					iPoint.y += tallestInRow + this.vGap;
					tallestInRow = 0;
				} else {
					iPoint.x += currObjDims.x + this.hGap;
				}
			}
			
			var row:int = 0;
			for (i=0; i<numObjects; i++) {
				
				// Consider the difference between the xMin, yMin and 0,0 for the iObj
				var iObjBounds:Rectangle = this.objects_arr[i].getBounds(this.objects_arr[i].parent);				
				var topLeftOffset:Point = new Point(this.objects_arr[i].x - iObjBounds.topLeft.x, this.objects_arr[i].y - iObjBounds.topLeft.y);
				topLeftOffset = CoordMath.convertDistance(topLeftOffset, this.objects_arr[i].parent, root_mc);
				positions_arr[i].x += topLeftOffset.x;
				positions_arr[i].y += topLeftOffset.y;
				
				positions_arr[i].x += this.x;
				positions_arr[i].y += this.y;
				
				// Account for horizontal alignment.
				positions_arr[i].x += hOffsets_arr[row];
				
				// Account for vertical alignment.
				if (this.defaultVAlign == MIDDLE) {
					positions_arr[i].y += rowHeights_arr[row] / 2 - this.objects_arr[i].height / 2;
				} else if (this.defaultVAlign == BOTTOM) {
					positions_arr[i].y += rowHeights_arr[row] - this.objects_arr[i].height;
				}				
				
				positions_arr[i] = CoordMath.convertCoords(positions_arr[i], this.root_mc, this.objects_arr[i].parent);
				this.moveObject(this.objects_arr[i], positions_arr[i].x, positions_arr[i].y);
				if (i >= rowIndices_arr[row]) row++;
			}
		}
		protected function moveObject(objToMove:DisplayObject, newX:Number, newY:Number):void {
			objToMove.x = newX;
			objToMove.y = newY
		}
		public function get defaultHAlign():Number {
			return this._defaultHAlign;
		}
		public function set defaultHAlign(newHAlign:Number):void {
			if (this._defaultHAlign != newHAlign) {
				this._defaultHAlign = newHAlign;
				this.invalidate();
			}
		}
		public function get defaultVAlign():Number {
			return this._defaultVAlign;
		}
		public function set defaultVAlign(newVAlign:Number):void {
			if (this._defaultVAlign != newVAlign) {
				this._defaultVAlign = newVAlign;
				this.invalidate();
			}
		}
		public function get hGap():Number {
			return this._hGap;
		}
		public function set hGap(newHGap:Number):void {	
			if (!isNaN(newHGap)) {
				this._hGap = newHGap;
				this.invalidate();
			}
		}
		
		public function get vGap():Number {
			return this._vGap;
		}
		public function set vGap(newVGap:Number):void {
			if (!isNaN(newVGap)) {
				this._vGap = newVGap;
				this.invalidate();
			}
		}
	}	
}
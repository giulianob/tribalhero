/*
	Copyright (c) 2006, 2007 Nicholas Bilyk
*/
package nbilyk.gui.layoutManagers {

	import flash.display.DisplayObject;
	import flash.geom.Point;
	import flash.geom.Rectangle;
	import nbilyk.utils.CoordMath;

	public class GridLayout_3_0 extends LayoutManager_3_0 {
		private var _hGap:int = 5;
		private var _vGap:int = 5;
		private var _rows:int;
		private var _cols:int;
		protected var fixedSizesAreInvalid:Boolean = true;
		protected var rowSizeInputs_arr:Array = [];
		protected var colSizeInputs_arr:Array = [];
		protected var rowSizes_arr:Array;
		protected var colSizes_arr:Array;
		private var _useFixedSizes:Boolean = false;

		private var hAligns_arr:Array;
		private var vAligns_arr:Array;
		private var _defaultHAlign:int = 0;
		private var _defaultVAlign:int = 0;
		public static var LEFT:int = 0;
		public static var CENTER:int = 1;
		public static var RIGHT:int = 2;
		public static var TOP:int = 0;
		public static var MIDDLE:int = 1;
		public static var BOTTOM:int = 2;

		function GridLayout_3_0(root_mc:DisplayObject, rows:int = 2, cols:int = 2) {
			super(root_mc);
			this.rowSizes_arr = new Array();
			this.colSizes_arr = new Array();
			this.hAligns_arr = new Array();
			this.vAligns_arr = new Array();
			this._rows = rows;
			this._cols = cols;
		}
		protected override function draw():void {
			super.draw();
			if (this.useFixedSizes && this.fixedSizesAreInvalid) {
				// Calculate fixed sizes. (sets according to available width and height)
				this.interpretRowSizes();
				this.interpretColSizes();
				this.fixedSizesAreInvalid = false;
			}
			// Calculates variable sizes based on objects_arr elements' width and height properties.
			if (!this.useFixedSizes) this.calculateVariableSizes();
			
			// Calculate positioning.
			var positions_arr:Array = new Array(this.objects_arr.length);
			var counter:int = 0;
			var currY:Number = this.topMargin;
			for (var r:int=0; r<this._rows; r++) {
				var currX:Number = this.leftMargin;
				for (var c:int=0; c<this._cols; c++) {
					if (this.objects_arr[counter] == undefined) {
						counter++;
						continue;
					}
					// Horizontal alignment.
					var hAlign:int = this._defaultHAlign;
					if (!isNaN(this.hAligns_arr[c])) hAlign = this.hAligns_arr[c];
					
					var currObjDims:Point = new Point(this.objects_arr[counter].width, this.objects_arr[counter].height);
					currObjDims = CoordMath.convertDistance(currObjDims, this.objects_arr[counter].parent, this.root_mc);
					
					var xOffset:Number;
					if (hAlign == CENTER) {
						xOffset = this.colSizes_arr[c]/2 - currObjDims.x/2 - this._hGap/2;
					} else if (hAlign == RIGHT) {
						xOffset = this.colSizes_arr[c] - currObjDims.x - this._hGap;
					} else {
						// LEFT
						xOffset = 0;
					}
					// Vertical alignment.
					var vAlign:int = this._defaultVAlign;
					if (!isNaN(this.vAligns_arr[r])) vAlign = this.vAligns_arr[r];
					var yOffset:Number;
					if (vAlign == MIDDLE) {
						yOffset = this.rowSizes_arr[r]/2 - currObjDims.y/2 - this._vGap/2;
					} else if (vAlign == BOTTOM) {
						yOffset = this.rowSizes_arr[r] - currObjDims.y - this._vGap;
					} else {
						// TOP
						yOffset = 0;
					}
					positions_arr[counter] = new Point(currX+xOffset, currY+yOffset);
					currX += this.colSizes_arr[c];
					counter++;
				}
				currY += this.rowSizes_arr[r];
			}
			var positionsL:int = positions_arr.length;
			for (var i:int=0; i<positionsL; i++) {
				if (i > this._rows * this._cols - 1) {
					// There are more objects than what can fit in the given rows/cols
					break; 
				}
				if (this.objects_arr[i] == undefined || positions_arr[i] == undefined) continue;
				
				// Consider the difference between the xMin, yMin and 0,0 for the iObj
				var iObjBounds:Rectangle = this.objects_arr[i].getBounds(this.objects_arr[i].parent);				
				var topLeftOffset:Point = new Point(this.objects_arr[i].x - iObjBounds.topLeft.x, this.objects_arr[i].y - iObjBounds.topLeft.y);
				topLeftOffset = CoordMath.convertDistance(topLeftOffset, this.objects_arr[i].parent, root_mc);
				positions_arr[i].x += topLeftOffset.x;
				positions_arr[i].y += topLeftOffset.y;
				
				positions_arr[i].x += this.x;
				positions_arr[i].y += this.y;
				
				positions_arr[i] = CoordMath.convertCoords(positions_arr[i], this.root_mc, this.objects_arr[i].parent);
				this.moveObject(this.objects_arr[i], positions_arr[i].x, positions_arr[i].y);
			}
		}
		protected function moveObject(objToMove:DisplayObject, newX:Number, newY:Number):void {
			objToMove.x = newX;
			objToMove.y = newY;
		}
		protected function calculateVariableSizes():void {
			this.colSizes_arr = new Array(this._cols);
			var c:int;
			var r:int;
			var objectIndex:int;
			var currObjDims:Point;
			
			// appropriate widths
			var colSizeInputsL:int = this.colSizeInputs_arr.length;
			for (c=0; c<this._cols; c++) {
				if (c < colSizeInputsL && this.colSizeInputs_arr[c] != null) {
					this.colSizes_arr[c] = this.colInputToValue(this.colSizeInputs_arr[c]);
				}
				if (c >= colSizeInputsL || isNaN(this.colSizes_arr[c])) {
					var maxWidth:Number = 0;
					for (r=0; r<this._rows; r++) {
						objectIndex = c+r*this._cols;
						if (this.objects_arr[objectIndex] == undefined) continue;
						currObjDims = new Point(this.objects_arr[objectIndex].width, 0);
						currObjDims = CoordMath.convertDistance(currObjDims, this.objects_arr[objectIndex].parent, this.root_mc);
						if (currObjDims.x > maxWidth) {
							maxWidth = currObjDims.x;
						}
					}
					this.colSizes_arr[c] = maxWidth + this._hGap;
				}
			}
			// appropriate heights
			var rowSizeInputsL:int = this.rowSizeInputs_arr.length;
			this.rowSizes_arr = new Array(this._rows);
			for (r=0; r<this._rows; r++) {
				if (r < rowSizeInputsL && this.rowSizeInputs_arr[r] != null) {
					this.rowSizes_arr[r] = this.rowInputToValue(this.rowSizeInputs_arr[r]);
				}
				if (r >= rowSizeInputsL || isNaN(this.rowSizes_arr[r])) {
					var maxHeight: int = 0;
					for (c=0; c<this._cols; c++) {
						objectIndex = c+r*this._cols;
						if (this.objects_arr[objectIndex] == undefined) continue;
						currObjDims = new Point(0, this.objects_arr[objectIndex].height);
						currObjDims = CoordMath.convertDistance(currObjDims, this.objects_arr[objectIndex].parent, this.root_mc);
						if (currObjDims.y > maxHeight) {
							maxHeight = currObjDims.y;
						}
					}
					this.rowSizes_arr[r] = maxHeight + this._vGap;
				}
			}
		}
		public function getRowSizes():Array {
			return this.rowSizeInputs_arr;
		}
		public function setRowSizes(... args):void {
			if (args[0] is Array) {
				this.rowSizeInputs_arr = args[0];
			} else if (args.length > 0) {
				this.rowSizeInputs_arr = args;
			}
			if (args.length == 1) {
				var fillAllWith:* = this.rowSizeInputs_arr[0];
				var numRows:int = this._rows;
				for (var i:int=0; i<numRows; i++) {
					this.rowSizeInputs_arr[i] = fillAllWith;
				}
			}
			this.fixedSizesAreInvalid = true;
			this.invalidate();
		}
		protected function interpretRowSizes():void {
			var totalSize:Number = 0;
			var undecidedRows_arr:Array = new Array();
			var numRows:int = this._rows;
			this.rowSizes_arr = new Array(numRows);
			var rowSizeInputsL:int = this.rowSizeInputs_arr.length;
			for (var i:int=0; i<numRows; i++) {
				var input:*;
				if (i >= rowSizeInputsL) input = null;
				else input = this.rowSizeInputs_arr[i];
				this.rowSizes_arr[i] = this.rowInputToValue(input);
				
				if (isNaN(this.rowSizes_arr[i])) {
					// Ration a share of the remaining space with this row.
					undecidedRows_arr.push(i);
				} else {
					// Good input.
					totalSize += this.rowSizes_arr[i];
				}
			}
			
			var undecidedRowsL:int = undecidedRows_arr.length;
			var splitUpSize:Number = (this.height - totalSize - this.topMargin - this.bottomMargin) / undecidedRowsL;
			for (var j:int=0; j<undecidedRowsL; j++) {
				this.rowSizes_arr[undecidedRows_arr[j]] = splitUpSize;
			}
		}
		protected function rowInputToValue(input:*):Number {
			if (input == null) {
			} else if (input is Number) {
				return input;
			} else if (input is String) {
				var percent:Number = parseInt(input);
				return (this.height - this.topMargin - this.bottomMargin) * (percent / 100);
			}
			return NaN;
		}
		public function getColSizes():Array {
			return this.colSizeInputs_arr;
		}
		public function setColSizes(... args):void {
			if (args[0] is Array) {
				this.colSizeInputs_arr = args[0];
			} else if (args.length > 0) {
				this.colSizeInputs_arr = args;
			}
			if (args.length == 1) {
				var fillAllWith:* = this.colSizeInputs_arr[0];
				var numCols:int = this._cols;
				for (var i:int=0; i<numCols; i++) {
					this.colSizeInputs_arr[i] = fillAllWith;
				}
			}
			this.fixedSizesAreInvalid = true;
			this.invalidate();
		}
		protected function interpretColSizes():void {
			var totalSize:Number = 0;
			var undecidedCols_arr:Array = new Array();
			var numCols:int = this._cols;
			this.colSizes_arr = new Array(numCols);
			var colSizeInputsL:int = this.colSizeInputs_arr.length;
			for (var i:int=0; i<numCols; i++) {
				var input:*;
				if (i >= colSizeInputsL) input = null;
				else input = this.colSizeInputs_arr[i];
				this.colSizes_arr[i] = this.colInputToValue(input);
				
				if (isNaN(this.colSizes_arr[i])) {
					// Ration a share of the remaining space with this column.
					undecidedCols_arr.push(i);
				} else {
					// Good input.
					totalSize += this.colSizes_arr[i];
				}
			}
			
			var undecidedColsL:int = undecidedCols_arr.length;
			var splitUpSize:Number = (this.width - totalSize - this.leftMargin - this.rightMargin) / undecidedColsL;
			for (var j:int=0; j<undecidedColsL; j++) {
				this.colSizes_arr[undecidedCols_arr[j]] = splitUpSize;
			}
		}
		protected function colInputToValue(input:*):Number {
			if (input == null) {
			} else if (input is Number) {
				return input;
			} else if (input is String) {
				var percent:Number = parseInt(input);
				return (this.width - this.leftMargin - this.rightMargin) * (percent / 100);
			}
			return NaN;
		}
		public function get useFixedSizes():Boolean {
			return this._useFixedSizes;
		}
		public function set useFixedSizes(newUseFixedSizes:Boolean):void {
			this._useFixedSizes = newUseFixedSizes;
			this.invalidate();
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
		public function get rows():Number {
			return this._rows;
		}
		public function set rows(newRows:Number):void {
			if (!isNaN(newRows)) {
				this._rows = newRows;
				this.fixedSizesAreInvalid = true;
				this.invalidate();
			}
		}
		public function get cols():Number {
			return this._cols;
		}
		public function set cols(newCols:Number):void {
			if (!isNaN(newCols)) {
				this._cols = newCols;
				this.fixedSizesAreInvalid = true;
				this.invalidate();
			}
		}
		public function get defaultHAlign():int {
			return this._defaultHAlign;
		}
		public function set defaultHAlign(newHAlign:int):void {
			if (newHAlign != this._defaultHAlign) {
				this._defaultHAlign = newHAlign;
				this.invalidate();
			}
		}
		public function getHAligns():Array {
			return this.hAligns_arr;
		}
		public function setHAligns(... hAligns):void {
			// usage 2: align1, align2, align3, etc
			this.hAligns_arr = new Array(this._cols);
			for (var i:int=0; i<this._cols; i++) {
				if (hAligns[i] is Number || hAligns[i] == undefined) {
					this.hAligns_arr[i] = hAligns[i];
				} else {
					throw new ArgumentError("Argument " + i + " is not a Number");
				}
			}
			this.invalidate();
		}
		public function get defaultVAlign():int {
			return this._defaultVAlign;
		}
		public function set defaultVAlign(newVAlign:int):void {
			if (newVAlign != this._defaultVAlign) {
				this._defaultVAlign = newVAlign;
				this.invalidate();
			}
		}
		public function getVAligns():Array {
			return this.vAligns_arr;
		}
		public function setVAligns(... vAligns):void {
			// usage 2: valign1, valign2, valign3, etc
			this.vAligns_arr = new Array(this._rows);
			for (var i:int=0; i<this._rows; i++) {
				if (vAligns[i] is Number || vAligns[i] == undefined) {
					this.vAligns_arr[i] = vAligns[i];
				} else {
					throw new ArgumentError("Argument " + i + " is not a Number");
				}
			}
			this.invalidate();
		}
		public function getObjByRowCol(row:int, col:int):DisplayObject {
			var objectIndex:int = col+row*this._cols;
			return DisplayObject(this.objects_arr[objectIndex]);
		}
		public function setObjByRowCol(row:int, col:int, newObj:DisplayObject):void {
			if (this._rows < row+1) {
				this._rows = row+1;
			}
			if (this._cols < col+1) {
				this._cols = col+1;
			}
			var objectIndex:int = col+row*this._cols;
			this.objects_arr[objectIndex] = newObj;
			this.invalidate();
		}
	}
}
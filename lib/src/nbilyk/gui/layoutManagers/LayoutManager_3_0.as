/*
	Copyright (c) 2006, 2007 Nicholas Bilyk
*/

package nbilyk.gui.layoutManagers {
	
	import flash.display.DisplayObject;
	import flash.geom.Point;
	import flash.geom.Rectangle;
	import nbilyk.gui.drawing.IDraw;
	import nbilyk.utils.CoordMath;
	import nbilyk.utils.FunctionLimiter;
	
	public class LayoutManager_3_0 implements IDraw {
		protected var root_mc:DisplayObject;
		protected var objects_arr:Array;
		private var _x:Number;
		private var _y:Number;
		private var _width:Number;
		private var _height:Number;
		private var _leftMargin:Number;
		private var _rightMargin:Number;
		private var _topMargin:Number;
		private var _bottomMargin:Number;
		private var drawIntervalId:uint;
	
		public function LayoutManager_3_0(root_mc:DisplayObject ) {
			this.root_mc = root_mc;
			this.objects_arr = new Array();
			
			// Defaults
			var defaultBounds:Rectangle = root_mc.getBounds(this.root_mc);
			this._width = defaultBounds.width; 
			this._height = defaultBounds.height; 
			this._x = defaultBounds.topLeft.x; 
			this._y = defaultBounds.topLeft.y; 			
			
			this._leftMargin = 5;
			this._rightMargin = 5;
			this._topMargin = 5;
			this._bottomMargin = 5;
		}
		// IDraw methods.
		protected function draw():void {}
		public function drawNow():void {
			this.draw();
		}
		public function invalidate():void {
			var allowPass:Boolean = FunctionLimiter.limit(this.invalidate);
			if (!allowPass) return;
			this.draw();
		}
		
		public function addObj(... objs):void {
			var numObjs:int = objs.length;
			for (var i:int = 0; i<numObjs; i++) {
				if (objs[i] is DisplayObject) {
					this.addObjAt(objs[i], this.objects_arr.length);
				}
			}
		}
		public function addObjAt(objToAdd:DisplayObject, index:Number):void {
			this.objects_arr.splice(index, 0, objToAdd);
			this.invalidate();
		}
		public function objExists(objToCheck:DisplayObject):Boolean {
			return this.objects_arr.indexOf(objToCheck) != -1;
		}
		public function indexOfObj(objToCheck:DisplayObject):int {
			return this.objects_arr.indexOf(objToCheck);
		}
		public function lastIndexOfObj(objToCheck:DisplayObject):int {
			return this.objects_arr.lastIndexOf(objToCheck);
		}
		public function removeObj(objToRemove:Object):Boolean {
			var index:Number = this.objects_arr.indexOf(objToRemove);
			if (index == -1) {
				return false;
			} else {
				this.objects_arr.splice(index, 1);
				this.invalidate();
				return true;
			}
		}
		public function removeAllObjs():void {
			this.objects_arr = new Array();
			this.invalidate();
		}
		public function get leftMargin():Number { 
			return this._leftMargin; 
		}
		public function set leftMargin(newLeftMargin:Number):void {
			if (!isNaN(newLeftMargin)) {
				this._leftMargin = newLeftMargin;
				this.invalidate();
			}
		}
		
		public function get rightMargin():Number { 
			return this._rightMargin; 
		}
		public function set rightMargin(newRightMargin:Number):void {
			if (!isNaN(newRightMargin)) {
				this._rightMargin = newRightMargin;
				this.invalidate();
			}
		}
		
		public function get topMargin():Number { 
			return this._topMargin; 
		}
		public function set topMargin(newTopMargin:Number):void {
			if (!isNaN(newTopMargin)) {
				this._topMargin = newTopMargin;
				this.invalidate();
			}
		}
		
		public function get bottomMargin():Number { 
			return this._bottomMargin; 
		}	
		public function set bottomMargin(newBottomMargin:Number):void {
			if (!isNaN(newBottomMargin)) {
				this._bottomMargin = newBottomMargin;
				this.invalidate();
			}
		}
		
		public function get x():Number { 
			return this._x; 
		}
		public function set x(newX:Number):void {
			this._x = newX;
			this.invalidate();
		}
		
		public function get y():Number { 
			return this._y; 
		}
		public function set y(newY:Number):void {
			this._y = newY;
			this.invalidate();
		}
		
		public function get height():Number { 
			return this._height; 
		}
		public function set height(newHeight:Number):void {
			if (newHeight > 0) {
				this._height = newHeight;
				this.invalidate();
			}
		}
		
		public function get width():Number { 
			return this._width; 
		}
		public function set width(newWidth:Number):void {
			if (newWidth > 0) {
				this._width = newWidth;
				this.invalidate();
			}
		}
		
		public function getObj(itemNum:Number):DisplayObject {
			return this.objects_arr[itemNum];
		}
		public function setObj(itemIndex:int, new_obj:DisplayObject):void {
			this.objects_arr.splice(itemIndex, 0, new_obj);
		}
		
		public function get numItems():int {
			var returnNum:int = this.objects_arr.length;
			while (returnNum && (this.objects_arr[returnNum-1] == undefined)) {
				returnNum--;
			}
			return returnNum;
		}
				
		public function get rightEdge():Number {
			// Inaccurate unless called after draw() has been invoked. 
			var objectsL:Number = this.objects_arr.length;
			var rightMostPoint:Point = new Point(Number.MIN_VALUE, 0);
			var currPoint:Point;
			for (var i:int=0; i<objectsL; i++) {
				var w:Number = this.objects_arr[i].width;
				currPoint = new Point(this.objects_arr[i].x + w, this.objects_arr[i].y);
				currPoint = CoordMath.convertCoords(currPoint, this.objects_arr[i].parent, this.root_mc);
				if (currPoint.x > rightMostPoint.x) {
					rightMostPoint = currPoint;
				}
			}
			return rightMostPoint.x;
		}
		public function get bottomEdge():Number {
			// Inaccurate unless called after draw() has been invoked.
			var objectsL:Number = this.objects_arr.length;
			var lowestPoint:Point = new Point(0, Number.MIN_VALUE);
			var currPoint:Point;
			for (var i:int=0; i<objectsL; i++) {
				var h:Number = this.objects_arr[i].height;
				currPoint = new Point(this.objects_arr[i].x, this.objects_arr[i].y + h);
				currPoint = CoordMath.convertCoords(currPoint, this.objects_arr[i].parent, this.root_mc);
				if (currPoint.y > lowestPoint.y) {
					lowestPoint = currPoint;
				}
			}
			return lowestPoint.y;
		}
		public function removeInvalidObjects():void {
			// Removes all undefined objects from objects_arr.
			var numObjects:Number = this.objects_arr.length;
			for (var i:int=0; i<numObjects; i++) {
				if (this.objects_arr[i] == undefined) {
					this.objects_arr.splice(i,1);
					i--; numObjects--;
					continue;
				}
			}
		}
	}
}
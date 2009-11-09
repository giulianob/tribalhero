/*
	Copyright (c) 2006, 2007 Nicholas Bilyk
*/

package nbilyk.gui.layoutManagers {
	
	import flash.display.DisplayObject;
	import fl.transitions.Tween;
	import fl.transitions.easing.Regular;
	
	public class AnimatedFlowLayout_3_0 extends FlowLayout_3_0 {
		public var steps:Number = 7;
		public var animationOn:Boolean = true;
		private var tweenQueue:Array;
		
		function AnimatedFlowLayout_3_0(root_mc:DisplayObject) {
			super(root_mc);
		}
		override protected function moveObject(objToMove:DisplayObject, newX:Number, newY:Number):void {
			if (this.animationOn) {
				var xTween:Tween = new Tween(objToMove, "x", Regular.easeOut, objToMove.x, newX, this.steps, false);
				this.tweenQueue.push(xTween);
				var yTween:Tween = new Tween(objToMove, "y", Regular.easeOut, objToMove.y, newY, this.steps, false);
				this.tweenQueue.push(yTween);
			} else {
				super.moveObject(objToMove, newX, newY);
			}
		}
		override protected function draw():void {
			this.tweenQueue = new Array();  // This is to enforce a strong reference to the Tween objects so the garbage collector doesn't get overzealous.
			super.draw();
		}
	}
}
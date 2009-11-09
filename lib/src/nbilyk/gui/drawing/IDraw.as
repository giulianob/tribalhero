/*
	Copyright (c) 2006, 2007 Nicholas Bilyk
*/

package nbilyk.gui.drawing {
	public interface IDraw {
		function drawNow():void;
		function invalidate():void;
	}
}


/*

	import nbilyk.utils.FunctionLimiter;
	
	// IDraw methods.
	function draw():void {}
	public function drawNow():void {
		this.draw();
	}
	public function invalidate():void {
		var allowPass:Boolean = FunctionLimiter.limit(this.invalidate);
		if (!allowPass) return;
		this.draw();
	}
	
*/
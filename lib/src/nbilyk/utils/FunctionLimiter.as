/*
	Copyright (c) 2006, 2007 Nicholas Bilyk
*/

package nbilyk.utils {
	import flash.utils.Timer;
	import flash.events.Event;
	import flash.events.TimerEvent;
	
	public class FunctionLimiter {
		private static var functions_arr:Array = [];
		private static var timers_arr:Array = [];
		private static var args_arr:Array = [];
		
		public static function limit(func:Function, time:Number = 20, args:Array = null):Boolean {			
			var existingIndex:int = FunctionLimiter.functions_arr.indexOf(func);
			if (existingIndex != -1) {
				return FunctionLimiter.checkIfTimerFinished(existingIndex);
			}
			
			// Time limiter doesn't exist, create a new timer.
			var myTimer:Timer = new Timer(time, 1);
			myTimer.addEventListener(TimerEvent.TIMER, FunctionLimiter.timerHandler);
			FunctionLimiter.timers_arr.push(myTimer);
			FunctionLimiter.functions_arr.push(func);
			FunctionLimiter.args_arr.push(args);
			myTimer.start();
			return false;
		}
		public static function limitFrames(func:Function, frames:int = 1, args:Array = null):Boolean {			
			var existingIndex:int = FunctionLimiter.functions_arr.indexOf(func);
			if (existingIndex != -1) {
				return FunctionLimiter.checkIfTimerFinished(existingIndex);
			}
			
			// Time limiter doesn't exist, create a new frame timer.
			var myTimer_mc:TimerClip = new TimerClip();
			myTimer_mc.frameCount = frames;
			myTimer_mc.addEventListener(Event.ENTER_FRAME, FunctionLimiter.framesHandler);
			FunctionLimiter.timers_arr.push(myTimer_mc);
			FunctionLimiter.functions_arr.push(func);
			FunctionLimiter.args_arr.push(args);
			return false;
		}
		private static function timerHandler(evt:TimerEvent):void {
			var existingIndex:int = FunctionLimiter.timers_arr.indexOf(evt.currentTarget);
			if (existingIndex == -1) throw Error("Timer not found.");
			
			FunctionLimiter.timers_arr[existingIndex] = null;
			FunctionLimiter.functions_arr[existingIndex].apply(null, FunctionLimiter.args_arr[existingIndex]);
		}
		private static function framesHandler(evt:Event):void {
			var currentTimer:TimerClip = TimerClip(evt.currentTarget);
			var existingIndex:int = FunctionLimiter.timers_arr.indexOf(currentTimer);
			if (existingIndex == -1) throw Error("Timer not found.");
			
			currentTimer.frameCount--;
			if (currentTimer.frameCount <= 0) {
				currentTimer.removeEventListener(Event.ENTER_FRAME, FunctionLimiter.framesHandler);
				FunctionLimiter.timers_arr[existingIndex] = null;
				FunctionLimiter.functions_arr[existingIndex].apply(null, FunctionLimiter.args_arr[existingIndex]);
			}
		}
		private static function checkIfTimerFinished(existingIndex:int): Boolean {
			if (FunctionLimiter.timers_arr[existingIndex] == null) {
				// Timer has completed.
				FunctionLimiter.timers_arr.splice(existingIndex, 1);
				FunctionLimiter.functions_arr.splice(existingIndex, 1);
				FunctionLimiter.args_arr.splice(existingIndex, 1);
				return true;
			} else {
				return false;
			}
		}
		public static function interrupt(func:Function):Boolean {
			var existingIndex:int = FunctionLimiter.functions_arr.indexOf(func);
			if (existingIndex != -1) {
				if (FunctionLimiter.timers_arr[existingIndex] is Timer) {
					FunctionLimiter.timers_arr[existingIndex].stop();
				} else if (FunctionLimiter.timers_arr[existingIndex] is TimerClip) {
					FunctionLimiter.timers_arr[existingIndex].removeEventListener(Event.ENTER_FRAME, FunctionLimiter.framesHandler);
				}
				FunctionLimiter.timers_arr.splice(existingIndex, 1);
				FunctionLimiter.functions_arr.splice(existingIndex, 1);
				FunctionLimiter.args_arr.splice(existingIndex, 1);
				return true;
			} else {
				return false;
			}
		}
	}
}
import flash.display.Sprite;
class TimerClip extends Sprite {
	public var frameCount:int;
}
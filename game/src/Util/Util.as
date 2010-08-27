/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Util {
	import fl.motion.AdjustColor;
	import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
	import flash.filters.ColorMatrixFilter;
	import flash.geom.Rectangle;
	import flash.utils.getQualifiedClassName;
	import org.aswing.Component;
	import org.aswing.Container;
	import org.aswing.event.*;
	import org.aswing.geom.IntPoint;
	import org.aswing.JFrame;
	import org.aswing.AsWingUtils;

	public class Util {

		public static function centerSprite(obj: DisplayObjectContainer) : void {
			var item: DisplayObject;
			for (var i: int = 0; i < obj.numChildren; i++)
			{
				item = obj.getChildAt(i);
				var rect: Rectangle = item.getRect(item);
				item.x -= rect.x;
				item.y -= rect.y;
			}
		}

		public static function getFrame(component: Component) : JFrame {
			return getFrameEx(component) as JFrame;
		}

		private static function getFrameEx(component: Component) : Container {
			return component is JFrame ? component as JFrame : getFrameEx(component.getParent());
		}

		public static function dumpDisplayObject(obj: DisplayObject, depth: int = 0):void {
			if (depth == 0) trace("===========================");

			var spacer: String = "";
			for (var j: int = 0; j < depth; j++) {
				spacer += "\t";
			}

			trace(spacer + "(" + getQualifiedClassName(obj) + ") " + obj.name + (!obj.visible ? "<invisible>" : ""));

			if (!(obj is DisplayObjectContainer)) return;

			var container: DisplayObjectContainer = obj as DisplayObjectContainer;

			for (var i: int = 0; i < container.numChildren; i++)
			{
				var child: DisplayObject = container.getChildAt(i);

				dumpDisplayObject(child, depth + 1);
			}
		}

		public static function centerFrame(frame: JFrame ):void {
			var location:IntPoint = AsWingUtils.getScreenCenterPosition();
			location.x = Math.round(location.x - frame.getWidth()/2);
			location.y = Math.round(location.y - frame.getHeight()/2);
			frame.setLocation(location);
		}

		public static function binarySearch(array: Array, compare: Function, value: *): int
		{
			return binarySearchAlg(array, compare, value, 0, array.length - 1);
		}

		private static function binarySearchAlg(array: Array, compare: Function, value: *, low: int, high: int): int
		{
			while (low <= high) {
				var mid: int = (low + high) / 2;

				var returnCompare: int = compare(array[mid], value);

				if (returnCompare > 0)
				high = mid - 1;
				else if (returnCompare < 0)
				low = mid + 1;
				else
				return mid;
			}

			return -1;
		}

		public static function binarySearchRange(array: Array, compare: Function, value: *): Array
		{
			var idx: int = binarySearchAlg(array, compare, value, 0, array.length - 1);

			var ret: Array = new Array();

			if (idx == -1)
			return ret;

			ret.push(idx);

			var i: int;
			for (i = idx - 1; i >= 0; i--)
			{
				if (compare(array[i], value) == 0)
				ret.push(i);
				else
				break;
			}

			ret = ret.reverse();

			for (i = idx + 1; i < array.length; i++)
			{
				if (compare(array[i], value) == 0)
				ret.push(i);
				else
				break;
			}

			return ret;
		}

		public static function formatTime(time: int): String
		{
			if (time <= 0) return "--:--:--";

			var hours: int = int(time / (60 * 60));
			time -= hours * 60 * 60;
			var minutes: int = int(time / 60);
			time -= minutes * 60;
			var seconds: int = time;

			return (hours <= 9 ? "0" + hours : hours) + ":" + (minutes <= 9 ? "0" + minutes : minutes) + ":" + (seconds <= 9 ? "0" + seconds : seconds);
		}

		public static function niceTime(time: int): String
		{
			if (time < 60)
			return "less than a minute";

			var hours: int = int(time / (60 * 60));
			time -= hours * 60 * 60;
			var minutes: int = int(time / 60);
			time -= minutes * 60;
			var seconds: int = time;

			if (seconds > 30) //add 1 minute if seconds is greater than 30
			minutes++;

			var nice: String = "";

			if (hours > 0) {
				if (hours > 1)
				nice += hours + " hours";
				else
				nice += "an hour";

				if (minutes > 0)
				nice += " and ";
			}

			if (minutes > 1)
			nice += minutes + " minutes";
			else if (minutes == 1)
			nice += "a minute";

			return nice;
		}

		private static var filterBW: Array;

		public static function setGrayed(obj: DisplayObject, gray: Boolean) : void {

			if (filterBW == null) {
				var color : AdjustColor;
				var colorMatrix : ColorMatrixFilter;
				var matrix : Array;
				var filterBW : Array;

				color = new AdjustColor();
				color.brightness = 0;
				color.contrast = 0;
				color.hue = 0;
				color.saturation = -100;

				matrix = color.CalculateFinalFlatArray();
				colorMatrix = new ColorMatrixFilter(matrix);
				filterBW = [colorMatrix];
			}

			if (gray) {
				obj.filters = filterBW;
			} else {
				obj.filters = [];
			}
		}
		
		public static function implode(glue: String, arr: Array) : String {
			var s: String = new String();
			for (var i: int = 0; i < arr.length; i++) {
				s += arr[i];
				if ( i != arr.length - 1 ) s += glue;
			}
			
			return s;
		}

	}

}


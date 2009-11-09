/*
	Copyright (c) 2006, 2007 Nicholas Bilyk
*/

package nbilyk.utils {

	import flash.display.DisplayObject;
	import flash.geom.Point;
	
	public class CoordMath {	
	
		public static function convertCoords(point:Point, from_mc:DisplayObject, to_mc:DisplayObject):Point {
			if (to_mc == null || from_mc == null) return point.clone();
			point = from_mc.localToGlobal(point);
			point = to_mc.globalToLocal(point);
			return point;
		}
		public static function convertDistance (distance:Point, from_mc:DisplayObject, to_mc:DisplayObject):Point {
			if (to_mc == null || from_mc == null) return distance.clone();
			var topLeft:Point = new Point(0,0);
			topLeft = CoordMath.convertCoords(topLeft, from_mc, to_mc);
			distance = CoordMath.convertCoords(distance, from_mc, to_mc);
			distance.x -= topLeft.x;
			distance.y -= topLeft.y;	
			return distance;
		}
	}
}
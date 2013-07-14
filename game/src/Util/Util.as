/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Util {
    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.Stage;
    import flash.external.ExternalInterface;
    import flash.geom.Rectangle;
    import flash.utils.getQualifiedClassName;

    import mx.utils.StringUtil;

    import org.aswing.AsWingConstants;
    import org.aswing.AsWingManager;
    import org.aswing.AsWingUtils;
    import org.aswing.Component;
    import org.aswing.Container;
    import org.aswing.FocusManager;
    import org.aswing.JFrame;
    import org.aswing.JPanel;
    import org.aswing.JScrollPane;
    import org.aswing.JTextComponent;
    import org.aswing.JViewport;
    import org.aswing.geom.IntPoint;

    import src.Constants;
    import src.UI.GameJImagePanelBackground;

    public class Util {
               
		public static function createTopAlignedScrollPane(pnl: JPanel): JScrollPane {
			var scrollPane: JScrollPane = new JScrollPane(new JViewport(pnl, true), JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_AS_NEEDED);
			(scrollPane.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);

			return scrollPane;
		}
		
		public static function log(msg: String) : void {					
			
            try {
                ExternalInterface.call("console.log", msg);                    
            }
            catch (e: Error) {                    
            }            
			
            trace(msg);
		}
		
		public static function textfieldHasFocus(stage: Stage) : Boolean {
			var focusOwner: Component = FocusManager.getManager(stage).getFocusOwner();
			if (!focusOwner) return false;
			return focusOwner is JTextComponent;	
		}
		
		public static function centerSprite(obj: DisplayObjectContainer) : void {
			var item: DisplayObject;
			
			for (var i: int = 0; i < obj.numChildren; i++)
			{
				item = obj.getChildAt(i);
				var rect: Rectangle = item.getRect(obj);
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
			if (depth == 0) Util.log("===========================");

			var spacer: String = "";
			for (var j: int = 0; j < depth; j++) {
				spacer += "\t";
			}

			Util.log(spacer + "(" + getQualifiedClassName(obj) + ") " + obj.name + (!obj.visible ? "<invisible>" : ""));

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
			location.y = Math.max(0, Math.round(location.y - frame.getHeight()/2));
			frame.setLocation(location);
		}
		
		public static function getMaxGamePanelHeight(top: int = 0): int {			
			return Constants.screenH - GameJImagePanelBackground.getFrameHeight() - top;
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

			return ~low;
		}

		public static function binarySearchRange(array: Array, compare: Function, value: *): Array
		{
			var idx: int = binarySearchAlg(array, compare, value, 0, array.length - 1);

			var ret: Array = [];

			if (idx <= -1) return ret;

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

		public static function implode(glue: String, arr: Array) : String {
			var s: String = '';
			for (var i: int = 0; i < arr.length; i++) {
				s += arr[i];
				if ( i != arr.length - 1 ) s += glue;
			}

			return s;
		}
		
		public static function roundNumber(number: Number, digit: int = 1) : Number {
			return Math.round(number * Math.pow(10, digit)) / Math.pow(10, digit);
		}
		
		public static function truncateNumber(number: Number, digit: int = 1) : Number {
			return int(number * Math.pow(10, digit)) / Math.pow(10, digit);
		}		

        public static function triggerJavascriptEvent(event: String, ...rest): void {                        
            if (!ExternalInterface.available) {
                return;
            }
            
            var jsVarEncode: Function = function(param: String): String {
                return StringUtil.substitute("\"{0}\"", param.replace("\"", "\\\""));
            };
            
            try {                
                var escapedArgs: Array = [ jsVarEncode(event) ];
                for each (var param: String in rest) {
                    escapedArgs.push(jsVarEncode(param));
                }

                ExternalInterface.call(StringUtil.substitute("$(window).trigger({0})", escapedArgs.join(",")));
            }
            catch (e: Error) {                
            }
        }
    }

}


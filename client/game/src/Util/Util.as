/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Util {
    import feathers.controls.LayoutGroup;
    import feathers.core.FeathersControl;

    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.Stage;
    import flash.events.TimerEvent;
    import flash.external.ExternalInterface;
    import flash.geom.Rectangle;
    import flash.utils.Timer;
    import flash.utils.getQualifiedClassName;

    import mx.utils.StringUtil;

    import org.aswing.AsWingConstants;
    import org.aswing.AsWingUtils;
    import org.aswing.Component;
    import org.aswing.Container;
    import org.aswing.FocusManager;
    import org.aswing.JFrame;
    import org.aswing.JScrollPane;
    import org.aswing.JTextComponent;
    import org.aswing.JViewport;
    import org.aswing.geom.IntDimension;
    import org.aswing.geom.IntPoint;

    import src.Constants;
    import src.Global;
    import src.UI.GameJImagePanelBackground;

    import starling.display.DisplayObject;
    import starling.utils.formatString;

    public class Util {

        public static function callLater(func:Function, time:int=40):void{
            var timer:Timer = new Timer(time, 1);
            timer.addEventListener(TimerEvent.TIMER, function(e:TimerEvent):void{
                func();
            });
            timer.start();
        }

        public static function calculateSize(width: Number, height: Number, targetW: Number, targetH: Number): IntDimension
        {
            if (targetW > width && targetH > height) {
                return new IntDimension(width, height);
            }

            var ratio:Number = width / height;
            var rW:Number = targetW;
            var rH:Number = targetH;
            if (ratio >= 1) {
                rH = targetW / ratio;
                if (rH > targetH) {
                    rH = targetH;
                    rW = rH * ratio;
                }
            }else {
                rW = targetH * ratio;
                if (rW > targetW) {
                    rW = targetW;
                    rH = rW / ratio;
                }
            }

            return new IntDimension(rW, rH);
        }

        public static function resizeSprite(sprite: flash.display.DisplayObject, targetW: Number, targetH: Number): void {
            sprite.scaleX = 1;
            sprite.scaleY = 1;

            var size: IntDimension = Util.calculateSize(sprite.width, sprite.height, targetW, targetH);

            sprite.scaleX = size.width / sprite.width;
            sprite.scaleY = size.height / sprite.height;
        }

		public static function createTopAlignedScrollPane(component: Component): JScrollPane {
			var scrollPane: JScrollPane = new JScrollPane(new JViewport(component, true), JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_AS_NEEDED);
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
			var item: flash.display.DisplayObject;
			
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

        public static function dumpStarlingStage(): void {
            trace("========================================");
            trace("Starling stage:");

            var dumpStarlingDisplayObject: Function = function(obj: starling.display.DisplayObject, depth: int = 0): void {
                var spacer: String = "";
                for (var j: int = 0; j < depth; j++) {
                    spacer += "\t";
                }

                var objDesc: Array = [getQualifiedClassName(obj), formatString("name={0} x={1} y={2} size={3}x{4}", obj.name, obj.x, obj.y, obj.width, obj.height)];

                var feathersControl: FeathersControl = obj as FeathersControl;
                if (feathersControl) {
                    if (feathersControl.layoutData) {
                        for (var prop: String in feathersControl.layoutData) {
                            if (!feathersControl.layoutData.hasOwnProperty(prop)) {
                                continue;
                            }

                            objDesc.push(formatString("layoutData.{0}={1}", prop, feathersControl.layoutData[prop]));
                        }
                    }

                    var layoutGroup: LayoutGroup = obj as LayoutGroup;
                    if (layoutGroup) {
                        objDesc.push(formatString("layout={0}", layoutGroup.layout));
                    }
                }

                var container: starling.display.DisplayObjectContainer = obj as starling.display.DisplayObjectContainer;
                if (container == null) {
                    return;
                }

                trace(spacer + objDesc.join(","));

                for (var i: int = 0; i < container.numChildren; i++)
                {
                    var child: starling.display.DisplayObject = container.getChildAt(i);

                    dumpStarlingDisplayObject(child, depth + 1);
                }
            };

            dumpStarlingDisplayObject(Global.starlingStage);

            trace("========================================");
        }

		public static function dumpDisplayObject(obj: flash.display.DisplayObject, depth: int = 0):void {
			if (depth == 0) {
                Util.log("===========================");
            }

			var spacer: String = "";
			for (var j: int = 0; j < depth; j++) {
				spacer += "\t";
			}

			Util.log(spacer + "(" + getQualifiedClassName(obj) + ") " + obj.name + (!obj.visible ? "<invisible>" : ""));

			if (!(obj is DisplayObjectContainer)) return;

			var container: DisplayObjectContainer = obj as DisplayObjectContainer;

			for (var i: int = 0; i < container.numChildren; i++)
			{
				var child: flash.display.DisplayObject = container.getChildAt(i);

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


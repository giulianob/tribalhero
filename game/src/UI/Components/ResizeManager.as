package src.UI.Components
{
	import flash.display.DisplayObject;
	import flash.display.Stage;
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.geom.IntDimension;
	import org.aswing.JFrame;
	import org.aswing.JPanel;
	import src.Constants;
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class ResizeManager
	{

		private var objects: Array = new Array();

		private var stage: Stage;
		private var lastSize: IntDimension;
		private var originalStageSize: IntDimension;

		public static const ANCHOR_LEFT: int = 1;
		public static const ANCHOR_RIGHT: int = 2;
		public static const ANCHOR_TOP: int = 4;
		public static const ANCHOR_BOTTOM: int = 8;

		private const resizeDelay: Timer = new Timer(150, 0);

		public function ResizeManager(stage: Stage)
		{
			this.stage = stage;

			resizeDelay.addEventListener(TimerEvent.TIMER, onResize);

			originalStageSize = new IntDimension(Constants.origScreenW, Constants.origScreenH);

			lastSize = new IntDimension(Constants.screenW, Constants.screenH);

			stage.addEventListener(Event.RESIZE, function(e: Event) : void {
				resizeDelay.repeatCount = 1;
				resizeDelay.start();
			});
		}

		public function onResize(e: Event = null) : void {

			resizeDelay.stop();

			Constants.screenW = stage.stageWidth;
			Constants.screenH = stage.stageHeight;

			var delta: IntDimension = new IntDimension(stage.stageWidth - lastSize.width, stage.stageHeight - lastSize.height);

			for each (var def: * in objects) {
				var obj: DisplayObject = def.obj;

				if (obj is JPanel) {
					var frame: JFrame = (obj as JPanel).getParent() as JFrame;
					if (frame) {
						obj = frame.parent;
					} else {
						continue;
					}
				}

				var anchors: int = def.anchors;

				var origWidth: int = obj.width / obj.scaleX;
				var origHeight: int = obj.height / obj.scaleY;

				var stageScaleX: Number = stage.stageWidth / originalStageSize.width;
				var stageScaleY: Number = stage.stageHeight / originalStageSize.height;

				if ((anchors & ANCHOR_LEFT) == ANCHOR_LEFT && (anchors & ANCHOR_RIGHT) == ANCHOR_RIGHT) {
					obj.width = stageScaleX * (obj.width / obj.scaleX);
				}
				else if ((anchors & ANCHOR_RIGHT) == ANCHOR_RIGHT) {
					obj.x += delta.width;
				}

				if ((anchors & ANCHOR_TOP) == ANCHOR_TOP && (anchors & ANCHOR_BOTTOM) == ANCHOR_BOTTOM) {
					obj.height = stageScaleY * (obj.height / obj.scaleY);
				} else if ((anchors & ANCHOR_BOTTOM) == ANCHOR_BOTTOM) {
					obj.y += delta.height;
				}
			}

			lastSize = new IntDimension(stage.stageWidth, stage.stageHeight);
		}

		public function addObject(obj: DisplayObject, anchors: int) : void {
			objects.push({anchors: anchors, obj: obj});
		}

		public function removeObject(obj: DisplayObject) : void {
			for (var i: int = 0; i < objects.length; i++) {
				if (objects[i].obj == obj) {
					objects.splice(i, 1);
					break;
				}
			}
		}

		public function removeAllObjects() : void {
			objects = new Array();
		}

		public function forceMove() : void {
			onResize();
		}

	}

}


package src.UI.Components
{
	import flash.events.*;
	import flash.geom.Point;
	import org.aswing.*;
	import org.aswing.border.EmptyBorder;
	import org.aswing.event.*;
	import src.*;
	import src.Map.MapUtil;
	import src.Map.Username;
	import src.UI.Components.*;

	public class CoordLabel extends JLabelButton
	{
		public function CoordLabel(x: int, y: int)
		{
			super(x + "," + y);
			
			setBorder(new EmptyBorder());
			
			setHorizontalAlignment(AsWingConstants.LEFT);

			new SimpleTooltip(this, "Go to coords");
			
			addEventListener(MouseEvent.MOUSE_DOWN, function(e: MouseEvent) : void {
				Global.gameContainer.clearAllSelections();
				Global.gameContainer.closeAllFrames(true);			
				var pt: Point = MapUtil.getScreenCoord(x, y);
				Global.map.camera.ScrollToCenter(pt.x, pt.y);
			});
		}
		
	}

}

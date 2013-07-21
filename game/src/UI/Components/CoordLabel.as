package src.UI.Components
{
    import flash.events.*;
    import flash.geom.Point;

    import org.aswing.*;
    import org.aswing.border.EmptyBorder;

    import src.*;
    import src.Map.TileLocator;

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
				var pt: Point = TileLocator.getScreenCoord(x, y);
				Global.map.camera.ScrollToCenter(pt.x, pt.y);
			});
		}
		
	}

}

package src.UI.Components
{
    import flash.events.*;

    import org.aswing.*;
    import org.aswing.border.EmptyBorder;

    import src.*;
    import src.Map.Position;
    import src.Map.ScreenPosition;
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
				var pt: ScreenPosition = TileLocator.getScreenCoord(new Position(x, y));
				Global.map.camera.ScrollToCenter(pt);
			});
		}
		
	}

}


package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
    import org.aswing.AsWingConstants;
    import org.aswing.ext.MultilineLabel;
    import org.aswing.JLabel;
    import org.aswing.JPanel;
    import org.aswing.SoftBoxLayout;
	import src.Global;
	import src.Objects.Factories.*;
	import src.Objects.*;
	import src.Objects.Actions.ActionButton;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Cursors.*;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.UI.Tooltips.Tooltip;

	public class BuildRoadButton extends ActionButton
	{
		public function BuildRoadButton(parentObj: SimpleGameObject)
		{
			super(parentObj, "Build Road");

            // Tooltip
            var tooltip: Tooltip = new Tooltip();
            var tooltipPnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
            
            var lblBuildRoad: JLabel = new JLabel("Build Road", null, AsWingConstants.LEFT)
            GameLookAndFeel.changeClass(lblBuildRoad, "Tooltip.text");
            
            var lblBuildRoadDescription: MultilineLabel = new MultilineLabel("All structures, other than Farms and Lumbermills, must be connected to a road.", 0, 20);
            GameLookAndFeel.changeClass(lblBuildRoadDescription, "Tooltip.text");
            
            tooltipPnl.appendAll(lblBuildRoad, lblBuildRoadDescription);            
            tooltip.getUI().insert(0, tooltipPnl);
            tooltip.bind(this);
            
			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled())
			{
				var cursor: BuildRoadCursor = new BuildRoadCursor();
				cursor.init(parentObj);
			}
		}

		override public function alwaysEnabled(): Boolean
		{
			return true;
		}
	}

}


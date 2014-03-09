package src.UI.Components 
{
    import flash.events.Event;

    import org.aswing.*;

    import src.Objects.*;
    import src.UI.LookAndFeel.GameLookAndFeel;

    public class SimpleResourcesPanel extends JPanel
	{
		private var resources: Resources = null;
		private var showLabor: Boolean;		
		private var forTooltip: Boolean;		
		
		public function SimpleResourcesPanel(resources: Resources, showLabor: Boolean = true, forTooltip: Boolean = false) 
		{
			this.showLabor = showLabor;
			this.resources = resources;
			this.forTooltip = forTooltip;
			
			setBorder(null);
			setLayout(new FlowLayout(AsWingConstants.LEFT, 8, 5, false));
			
			draw();
		}			
		
		private function draw(e: Event = null): void
		{			
			removeAll();
			
			if (showLabor)
				append(resourceLabelMaker(resources.labor, "Labor", new AssetIcon(new ICON_LABOR())));
				
			append(resourceLabelMaker(resources.gold, "Gold", new AssetIcon(new ICON_GOLD())));
			append(resourceLabelMaker(resources.wood, "Wood", new AssetIcon(new ICON_WOOD())));
			append(resourceLabelMaker(resources.crop, "Crop", new AssetIcon(new ICON_CROP())));
			append(resourceLabelMaker(resources.iron, "Iron", new AssetIcon(new ICON_IRON())));			
		}
		
		private function resourceLabelMaker(value: int, tooltip: String, icon: Icon = null) : JLabel {
			var label: JLabel = new JLabel(value.toString(), icon);
			
			if (forTooltip)
				GameLookAndFeel.changeClass(label, "Tooltip.text Label.small");
			else
				new SimpleTooltip(label, tooltip);
			
			label.setIconTextGap(0);
			label.setHorizontalTextPosition(AsWingConstants.RIGHT);
			return label;
		}
			
	}

}
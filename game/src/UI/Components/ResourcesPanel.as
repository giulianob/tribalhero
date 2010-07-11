package src.UI.Components 
{
	import flash.events.Event;
	import org.aswing.event.AWEvent;
	import src.Map.City;
	import src.Objects.*;
	import src.UI.LookAndFeel.GameLookAndFeel;
	
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;	
	
	public class ResourcesPanel extends JPanel
	{
		private var city: City = null;
		private var resources: Resources = null;
		
		public function ResourcesPanel(resources: Resources, city: City) 
		{
			this.city = city;
			this.resources = resources;
			
			setBorder(null);
			setLayout(new FlowLayout(AsWingConstants.LEFT, 8, 5, false));
			
			addEventListener(Event.ADDED_TO_STAGE, addedToStage);
			
			addEventListener(Event.REMOVED_FROM_STAGE, removedFromStage);		
		}			
		
		private function addedToStage(e: Event) : void
		{
			city.addEventListener(City.RESOURCES_UPDATE, draw);
			draw();			
		}
		
		private function removedFromStage(e: Event) : void
		{
			city.removeEventListener(City.RESOURCES_UPDATE, draw);
		}
		
		private function draw(e: Event = null): void
		{			
			removeAll();
			var affordable: Resources = new Resources( -1, -1, -1, -1, -1);
			
			if (city)
				affordable = city.resources.toResources();
			
			append(resourceLabelMaker(resources.labor, affordable.labor, new AssetIcon(new ICON_LABOR())));
			append(resourceLabelMaker(resources.gold, affordable.gold, new AssetIcon(new ICON_GOLD())));
			append(resourceLabelMaker(resources.wood, affordable.wood, new AssetIcon(new ICON_WOOD())));
			append(resourceLabelMaker(resources.crop, affordable.crop, new AssetIcon(new ICON_CROP())));
			append(resourceLabelMaker(resources.iron, affordable.iron, new AssetIcon(new ICON_IRON())));			
		}
		
		private function resourceLabelMaker(value: int, afford: int, icon: Icon = null) : JLabel {
			var label: JLabel = new JLabel(value.toString(), icon);
			
			if (afford == -1 || value <= afford)
				GameLookAndFeel.changeClass(label, "Tooltip.text Label.small");
			else
				GameLookAndFeel.changeClass(label, "Label.error Label.small");
			
			label.setIconTextGap(0);
			label.setHorizontalTextPosition(AsWingConstants.LEFT);
			return label;
		}
			
	}

}
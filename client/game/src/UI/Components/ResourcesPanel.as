package src.UI.Components
{
    import flash.events.Event;

    import org.aswing.*;

    import src.Map.City;
    import src.Objects.*;
    import src.Objects.Factories.SpriteFactory;
    import src.UI.LookAndFeel.GameLookAndFeel;

    public class ResourcesPanel extends JPanel
	{
		private var cityOrResource: * = null;
		private var resources: Resources = null;
		private var tooltipMode: Boolean;
		private var showLabor: Boolean;

		/*
		* Set a city to show affordability based on city resources
		*/
		public function ResourcesPanel(resources: Resources, cityOrResource: * = null, forTooltip: Boolean = true, showLabor: Boolean = true)
		{
			this.cityOrResource = cityOrResource;
			this.resources = resources;
			this.tooltipMode = forTooltip;
			this.showLabor = showLabor;

			setBorder(null);
			setLayout(new FlowLayout(AsWingConstants.LEFT, 8, 5, false));

			addEventListener(Event.ADDED_TO_STAGE, addedToStage);

			addEventListener(Event.REMOVED_FROM_STAGE, removedFromStage);
		}

		private function addedToStage(e: Event) : void
		{
			if (cityOrResource is City) {
				cityOrResource.addEventListener(City.RESOURCES_UPDATE, draw);
			}
			draw();
		}

		private function removedFromStage(e: Event) : void
		{
			if (cityOrResource is City) {
				cityOrResource.removeEventListener(City.RESOURCES_UPDATE, draw);
			}
		}

		private function draw(e: Event = null): void
		{
			removeAll();
			var affordable: Resources = new Resources( -1, -1, -1, -1, -1);

			if (cityOrResource is City) {
				affordable = (cityOrResource as City).resources.toResources();
			}
			else if (cityOrResource is Resources) {
				affordable = cityOrResource;
			}

			if (showLabor) {
				append(resourceLabelMaker(resources.labor, affordable.labor, new AssetIcon(SpriteFactory.getFlashSprite("ICON_LABOR"))));
			}
			
			append(resourceLabelMaker(resources.gold, affordable.gold, new AssetIcon(SpriteFactory.getFlashSprite("ICON_GOLD"))));
			append(resourceLabelMaker(resources.wood, affordable.wood, new AssetIcon(SpriteFactory.getFlashSprite("ICON_WOOD"))));
			append(resourceLabelMaker(resources.crop, affordable.crop, new AssetIcon(SpriteFactory.getFlashSprite("ICON_CROP"))));
			append(resourceLabelMaker(resources.iron, affordable.iron, new AssetIcon(SpriteFactory.getFlashSprite("ICON_IRON"))));
		}

		private function resourceLabelMaker(value: int, afford: int, icon: Icon = null) : JLabel {
			var label: JLabel = new JLabel(value.toString(), icon);

			if (afford == -1 || value <= afford) {
				GameLookAndFeel.changeClass(label, tooltipMode ? "Tooltip.text Label.small" : "Label.small");
			} else {
				GameLookAndFeel.changeClass(label, "Label.error Label.small");
			}

			label.setIconTextGap(0);
			label.setHorizontalTextPosition(AsWingConstants.LEFT);
			return label;
		}

	}

}


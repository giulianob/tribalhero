package src.UI.Components
{
    import flash.events.Event;

    import org.aswing.*;

    import src.Objects.Effects.Formula;
    import src.UI.LookAndFeel.GameLookAndFeel;

    public class NewCityResourcesPanel extends JPanel
	{
		/*
		* Set a city to show affordability based on city resources
		*/
		public function NewCityResourcesPanel()
		{
			setBorder(null);
			setLayout(new FlowLayout(AsWingConstants.LEFT, 8, 5, false));
			draw();
		}

		private function draw(e: Event = null): void
		{
			removeAll();
			var data:* = Formula.getResourceNewCity();

			append(labelMaker(data.influenceRequired, data.influenceCurrent, new AssetIcon(new ICON_UPGRADE())));
			append(labelMaker(data.wagonRequired, data.wagonCurrent, new AssetIcon(new WAGON_UNIT())));
		}

		private function labelMaker(value: int, afford: int, icon: Icon = null) : JLabel {
			var label: JLabel = new JLabel(value.toString(), icon);

			if (afford == -1 || value <= afford) {
				GameLookAndFeel.changeClass(label, "Tooltip.text Label.small");
			} else {
				GameLookAndFeel.changeClass(label, "Label.error Label.small");
			}

			label.setIconTextGap(0);
			label.setHorizontalTextPosition(AsWingConstants.LEFT);
			return label;
		}

	}

}


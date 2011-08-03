package src.UI.Components
{

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.UI.GameJFrame;
	import src.UI.LookAndFeel.GameLookAndFeel;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class StarRating extends JPanel
	{
		private var min: int;
		private var max: int;
		private var numberOfStars: int;
		private var value: int;

		public function StarRating(min: int, max: int, value: int, numberOfStars: int)
		{
			setLayout(new FlowLayout(AsWingConstants.LEFT, 2, 0, false));
			setBorder(new EmptyBorder(null, new Insets()));

			this.min = min;
			this.max = max;
			this.value = value;
			this.numberOfStars = numberOfStars;

			draw();
		}

		public function draw() : void {
			removeAll();

			// normalize max and value so it goes from 0-max only
			var adjustedMax: int = max - min;
			var adjustedValue: int = value - min;

			var percentage: int = Math.max(0, Math.min(100, (adjustedValue / adjustedMax) * 100));
			var fullStarPercent: int = Math.round(100 / numberOfStars);			

			var fullStars: int = Math.floor(percentage / fullStarPercent);
			var halfStars: int = fullStars == 0 ? 1 : Math.floor((percentage - (fullStars * fullStarPercent)) / (fullStarPercent / 2));

			for (var i: int = 0; i < fullStars; i++) {
				append(new AssetPane(new ICON_STAR));
			}

			for (i = 0; i < halfStars; i++) {
				append(new AssetPane(new ICON_HALF_STAR));
			}
			
			for (i = 0; i < numberOfStars - (fullStars + halfStars); i++) {
				append(new AssetPane(new ICON_EMPTY_STAR));
			}
			
			var lblValue: JLabel = new JLabel("(" + value + ")", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblValue, "Tooltip.text Label.very_small");
			append(lblValue);
		}

		public function setValue(newValue: int) : void {
			this.value = newValue;
			draw();
		}

	}

}


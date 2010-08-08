package src.UI.Tooltips {
	import org.aswing.AsWingConstants;
	import org.aswing.Component;
	import org.aswing.ext.MultilineLabel;
	import org.aswing.JLabel;
	import org.aswing.SoftBoxLayout;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class TextTooltip extends Tooltip {

		public function TextTooltip(text: String) {
			var label: Component;

			if (text.length < 30) {
				label = new JLabel(text);
			} else {
				label = new MultilineLabel(text, 0, 20);
			}

			GameLookAndFeel.changeClass(label, "Tooltip.text");

			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(5);
			ui.setLayout(layout0);

			ui.append(label);
		}
	}

}


package src.UI.Tooltips {
	import org.aswing.AsWingConstants;
	import org.aswing.Component;
	import org.aswing.ext.MultilineLabel;
	import org.aswing.JLabel;
	import org.aswing.SoftBoxLayout;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class TextTooltip extends Tooltip {

		private var text: String = "";
		private var label: Component;
		
		public function TextTooltip(text: String) {							
			ui.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 5));			
			
			createUI(text);			
		}
		
		public function append(label: Component): void {
			ui.append(label);
		}
		
		public function getText(): String {
			return text;
		}
		
		public function setText(text: String): void {
			if (this.text == text) {
				return;
			}
			
			ui.remove(label);
			createUI(text);
		}
		
		private function createUI(text: String): void {
			if (text.length < 40) {
				label = new JLabel(text, null, AsWingConstants.LEFT);
			} else {
				label = new MultilineLabel(text, 0, 20);
			}			
			
			
			GameLookAndFeel.changeClass(label, "Tooltip.text");
			
			this.text = text;
			
			ui.insert(0, label);
		}
	}

}


package src.UI.Tooltips {
    import org.aswing.AsWingConstants;
    import org.aswing.Component;
    import org.aswing.JLabel;
    import org.aswing.SoftBoxLayout;
    import org.aswing.ext.MultilineLabel;

    import src.UI.LookAndFeel.GameLookAndFeel;

    public class TextTooltip extends Tooltip {

		private var text: String = "";
		private var label: Component;
		private var headerLabel:JLabel;
		private var header: String;
		
		public function TextTooltip(text: String, header: String = "") {										
			ui.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 5));			
			
			createUI(text, header);			
		}
		
		public function append(label: Component): void {
			ui.append(label);
		}
		
		public function getText(): String {
			return text;
		}
		
		public function setText(text: String, header: String = ""): void {
			if (this.text == text && this.header == header) {
				return;
			}
			
			ui.remove(label);
			label = null;
			
			if (headerLabel) {
				ui.remove(headerLabel);
				headerLabel = null;
			}
			
			createUI(text, header);
		}
		
		private function createUI(text: String, header: String): void {
			if (text.length < 40) {
				label = new JLabel(text, null, AsWingConstants.LEFT);
			} else {
				label = new MultilineLabel(text, 0, 20);
			}			
			
			GameLookAndFeel.changeClass(label, "Tooltip.text");
			
			this.text = text;
			this.header = header;
			
			ui.insert(0, label);
			
			if (header != "") {
				headerLabel = new JLabel(header, null, AsWingConstants.LEFT);
				GameLookAndFeel.changeClass(headerLabel, "header");
				ui.insert(0, headerLabel);
			}
		}
	}

}


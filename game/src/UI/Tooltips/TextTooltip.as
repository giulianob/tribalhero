package src.UI.Tooltips {
	import org.aswing.ASColor;
	import org.aswing.AssetPane;
	import org.aswing.AsWingConstants;
	import org.aswing.Icon;
	import org.aswing.JLabel;
	import org.aswing.JPanel;
	import org.aswing.JToggleButton;
	import org.aswing.plaf.ASColorUIResource;
	import org.aswing.plaf.ComponentUI;
	import org.aswing.SoftBoxLayout;
	import src.UI.GameLookAndFeel;

	public class TextTooltip extends Tooltip {
		
		public function TextTooltip(text: String, icon: Icon = null) {				
			var label: JLabel = new JLabel(text, icon);	
			
			GameLookAndFeel.changeClass(label, "header");
			
			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(5);
			ui.setLayout(layout0);
			
			ui.append(label);
		}
	}
	
}
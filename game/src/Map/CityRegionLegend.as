package src.Map 
{
	import fl.lang.Locale;
	import flash.display.DisplayObject;
	import flash.display.Sprite;
	import flash.events.AsyncErrorEvent;
	import org.aswing.AssetIcon;
	import org.aswing.AssetPane;
	import org.aswing.AsWingConstants;
	import org.aswing.border.EmptyBorder;
	import org.aswing.EmptyLayout;
	import org.aswing.geom.IntDimension;
	import org.aswing.GridLayout;
	import org.aswing.Insets;
	import org.aswing.JButton;
	import org.aswing.JLabel;
	import org.aswing.Icon;
	import org.aswing.JPanel;
	import org.aswing.SoftBox;
	import org.aswing.SoftBoxLayout;
	import src.Constants;
	import src.Objects.Factories.ObjectFactory;
	import src.UI.GameJBox;
	import src.UI.LookAndFeel.GameLookAndFeel;
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class CityRegionLegend
	{
		public static const LEGEND_WIDTH :int = 140;
		private var ui: GameJBox = new GameJBox();
		private var button: JButton = new JButton("Default");
		private var legendPanel: JPanel = new JPanel();
		
		public function CityRegionLegend() 
		{
			ui.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));			
			ui.setBorder(new EmptyBorder(null, new Insets(10, 10, 10, 10)));
			ui.setPreferredWidth( -1);
			
			legendPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			GameLookAndFeel.changeClass(button, "GameJBoxButton");
						
			ui.append(button);
			ui.append(legendPanel);
		}
		
		public function setLegendTitle(title : String) : void
		{
			button.setText(title);
		}
		
		public function addOnClickListener(func : Function) : void
		{
			button.addActionListener(func, 0, true);
		}
		
		public function show(x : int, y: int) : void
		{
			ui.show();
			
			ui.getFrame().pack();
			ui.getFrame().setLocationXY(x, y);
			ui.getFrame().repaintAndRevalidate();
		}
		
		public function hide() : void {
			if (ui.getFrame()) {
				ui.getFrame().dispose();
			}
		}
		
		public function removeAll(): void {
			legendPanel.removeAll();
		}

		public function add(icon: DisplayObject, desc: String) : void
		{
			var legendLabel: JLabel = new JLabel(desc, new AssetIcon(icon), AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(legendLabel, "Tooltip.text Label.small");
			legendPanel.appendAll(legendLabel);
		}
	}

}
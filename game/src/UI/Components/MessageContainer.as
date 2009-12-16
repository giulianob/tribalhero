package src.UI.Components 
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetIcon;
	import org.aswing.AsWingConstants;
	import org.aswing.FlowLayout;
	import org.aswing.Icon;
	import org.aswing.JFrame;
	import org.aswing.JLabel;
	import org.aswing.JPanel;
	import org.aswing.SoftBoxLayout;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.UI.GameLookAndFeel;	
	import src.UI.GameJBox;
	import src.UI.Tooltips.*;
	
	public class MessageContainer
	{		
		private var ui: GameJBox = new GameJBox();
		
		private function show() : void
		{
			if (!ui.getFrame())
				ui.show();
			
			ui.getFrame().pack();
			ui.getFrame().setLocationXY(Constants.screenW / 2 - ui.getFrame().getWidth() / 2, Constants.screenH - ui.getFrame().getHeight() - 50);	
		}
		
		public function hide() : void {
			if (ui.getFrame())
				ui.getFrame().dispose();
		}
		
		public function showMessage(text: String, icon: Icon = null) : void {																
			hide();
			
			ui = new GameJBox();
			
			var label: JLabel = new JLabel(text, icon);	
			
			GameLookAndFeel.changeClass(label, "header");
			
			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(5);
			ui.setLayout(layout0);
			
			var pnl: JPanel = new JPanel();
			pnl.append(label);
			
			ui.append(pnl);
			
			show();
		}		
		
	}

}
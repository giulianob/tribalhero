package src.UI.Tooltips {
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import src.*;
	import src.Map.*;
	import src.Objects.Stronghold.*;
	import src.UI.LookAndFeel.*;
	import src.Util.StringHelper;


	public class StrongholdTooltip extends Tooltip{

		private var lblName: JLabel;
		private var lblLevel: JLabel;

		public function StrongholdTooltip(stronghold: Stronghold) {
			createUI();

			lblLevel.setText(StringHelper.localize("STR_LEVEL_VALUE", stronghold.level));
			
			if (!stronghold.strongholdName) {
				Global.map.usernames.strongholds.getUsername(stronghold.id, setName);
			}
			else {
				lblName.setText(stronghold.strongholdName);
			}
		}

		private function createUI() : void {
			ui.setLayout(new BorderLayout(20, 0));

			lblName = new JLabel();
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");
			lblName.setConstraints("Center");

			lblLevel = new JLabel();
			lblLevel.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLevel, "Tooltip.text");
			lblLevel.setConstraints("South");

			ui.append(lblName);
			ui.append(lblLevel);			
		}
		
		private function setName(username: Username, custom: * = null): void
		{						
			lblName.setText(username.name);	
			if (ui.getFrame())
				ui.getFrame().pack();
		}
	}
}


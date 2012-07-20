package src.UI.Components
{
	import flash.events.*;
	import org.aswing.*;
	import org.aswing.border.EmptyBorder;
	import org.aswing.event.*;
	import src.*;
	import src.Map.Username;
	import src.UI.Components.*;

	public class StrongholdLabel extends JLabelButton
	{
		public var strongholdId: int;

		public function StrongholdLabel(strongholdId: int, strongholdName: String = null, showTooltip: Boolean = true)
		{
			super("-");
			
			setBorder(new EmptyBorder());
			
			setHorizontalAlignment(AsWingConstants.LEFT);
			
			this.strongholdId = strongholdId;
			
			if (strongholdName)
				setText(strongholdName);
			else
				Global.map.usernames.strongholds.getUsername(strongholdId, onReceiveUsername);

		}
		
		private function onReceiveUsername(username: Username, custom: *) : void {
			setText(username.name);
			repaintAndRevalidate();
		}
	}

}

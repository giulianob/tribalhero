package src.UI.Components
{
    import flash.events.*;

    import org.aswing.*;
    import org.aswing.border.EmptyBorder;

    import src.*;
    import src.Map.Username;

    public class StrongholdLabel extends JLabelButton
	{
		public var strongholdId: int;

		public function StrongholdLabel(strongholdId: int, isPrivate: Boolean = false, strongholdName: String = null)
		{
			super("-");
			
			setBorder(new EmptyBorder());
			
			setHorizontalAlignment(AsWingConstants.LEFT);
			
			this.strongholdId = strongholdId;
			
			if (strongholdName)
				setText(strongholdName);
			else
				Global.map.usernames.strongholds.getUsername(strongholdId, onReceiveUsername);

			if (isPrivate) {
				new SimpleTooltip(this, "View profile");
				addEventListener(MouseEvent.MOUSE_DOWN, function(e: MouseEvent) : void {
					Global.mapComm.Stronghold.viewStrongholdProfile(strongholdId);
				});				
			} else {
				new SimpleTooltip(this, "Go to Stronghold");
				addEventListener(MouseEvent.MOUSE_DOWN, function(e: MouseEvent) : void {
					Global.mapComm.Stronghold.gotoStrongholdLocation(strongholdId);
				});
			}
		}
				
		private function onReceiveUsername(username: Username, custom: *) : void {
			setText(username.name);
			repaintAndRevalidate();
		}
	}

}

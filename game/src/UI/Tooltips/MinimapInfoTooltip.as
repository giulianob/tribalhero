package src.UI.Tooltips
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AsWingConstants;
	import org.aswing.JLabel;
	import org.aswing.SoftBoxLayout;
	import src.Global;
	import src.Map.Username;
	import src.Objects.SimpleGameObject;
	import src.UI.GameLookAndFeel;
	import src.UI.Tooltips.TextTooltip;

	/**
	 * ...
	 * @author
	 */
	public class MinimapInfoTooltip extends Tooltip
	{
		private var obj: SimpleGameObject;
		private var tooltip: TextTooltip;

		private var disposed: Boolean = false;

		public function MinimapInfoTooltip(obj: SimpleGameObject)
		{
			this.obj = obj;

			obj.addEventListener(Event.REMOVED_FROM_STAGE, dispose);
			obj.addEventListener(MouseEvent.ROLL_OUT, dispose);

			Global.map.usernames.cities.getUsername(obj.cityId, onGetUsername);
		}

		private function onGetUsername(username: Username, custom: * ) : void {
			if (disposed) return;

			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel(username.name, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");

			var lblLvl: JLabel = new JLabel("Level " + obj.getLevel(), null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLvl, "Tooltip.text");

			ui.append(lblName);
			ui.append(lblLvl);

			show(obj);
		}

		private function dispose(e: Event = null) : void {
			if (disposed) return;

			disposed = true;
			ui.removeEventListener(MouseEvent.ROLL_OUT, dispose);
			ui.removeEventListener(Event.REMOVED_FROM_STAGE, dispose);
			hide();
		}
	}

}


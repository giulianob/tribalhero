package src.UI.Tooltips
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import org.aswing.AsWingConstants;
	import org.aswing.JLabel;
	import org.aswing.SoftBoxLayout;
	import src.Constants;
	import src.Global;
	import src.Map.MapUtil;
	import src.Map.Username;
	import src.Objects.SimpleGameObject;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.UI.Tooltips.TextTooltip;
	import src.Util.Util;

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
			
			var mapPos: Point = MapUtil.getScreenMinimapToMapCoord(obj.getX(), obj.getY());
			var distance: int = MapUtil.distance(mapPos.x, mapPos.y, Global.gameContainer.selectedCity.MainBuilding.x, Global.gameContainer.selectedCity.MainBuilding.y);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblLvl);
			ui.append(lblDistance);

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


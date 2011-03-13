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
	import src.Objects.Factories.TroopFactory;
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
		private var obj: *;
		private var tooltip: TextTooltip;
		private var cityRegionObjectType: int;

		private var disposed: Boolean = false;

		public function MinimapInfoTooltip(cityRegionObjectType: int, obj: *)
		{
			this.obj = obj;
			this.cityRegionObjectType = cityRegionObjectType;

			obj.gameObj.addEventListener(Event.REMOVED_FROM_STAGE, dispose);
			obj.gameObj.addEventListener(MouseEvent.ROLL_OUT, dispose);

			// City tooltip
			if (cityRegionObjectType == 0) 
				Global.map.usernames.cities.getUsername(obj.gameObj.cityId, createCityUI);
			// Forest tooltip
			else if (cityRegionObjectType == 1)
				createForestUI();
			// Troop tooltip
			else if (cityRegionObjectType == 2)
				Global.map.usernames.cities.getUsername(obj.gameObj.cityId, createTroopUI);
		}

		private function createForestUI() : void {
			if (disposed) return;
			
			var gameObj: SimpleGameObject = obj.gameObj;

			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel("Forest", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");

			var lblLvl: JLabel = new JLabel("Level " + gameObj.getLevel(), null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLvl, "Tooltip.text");
			
			var mapPos: Point = MapUtil.getScreenMinimapToMapCoord(gameObj.getX(), gameObj.getY());
			var distance: int = MapUtil.distance(mapPos.x, mapPos.y, Global.gameContainer.selectedCity.MainBuilding.x, Global.gameContainer.selectedCity.MainBuilding.y);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblLvl);
			ui.append(lblDistance);

			show(gameObj);
		}		
		
		private function createCityUI(username: Username, custom: * ) : void {
			if (disposed) return;

			var gameObj: SimpleGameObject = obj.gameObj;
			
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel(username.name, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");

			var lblLvl: JLabel = new JLabel("Level " + gameObj.getLevel(), null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLvl, "Tooltip.text");
			
			var mapPos: Point = MapUtil.getScreenMinimapToMapCoord(gameObj.getX(), gameObj.getY());
			var distance: int = MapUtil.distance(mapPos.x, mapPos.y, Global.gameContainer.selectedCity.MainBuilding.x, Global.gameContainer.selectedCity.MainBuilding.y);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblLvl);
			ui.append(lblDistance);

			show(gameObj);
		}
		
		private function createTroopUI(username: Username, custom: * ) : void {
			if (disposed) return;

			var gameObj: SimpleGameObject = obj.gameObj;
			
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel(username.name + "(" + obj.troopId + ")", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");
			
			var mapPos: Point = MapUtil.getScreenMinimapToMapCoord(gameObj.getX(), gameObj.getY());
			var distance: int = MapUtil.distance(mapPos.x, mapPos.y, Global.gameContainer.selectedCity.MainBuilding.x, Global.gameContainer.selectedCity.MainBuilding.y);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblDistance);

			show(gameObj);
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


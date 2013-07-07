package src.UI.Tooltips
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import org.aswing.AsWingConstants;
	import org.aswing.JLabel;
	import org.aswing.SoftBoxLayout;
	import src.Global;
	import src.Map.CityRegionObject;
	import src.Map.TileLocator;
	import src.Map.Username;
	import src.Objects.Factories.ObjectFactory;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.UI.Tooltips.TextTooltip;

	/**
	 * ...
	 * @author
	 */
	public class MinimapInfoTooltip extends Tooltip
	{
		private var obj: CityRegionObject;
		private var tooltip: TextTooltip;

		private var disposed: Boolean = false;

		public function MinimapInfoTooltip(obj: CityRegionObject)
		{
			this.obj = obj;

			obj.addEventListener(Event.REMOVED_FROM_STAGE, dispose);
			obj.addEventListener(MouseEvent.ROLL_OUT, dispose);

			// City tooltip
			if (obj.type == ObjectFactory.TYPE_CITY) 
				Global.map.usernames.cities.getUsername(obj.groupId, createCityUI);
			// Forest tooltip
			else if (obj.type == ObjectFactory.TYPE_FOREST)
				createForestUI();
			// Troop tooltip
			else if (obj.type == ObjectFactory.TYPE_TROOP_OBJ)
				Global.map.usernames.cities.getUsername(obj.groupId, createTroopUI);
			// Stronghold tooltip
			else if (obj.type == ObjectFactory.TYPE_STRONGHOLD)
				Global.map.usernames.strongholds.getUsername(obj.objectId, createStrongholdUI, obj);
			else if (obj.type == ObjectFactory.TYPE_BARBARIAN_TRIBE)
				createBarbarianTribeUI();
			
		}

		private function createForestUI() : void {
			if (disposed) return;
			
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel("Forest", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");

			var lblLvl: JLabel = new JLabel("Level " + obj.extraProps.level, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLvl, "Tooltip.text");
			
			var mapPos: Point = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
			var distance: int = TileLocator.distance(mapPos.x, mapPos.y, Global.gameContainer.selectedCity.MainBuilding.x, Global.gameContainer.selectedCity.MainBuilding.y);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblLvl);
			ui.append(lblDistance);

			show(obj);
		}		
		
		private function createCityUI(username: Username, custom: * ) : void {
			if (disposed) return;
			
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel(username.name, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");

			var lblLvl: JLabel = new JLabel("Level " + obj.extraProps.level, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLvl, "Tooltip.text");
			
			var mapPos: Point = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
			var distance: int = TileLocator.distance(mapPos.x, mapPos.y, Global.gameContainer.selectedCity.MainBuilding.x, Global.gameContainer.selectedCity.MainBuilding.y);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblLvl);
			ui.append(lblDistance);

			show(obj);
		}

		private function onUpdateTribeName(username: Username, custom: * ) : void {
			var lblTribe: JLabel = new JLabel("Occupied by Tribe " + username.name, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblTribe, "Tooltip.text");
			ui.append(lblTribe);
		}
		
		private function createStrongholdUI(username: Username, custom: * ) : void {
			if (disposed) return;
			
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel(username.name, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");

			var lblLvl: JLabel = new JLabel("Level " + obj.extraProps.level, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLvl, "Tooltip.text");
			
			var mapPos: Point = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
			var distance: int = TileLocator.distance(mapPos.x, mapPos.y, Global.gameContainer.selectedCity.MainBuilding.x, Global.gameContainer.selectedCity.MainBuilding.y);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblLvl);
			ui.append(lblDistance);
			
			if(custom.extraProps.tribeId!=0) {
				Global.map.usernames.tribes.getUsername(custom.extraProps.tribeId, onUpdateTribeName, null);
			} else {
				var lblTribe: JLabel = new JLabel("Occupied by bunch of barbarians!", null, AsWingConstants.LEFT);
				GameLookAndFeel.changeClass(lblTribe, "Tooltip.text");
				ui.append(lblTribe);
			}
			
			show(obj);
		}		
		
		private function createTroopUI(username: Username, custom: * ) : void {
			if (disposed) return;
			
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel(username.name + "(" + obj.extraProps.troopId + ")", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");
			
			var mapPos: Point = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
			var distance: int = TileLocator.distance(mapPos.x, mapPos.y, Global.gameContainer.selectedCity.MainBuilding.x, Global.gameContainer.selectedCity.MainBuilding.y);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblDistance);

			show(obj);
		}		
		
		private function createBarbarianTribeUI() : void {
			if (disposed) return;
			
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel("Barbarian Tribe", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");

			var lblLvl: JLabel = new JLabel("Level " + obj.extraProps.level, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLvl, "Tooltip.text");
			
			var lblCampsRemain: JLabel = new JLabel(obj.extraProps.count + " Camps Remain", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblCampsRemain, "Tooltip.text");

			var mapPos: Point = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
			var distance: int = TileLocator.distance(mapPos.x, mapPos.y, Global.gameContainer.selectedCity.MainBuilding.x, Global.gameContainer.selectedCity.MainBuilding.y);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblLvl);
			ui.append(lblCampsRemain);
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


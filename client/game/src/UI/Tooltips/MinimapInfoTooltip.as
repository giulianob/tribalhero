package src.UI.Tooltips
{
    import flash.events.MouseEvent;

    import org.aswing.AsWingConstants;
    import org.aswing.AssetIcon;
    import org.aswing.JLabel;
    import org.aswing.SoftBoxLayout;

    import src.Global;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.Map.Position;
    import src.Map.TileLocator;
    import src.Map.Username;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Factories.SpriteFactory;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.Util.DateUtil;

    import starling.events.Event;
    import starling.events.Touch;
    import starling.events.TouchEvent;
    import starling.events.TouchPhase;

    public class MinimapInfoTooltip extends Tooltip
	{
		private var obj: MiniMapRegionObject;

		private var disposed: Boolean = false;

		public function MinimapInfoTooltip(obj: MiniMapRegionObject)
		{
			this.obj = obj;

			obj.addEventListener(Event.REMOVED_FROM_STAGE, dispose);
            obj.addEventListener(TouchEvent.TOUCH, onTouched);

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

        private function onTouched(event: TouchEvent): void {
            var touch: Touch = event.getTouch(obj, TouchPhase.HOVER);

            if (!touch) {
                hide();
            }
            else {
                show(obj);
            }
        }

		private function createForestUI() : void {
			if (disposed) return;
			
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel("Forest", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");

			var mapPos: Position = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
			var distance: int = TileLocator.distance(mapPos.x, mapPos.y, 1, Global.gameContainer.selectedCity.primaryPosition.x, Global.gameContainer.selectedCity.primaryPosition.y, 1);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);

            if(obj.extraProps.depleteTime > 0 && obj.extraProps.camps > 0) {
                var timeLeft: int = obj.extraProps.depleteTime - Global.map.getServerTime();
                var lblTime: JLabel = new JLabel("Time left: "+ DateUtil.formatTime(timeLeft), new AssetIcon(SpriteFactory.getFlashSprite("ICON_CLOCK")));
                GameLookAndFeel.changeClass(lblTime, "Tooltip.text");
                ui.append(lblTime);
            }

			ui.append(lblDistance);
		}		
		
		private function createCityUI(username: Username, custom: * ) : void {
			if (disposed) return;
			
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel(username.name, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");

			var lblLvl: JLabel = new JLabel("Level " + obj.extraProps.level, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLvl, "Tooltip.text");
			
			var mapPos: Position = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
			var distance: int = TileLocator.distance(mapPos.x, mapPos.y, 1, Global.gameContainer.selectedCity.primaryPosition.x, Global.gameContainer.selectedCity.primaryPosition.y, 1);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblLvl);
			ui.append(lblDistance);
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
			
			var mapPos: Position = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
			var distance: int = TileLocator.distance(mapPos.x, mapPos.y, 1, Global.gameContainer.selectedCity.primaryPosition.x, Global.gameContainer.selectedCity.primaryPosition.y, 1);
			
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
		}		
		
		private function createTroopUI(username: Username, custom: * ) : void {
			if (disposed) return;
			
			var layout0:SoftBoxLayout = new SoftBoxLayout(AsWingConstants.VERTICAL);
			ui.setLayout(layout0);

			var lblName: JLabel = new JLabel(username.name + "(" + obj.extraProps.troopId + ")", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");
			
			var mapPos: Position = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
			var distance: int = TileLocator.distance(mapPos.x, mapPos.y, 1, Global.gameContainer.selectedCity.primaryPosition.x, Global.gameContainer.selectedCity.primaryPosition.y, 1);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblDistance);
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

			var mapPos: Position = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
			var distance: int = TileLocator.distance(mapPos.x, mapPos.y, 1, Global.gameContainer.selectedCity.primaryPosition.x, Global.gameContainer.selectedCity.primaryPosition.y, 1);
			
			var lblDistance: JLabel = new JLabel(distance + " tiles away", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblDistance, "Tooltip.italicsText");			

			ui.append(lblName);
			ui.append(lblLvl);
			ui.append(lblCampsRemain);
			ui.append(lblDistance);
		}		

		private function dispose(e: * = null) : void {
			if (disposed) {
                return;
            }

			disposed = true;

            obj.removeEventListener(TouchEvent.TOUCH, onTouched);
            obj.removeEventListener(Event.REMOVED_FROM_STAGE, dispose);

			hide();
		}
	}

}


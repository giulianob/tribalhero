package src.FeathersUI.MiniMapDrawer.Drawers {
    import feathers.controls.ToggleButton;

    import src.FeathersUI.MiniMap.MiniMapDiamondIcon;

    import src.FeathersUI.MiniMap.MinimapDotIcon;

    import src.FeathersUI.MiniMapDrawer.LegendGroups.MiniMapGroupCity;

    import src.Global;
    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.Map.Position;
    import src.Map.TileLocator;
    import src.Objects.Factories.SpriteFactory;
    import src.Util.StringHelper;

    import starling.display.Image;
    import starling.events.Event;

    public class MiniMapCityDistance implements IMiniMapObjectDrawer {
        private var cityButton: ToggleButton = new ToggleButton();
        private var d100Button: ToggleButton = new ToggleButton();
        private var d200Button: ToggleButton = new ToggleButton();
        private var d300Button: ToggleButton = new ToggleButton();
        private var d400Button: ToggleButton = new ToggleButton();
        private var d500Button: ToggleButton = new ToggleButton();

        private var DEFAULT_COLORS: * = MiniMapGroupCity.DEFAULT_COLORS;

        public function applyObject(obj: MiniMapRegionObject): void {
            if (Global.map.cities.get(obj.groupId)) {
                if (cityButton.isSelected) return;
                obj.setIcon(new MiniMapDiamondIcon(MiniMapGroupCity.CITY_DEFAULT_COLOR).dot);
            } else {
                // Apply the difficulty transformation to the tile
                var point: Position = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
                var distance: int = TileLocator.distance(point.x, point.y, 1, Global.gameContainer.selectedCity.primaryPosition.x, Global.gameContainer.selectedCity.primaryPosition.y, 1);
                var distanceIdx: int;
                if (distance <= 100 && !d100Button.isSelected) distanceIdx = 4;
                else if (distance > 100 && distance <= 200 && !d200Button.isSelected) distanceIdx = 3;
                else if (distance > 200 && distance <= 300 && !d300Button.isSelected) distanceIdx = 2;
                else if (distance > 300 && distance <= 400 && !d400Button.isSelected) distanceIdx = 1;
                else if (distance > 400 && !d500Button.isSelected)distanceIdx = 0;
                else return;

                obj.setIcon(new MiniMapDiamondIcon(DEFAULT_COLORS[distanceIdx]).dot);
            }
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var cityIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.CITY_DEFAULT_COLOR).dot;
            legend.addFilterButton(cityButton, StringHelper.localize("MINIMAP_LEGEND_CITY"), cityIcon);

            var d100Icon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[4]).dot;
            legend.addFilterButton(d100Button, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN", 100), d100Icon);

            var d200Icon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[3]).dot;
            legend.addFilterButton(d200Button, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN", 200), d200Icon);

            var d300Icon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[2]).dot;
            legend.addFilterButton(d300Button, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN", 300), d300Icon);

            var d400Icon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[1]).dot;
            legend.addFilterButton(d400Button, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN", 400), d400Icon);

            var d500Icon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[4]).dot;
            legend.addFilterButton(d500Button, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_500"), d500Icon);
        }

        public function addOnChangeListener(callback: Function): void {
            cityButton.addEventListener(Event.CHANGE, callback);
            d100Button.addEventListener(Event.CHANGE, callback);
            d200Button.addEventListener(Event.CHANGE, callback);
            d300Button.addEventListener(Event.CHANGE, callback);
            d400Button.addEventListener(Event.CHANGE, callback);
            d500Button.addEventListener(Event.CHANGE, callback);
        }
    }
}
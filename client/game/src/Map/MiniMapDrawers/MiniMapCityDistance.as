package src.Map.MiniMapDrawers {
    import flash.display.*;
    import flash.geom.*;

    import org.aswing.JToggleButton;

    import src.Global;
    import src.Map.MiniMap.LegendGroups.MiniMapGroupCity;
    import src.Map.MiniMap.MiniMapLegendPanel;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Map.Position;
    import src.Map.TileLocator;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Factories.SpriteFactory;
    import src.Util.StringHelper;

    import starling.display.Image;

    public class MiniMapCityDistance implements IMiniMapObjectDrawer {
        private var cityButton: JToggleButton = new JToggleButton();
        private var d100Button: JToggleButton = new JToggleButton();
        private var d200Button: JToggleButton = new JToggleButton();
        private var d300Button: JToggleButton = new JToggleButton();
        private var d400Button: JToggleButton = new JToggleButton();
        private var d500Button: JToggleButton = new JToggleButton();

        private var DEFAULT_COLORS: * = MiniMapGroupCity.DEFAULT_COLORS;

        public function applyObject(obj: MiniMapRegionObject): void {
            var dotSprite: Image;

            if (Global.map.cities.get(obj.groupId)) {
                if (cityButton.isSelected()) return;

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = MiniMapGroupCity.CITY_DEFAULT_COLOR.hex;
                obj.setIcon(dotSprite);
            } else {
                // Apply the difficulty transformation to the tile
                var point: Position = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
                var distance: int = TileLocator.distance(point.x, point.y, 1, Global.gameContainer.selectedCity.primaryPosition.x, Global.gameContainer.selectedCity.primaryPosition.y, 1);
                var distanceIdx: int;
                if (distance <= 100 && !d100Button.isSelected()) distanceIdx = 4;
                else if (distance > 100 && distance <= 200 && !d200Button.isSelected()) distanceIdx = 3;
                else if (distance > 200 && distance <= 300 && !d300Button.isSelected()) distanceIdx = 2;
                else if (distance > 300 && distance <= 400 && !d400Button.isSelected()) distanceIdx = 1;
                else if (distance > 400 && !d500Button.isSelected())distanceIdx = 0;
                else return;

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = DEFAULT_COLORS[distanceIdx].hex;
                obj.setIcon(dotSprite);
            }
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var icon: DisplayObject = SpriteFactory.getFlashSprite("DOT_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, MiniMapGroupCity.CITY_DEFAULT_COLOR.r, MiniMapGroupCity.CITY_DEFAULT_COLOR.g, MiniMapGroupCity.CITY_DEFAULT_COLOR.b);
            legend.addToggleButton(cityButton, StringHelper.localize("MINIMAP_LEGEND_CITY"), icon);

            icon = SpriteFactory.getFlashSprite("DOT_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
            legend.addToggleButton(d100Button, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN", 100), icon);

            icon = SpriteFactory.getFlashSprite("DOT_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
            legend.addToggleButton(d200Button, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN", 200), icon);

            icon = SpriteFactory.getFlashSprite("DOT_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, DEFAULT_COLORS[2].r, DEFAULT_COLORS[2].g, DEFAULT_COLORS[2].b);
            legend.addToggleButton(d300Button, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN", 300), icon);

            icon = SpriteFactory.getFlashSprite("DOT_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
            legend.addToggleButton(d400Button, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN", 400), icon);

            icon = SpriteFactory.getFlashSprite("DOT_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
            legend.addToggleButton(d500Button, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_500"), icon);
        }

        public function addOnChangeListener(callback: Function): void {
            cityButton.addActionListener(callback);
            d100Button.addActionListener(callback);
            d200Button.addActionListener(callback);
            d300Button.addActionListener(callback);
            d400Button.addActionListener(callback);
            d500Button.addActionListener(callback);
        }
    }
}
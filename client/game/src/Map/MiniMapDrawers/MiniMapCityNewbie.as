package src.Map.MiniMapDrawers {
    import flash.display.*;
    import flash.geom.*;

    import org.aswing.JToggleButton;

    import src.Global;
    import src.Map.MiniMap.LegendGroups.MiniMapGroupCity;
    import src.Map.MiniMap.MiniMapLegendPanel;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Factories.SpriteFactory;
    import src.Util.StringHelper;

    import starling.display.Image;

    public class MiniMapCityNewbie implements IMiniMapObjectDrawer {
        private var cityButton: JToggleButton = new JToggleButton();
        private var newbieButton: JToggleButton = new JToggleButton();
        private var saltyButton: JToggleButton = new JToggleButton();

        private var DEFAULT_COLORS: * = MiniMapGroupCity.DEFAULT_COLORS;

        public function applyObject(obj: MiniMapRegionObject): void {
            var dotSprite: Image;

            if (Global.map.cities.get(obj.groupId)) {
                if (cityButton.isSelected()) {
                    return;
                }

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = MiniMapGroupCity.CITY_DEFAULT_COLOR.hex;
                obj.setIcon(dotSprite);
            } else if (obj.extraProps.isNewbie) {
                if (newbieButton.isSelected()) {
                    return;
                }

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = DEFAULT_COLORS[3].hex;
                obj.setIcon(dotSprite);
            } else {
                if (saltyButton.isSelected()) {
                    return;
                }

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = DEFAULT_COLORS[0].hex;
                obj.setIcon(dotSprite);
            }
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var icon: DisplayObject = SpriteFactory.getFlashSprite("DOT_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, MiniMapGroupCity.CITY_DEFAULT_COLOR.r, MiniMapGroupCity.CITY_DEFAULT_COLOR.g, MiniMapGroupCity.CITY_DEFAULT_COLOR.b);
            legend.addToggleButton(cityButton, StringHelper.localize("MINIMAP_LEGEND_CITY"), icon);

            icon = SpriteFactory.getFlashSprite("DOT_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
            legend.addToggleButton(newbieButton, StringHelper.localize("MINIMAP_LEGEND_NEWBIE_YES"), icon);

            icon = SpriteFactory.getFlashSprite("DOT_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
            legend.addToggleButton(saltyButton, StringHelper.localize("MINIMAP_LEGEND_NEWBIE_NO"), icon);
        }

        public function addOnChangeListener(callback: Function): void {
            cityButton.addActionListener(callback);
            newbieButton.addActionListener(callback);
            saltyButton.addActionListener(callback);
        }
    }
}
package src.Map.MiniMapDrawers {
    import flash.display.*;
    import flash.geom.*;

    import org.aswing.JToggleButton;

    import src.Constants;
    import src.Global;
    import src.Map.MiniMap.LegendGroups.MiniMapGroupCity;
    import src.Map.MiniMap.MiniMapLegendPanel;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Factories.SpriteFactory;
    import src.Util.StringHelper;

    import starling.display.Image;

    public class MiniMapCityTribe implements IMiniMapObjectDrawer {
        private var cityButton: JToggleButton = new JToggleButton();
        private var selfButton: JToggleButton = new JToggleButton();
        private var otherButton: JToggleButton = new JToggleButton();
        private var noneButton: JToggleButton = new JToggleButton();

        private var DEFAULT_COLORS: * = MiniMapGroupCity.DEFAULT_COLORS;

        public function applyObject(obj: MiniMapRegionObject): void {
            var dotSprite: Image;

            if (Global.map.cities.get(obj.groupId)) {
                if (cityButton.isSelected()) return;
                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = MiniMapGroupCity.CITY_DEFAULT_COLOR.hex;
                obj.setIcon(dotSprite);

            } else if (Constants.session.tribe.isInTribe(obj.extraProps.tribeId)) {
                if (selfButton.isSelected()) return;
                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = DEFAULT_COLORS[1].hex;
                obj.setIcon(dotSprite);
            } else if (obj.extraProps.tribeId > 0) {
                if (otherButton.isSelected()) return;
                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = DEFAULT_COLORS[4].hex;
                obj.setIcon(dotSprite);
            } else {
                if (noneButton.isSelected()) return;
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
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
            legend.addToggleButton(selfButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_SELF"), icon);

            icon = SpriteFactory.getFlashSprite("DOT_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
            legend.addToggleButton(otherButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_OTHER_IN_TRIBE"), icon);

            icon = SpriteFactory.getFlashSprite("DOT_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
            legend.addToggleButton(noneButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_OTHER_NO_TRIBE"), icon);

        }


        public function addOnChangeListener(callback: Function): void {
            cityButton.addActionListener(callback);
            selfButton.addActionListener(callback);
            otherButton.addActionListener(callback);
            noneButton.addActionListener(callback);
        }
    }
}
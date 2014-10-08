package src.FeathersUI.MiniMapDrawer.Drawers {

    import feathers.controls.ToggleButton;

    import src.Constants;
    import src.FeathersUI.MiniMap.MinimapDotIcon;
    import src.FeathersUI.MiniMapDrawer.LegendGroups.MiniMapGroupCity;
    import src.Global;
    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.Objects.Factories.SpriteFactory;
    import src.Util.StringHelper;

    import starling.display.Image;
    import starling.events.Event;

    public class MiniMapCityTribe implements IMiniMapObjectDrawer {
        private var cityButton: ToggleButton = new ToggleButton();
        private var selfButton: ToggleButton = new ToggleButton();
        private var otherButton: ToggleButton = new ToggleButton();
        private var noneButton: ToggleButton = new ToggleButton();

        private var DEFAULT_COLORS: * = MiniMapGroupCity.DEFAULT_COLORS;

        public function applyObject(obj: MiniMapRegionObject): void {
            var dotSprite: Image;

            if (Global.map.cities.get(obj.groupId)) {
                if (cityButton.isSelected) return;
                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = MiniMapGroupCity.CITY_DEFAULT_COLOR.hex;
                obj.setIcon(dotSprite);

            } else if (Constants.session.tribe.isInTribe(obj.extraProps.tribeId)) {
                if (selfButton.isSelected) return;
                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = DEFAULT_COLORS[1].hex;
                obj.setIcon(dotSprite);
            } else if (obj.extraProps.tribeId > 0) {
                if (otherButton.isSelected) return;
                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = DEFAULT_COLORS[4].hex;
                obj.setIcon(dotSprite);
            } else {
                if (noneButton.isSelected) return;
                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = DEFAULT_COLORS[0].hex;
                obj.setIcon(dotSprite);
            }
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var cityIcon: Image = new MinimapDotIcon(true, MiniMapGroupCity.CITY_DEFAULT_COLOR).dot;
            legend.addFilterButton(cityButton, StringHelper.localize("MINIMAP_LEGEND_CITY"), cityIcon);

            var selfIcon: Image = new MinimapDotIcon(true, MiniMapGroupCity.DEFAULT_COLORS[1]).dot;
            legend.addFilterButton(selfButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_SELF"), selfIcon);

            var otherIcon: Image = new MinimapDotIcon(true, MiniMapGroupCity.DEFAULT_COLORS[4]).dot;
            legend.addFilterButton(otherButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_OTHER_IN_TRIBE"), otherIcon);

            var noneIcon: Image = new MinimapDotIcon(true, MiniMapGroupCity.DEFAULT_COLORS[0]).dot;
            legend.addFilterButton(noneButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_OTHER_NO_TRIBE"), noneIcon);

        }

        public function addOnChangeListener(callback: Function): void {
            cityButton.addEventListener(Event.TRIGGERED, callback);
            selfButton.addEventListener(Event.TRIGGERED, callback);
            otherButton.addEventListener(Event.TRIGGERED, callback);
            noneButton.addEventListener(Event.TRIGGERED, callback);
        }
    }
}
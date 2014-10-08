package src.FeathersUI.MiniMapDrawer.Drawers {
    import feathers.controls.ToggleButton;

    import src.FeathersUI.MiniMap.MinimapDotIcon;

    import src.FeathersUI.MiniMapDrawer.LegendGroups.MiniMapGroupCity;

    import src.Global;
    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.Objects.Factories.SpriteFactory;
    import src.Util.StringHelper;

    import starling.display.Image;
    import starling.events.Event;

    public class MiniMapCityNewbie implements IMiniMapObjectDrawer {
        private var cityButton: ToggleButton = new ToggleButton();
        private var newbieButton: ToggleButton = new ToggleButton();
        private var veteranButton: ToggleButton = new ToggleButton();

        private var DEFAULT_COLORS: * = MiniMapGroupCity.DEFAULT_COLORS;

        public function applyObject(obj: MiniMapRegionObject): void {
            var dotSprite: Image;

            if (Global.map.cities.get(obj.groupId)) {
                if (cityButton.isSelected) {
                    return;
                }

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = MiniMapGroupCity.CITY_DEFAULT_COLOR.hex;
                obj.setIcon(dotSprite);
            } else if (obj.extraProps.isNewbie) {
                if (newbieButton.isSelected) {
                    return;
                }

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = DEFAULT_COLORS[3].hex;
                obj.setIcon(dotSprite);
            } else {
                if (veteranButton.isSelected) {
                    return;
                }

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = DEFAULT_COLORS[0].hex;
                obj.setIcon(dotSprite);
            }
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var cityIcon: Image = new MinimapDotIcon(true, MiniMapGroupCity.CITY_DEFAULT_COLOR).dot;
            legend.addFilterButton(cityButton, StringHelper.localize("MINIMAP_LEGEND_CITY"), cityIcon);

            var newbieIcon: Image = new MinimapDotIcon(true, MiniMapGroupCity.DEFAULT_COLORS[3]).dot;
            legend.addFilterButton(newbieButton, StringHelper.localize("MINIMAP_LEGEND_NEWBIE_YES"), newbieIcon);

            var veteranIcon: Image = new MinimapDotIcon(true, MiniMapGroupCity.DEFAULT_COLORS[0]).dot;
            legend.addFilterButton(veteranButton, StringHelper.localize("MINIMAP_LEGEND_NEWBIE_NO"), veteranIcon);
        }

        public function addOnChangeListener(callback: Function): void {
            cityButton.addEventListener(Event.TRIGGERED, callback);
            newbieButton.addEventListener(Event.TRIGGERED, callback);
            veteranButton.addEventListener(Event.TRIGGERED, callback);
        }
    }
}
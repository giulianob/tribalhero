package src.FeathersUI.MiniMapDrawer.Drawers {

    import feathers.controls.ToggleButton;

    import src.Constants;
    import src.FeathersUI.MiniMap.MiniMapDiamondIcon;
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
            if (Global.map.cities.get(obj.groupId)) {
                if (cityButton.isSelected) return;
                obj.setIcon(new MiniMapDiamondIcon(MiniMapGroupCity.CITY_DEFAULT_COLOR).dot);

            } else if (Constants.session.tribe.isInTribe(obj.extraProps.tribeId)) {
                if (selfButton.isSelected) return;
                obj.setIcon(new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[1]).dot);
            } else if (obj.extraProps.tribeId > 0) {
                if (otherButton.isSelected) return;
                obj.setIcon(new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[4]).dot);
            } else {
                if (noneButton.isSelected) return;
                obj.setIcon(new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[0]).dot);
            }
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var cityIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.CITY_DEFAULT_COLOR).dot;
            legend.addFilterButton(cityButton, StringHelper.localize("MINIMAP_LEGEND_CITY"), cityIcon);

            var selfIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[1]).dot;
            legend.addFilterButton(selfButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_SELF"), selfIcon);

            var otherIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[4]).dot;
            legend.addFilterButton(otherButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_OTHER_IN_TRIBE"), otherIcon);

            var noneIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[0]).dot;
            legend.addFilterButton(noneButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_OTHER_NO_TRIBE"), noneIcon);

        }

        public function addOnChangeListener(callback: Function): void {
            cityButton.addEventListener(Event.CHANGE, callback);
            selfButton.addEventListener(Event.CHANGE, callback);
            otherButton.addEventListener(Event.CHANGE, callback);
            noneButton.addEventListener(Event.CHANGE, callback);
        }
    }
}
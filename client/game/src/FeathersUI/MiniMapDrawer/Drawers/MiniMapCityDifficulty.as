package src.FeathersUI.MiniMapDrawer.Drawers {
    import feathers.controls.ToggleButton;

    import src.FeathersUI.MiniMap.MiniMapDiamondIcon;

    import src.FeathersUI.MiniMap.MinimapDotIcon;
    import src.FeathersUI.MiniMapDrawer.LegendGroups.MiniMapGroupCity;

    import src.FeathersUI.MiniMapDrawer.LegendGroups.MiniMapGroupCity;
    import src.Global;
    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.Objects.Factories.SpriteFactory;
    import src.Util.StringHelper;

    import starling.display.Image;
    import starling.events.Event;

    public class MiniMapCityDifficulty implements IMiniMapObjectDrawer {
        private var cityButton: ToggleButton = new ToggleButton();
        private var strongestButton: ToggleButton = new ToggleButton();
        private var strongButton: ToggleButton = new ToggleButton();
        private var normalButton: ToggleButton = new ToggleButton();
        private var weakButton: ToggleButton = new ToggleButton();
        private var weakestButton: ToggleButton = new ToggleButton();

        public function applyObject(obj: MiniMapRegionObject): void {
            if (Global.map.cities.get(obj.groupId)) {
                if (cityButton.isSelected) return;
                obj.setIcon(new MiniMapDiamondIcon(MiniMapGroupCity.CITY_DEFAULT_COLOR).dot);
            } else {
                // Apply the difficulty transformation to the tile
                var percDiff: Number = Number(obj.extraProps.value) / Math.max(1.0, Number(Global.gameContainer.selectedCity.value));
                var difficultyIdx: int;
                if (percDiff <= 0.2 && !weakestButton.isSelected) difficultyIdx = 0;
                else if (percDiff > .2 && percDiff <= 0.75 && !weakButton.isSelected) difficultyIdx = 1;
                else if (percDiff > .75 && percDiff <= 1.5 && !normalButton.isSelected) difficultyIdx = 2;
                else if (percDiff > 1.5 && percDiff <= 1.9 && !strongButton.isSelected) difficultyIdx = 3;
                else if (percDiff > 1.9 && !strongestButton.isSelected) difficultyIdx = 4;
                else return;

                obj.setIcon(new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[difficultyIdx]).dot);

            }
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var cityIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.CITY_DEFAULT_COLOR).dot;
            legend.addFilterButton(cityButton, StringHelper.localize("MINIMAP_LEGEND_CITY"), cityIcon);

            var strongestIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[4]).dot;
            legend.addFilterButton(strongestButton, StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_STRONGEST"), strongestIcon);

            var strongIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[3]).dot;
            legend.addFilterButton(strongButton, StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_STRONG"), strongIcon);

            var normalIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[2]).dot;
            legend.addFilterButton(normalButton, StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_NORMAL"), normalIcon);

            var weakIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[1]).dot;
            legend.addFilterButton(weakButton, StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_WEAK"), weakIcon);

            var weakestIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[0]).dot;
            legend.addFilterButton(weakestButton, StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_WEAKEST"), weakestIcon);
        }

        public function addOnChangeListener(callback: Function): void {
            cityButton.addEventListener(Event.CHANGE, callback);
            strongestButton.addEventListener(Event.CHANGE, callback);
            strongButton.addEventListener(Event.CHANGE, callback);
            normalButton.addEventListener(Event.CHANGE, callback);
            weakButton.addEventListener(Event.CHANGE, callback);
            weakestButton.addEventListener(Event.CHANGE, callback);
        }
    }
}
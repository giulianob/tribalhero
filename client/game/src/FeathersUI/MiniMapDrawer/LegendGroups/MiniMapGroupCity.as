package src.FeathersUI.MiniMapDrawer.LegendGroups {
    import feathers.controls.ToggleButton;

    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.FeathersUI.MiniMapDrawer.Drawers.*;
    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.Util.StringHelper;

    import starling.events.Event;

    public class MiniMapGroupCity {
        private var callback : Function;
        private var button: ToggleButton;
        private var filter : IMiniMapObjectDrawer = new MiniMapCityDifficulty();
        private var troopDrawer : MiniMapTroopDrawer = new MiniMapTroopDrawer();
        private var legendPanel : MiniMapLegendPanel = new MiniMapLegendPanel();

        public static const CITY_DEFAULT_COLOR: * = 0xFFFFFF;

        public static const DEFAULT_COLORS: Array = [
            0xc8c8c8,
            0x009c14,
            0xffff00,
            0xff6400,
            0xff0000
        ];

        public function MiniMapGroupCity() {
            button = new ToggleButton();
            button.label = StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY");
            button.addEventListener(Event.TRIGGERED, onFilterChange);

            troopDrawer.addOnChangeListener(onChange);
            filter.addOnChangeListener(onChange);

            draw();
        }

        private function onChange(e: Event = null): void {
            if (callback != null) {
                callback();
            }
        }

        private function onFilterChange(e:Event): void {
            if (button.label == StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY")) {
                button.label = StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT");
                filter = new MiniMapCityAlignment();
            } else if (button.label == StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT")) {
                button.label = StringHelper.localize("MINIMAP_LEGEND_DISTANCE");
                filter = new MiniMapCityDistance();
            } else if (button.label == StringHelper.localize("MINIMAP_LEGEND_DISTANCE")) {
                button.label = StringHelper.localize("MINIMAP_LEGEND_TRIBE");
                filter = new MiniMapCityTribe();
            } else if (button.label == StringHelper.localize("MINIMAP_LEGEND_TRIBE")) {
                button.label = StringHelper.localize("MINIMAP_LEGEND_NEWBIE");
                filter = new MiniMapCityNewbie();
            } else if (button.label == StringHelper.localize("MINIMAP_LEGEND_NEWBIE")) {
                button.label = StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY");
                filter = new MiniMapCityDifficulty();
            }

            filter.addOnChangeListener(onChange);

            draw();

            onChange();
        }

        public function applyCity(obj:MiniMapRegionObject):void {
            filter.applyObject(obj);
        }

        public function applyTroop(obj:MiniMapRegionObject):void {
            troopDrawer.applyObject(obj);
        }

        public function addOnChangeListener(callback:Function):void {
            this.callback = callback;
        }

        private function draw(): void {
            legendPanel.clear();
            legendPanel.addHeaderButton(button);
            filter.applyLegend(legendPanel);
            troopDrawer.applyLegend(legendPanel);
        }

        public function getLegendPanel(): MiniMapLegendPanel {
            return legendPanel;
        }
    }
}

package src.FeathersUI.MiniMapDrawer {
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.FeathersUI.MiniMapDrawer.LegendGroups.MiniMapGroupCity;
    import src.FeathersUI.MiniMapDrawer.LegendGroups.MiniMapGroupOther;
    import src.FeathersUI.MiniMapDrawer.LegendGroups.MiniMapGroupStronghold;

    import src.Objects.Factories.ObjectFactory;

    public class MiniMapDrawer {

        private var groupCity: MiniMapGroupCity = new MiniMapGroupCity();
        private var groupStronghold: MiniMapGroupStronghold = new MiniMapGroupStronghold();
        private var groupOther: MiniMapGroupOther = new MiniMapGroupOther();

        private var callback: Function = null;

        public function MiniMapDrawer() {
            groupCity.addOnChangeListener(onChange);
            groupStronghold.addOnChangeListener(onChange);
            groupOther.addOnChangeListener(onChange);
        }

        private function onChange(): void {
            if (callback != null) {
                callback();
            }
        }

        public function addOnChangeListener(callback: Function): void {
            this.callback = callback;
        }

        public function apply(obj: MiniMapRegionObject): void {
            obj.removeSprite();

            switch (obj.type) {
                case ObjectFactory.TYPE_CITY:
                    groupCity.applyCity(obj);
                    break;
                case ObjectFactory.TYPE_TROOP_OBJ:
                    groupCity.applyTroop(obj);
                    break;
                case ObjectFactory.TYPE_STRONGHOLD:
                    groupStronghold.applyStronghold(obj);
                    break;
                case ObjectFactory.TYPE_FOREST:
                    groupOther.applyForest(obj);
                    break;
                case ObjectFactory.TYPE_BARBARIAN_TRIBE:
                    groupOther.applyBarbarian(obj);
                    break;
            }
        }

        public function getLegendPanels(): Array {
            return [
                groupCity.getLegendPanel(),
                groupStronghold.getLegendPanel(),
                groupOther.getLegendPanel()
            ];
        }

    }
}
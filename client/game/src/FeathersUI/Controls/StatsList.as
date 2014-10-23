package src.FeathersUI.Controls {
    import feathers.controls.*;
    import feathers.controls.renderers.BaseDefaultItemRenderer;
    import feathers.controls.renderers.DefaultListItemRenderer;
    import feathers.controls.renderers.IListItemRenderer;
    import feathers.core.FeathersControl;
    import feathers.data.ListCollection;
    import feathers.layout.HorizontalLayout;

    import starling.display.Image;

    public class StatsList extends LayoutGroup {
        private var listData: ListCollection;

        public function StatsList() {
            listData = new ListCollection([]);
        }

        override protected function initialize(): void {
            this.layout = new HorizontalLayout();

            var list: List = new List();
            list.dataProvider = listData;
            list.isSelectable = false;
            list.itemRendererFactory = function():IListItemRenderer
            {
                var renderer:DefaultListItemRenderer = new DefaultListItemRenderer();
                renderer.layoutOrder = DefaultListItemRenderer.LAYOUT_ORDER_LABEL_ACCESSORY_ICON;
                renderer.accessoryGap = Number.POSITIVE_INFINITY;
                renderer.accessoryPosition = BaseDefaultItemRenderer.ACCESSORY_POSITION_RIGHT;
                return renderer;
            };
            addChild(list);
        }

        public function addLabel(statName: String, value: String, icon: Image = null): void {
            listData.addItem({
                label: statName,
                accessory: value,
                icon: icon
            });
        }

        public function addControl(statName: String, value: FeathersControl, icon: Image = null): void {
            listData.addItem({
                label: statName,
                accessory: value,
                icon: icon
            });
        }

        public function clear(): void {
            listData.removeAll();
        }
    }
}

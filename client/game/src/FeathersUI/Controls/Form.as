package src.FeathersUI.Controls {
    import feathers.controls.Button;
    import feathers.controls.LayoutGroup;
    import feathers.controls.List;
    import feathers.controls.renderers.BaseDefaultItemRenderer;
    import feathers.controls.renderers.DefaultListItemRenderer;
    import feathers.controls.renderers.IListItemRenderer;
    import feathers.core.FeathersControl;
    import feathers.data.ListCollection;
    import feathers.layout.HorizontalLayout;
    import feathers.layout.VerticalLayout;

    public class Form extends LayoutGroup {
        private var listData: ListCollection;
        private var footer: LayoutGroup;

        public function Form() {
            var layout: VerticalLayout = new VerticalLayout();
            layout.gap = 15;
            this.layout = layout;

            footer = new LayoutGroup();
            var footerLayout: HorizontalLayout = new HorizontalLayout();
            footerLayout.paddingRight = 15;
            footer.layout = footerLayout;

            listData = new ListCollection([]);

            var list: List = new List();
            list.dataProvider = listData;
            list.isSelectable = false;
            list.itemRendererFactory = function():IListItemRenderer
            {
                var renderer:DefaultListItemRenderer = new DefaultListItemRenderer();
                renderer.itemHasAccessory = true;
                renderer.accessoryGap = Number.POSITIVE_INFINITY;
                renderer.accessoryPosition = BaseDefaultItemRenderer.ACCESSORY_POSITION_RIGHT;
                return renderer;
            };

            list.validate();

            var listLayout: VerticalLayout = VerticalLayout(list.layout);
            listLayout.gap = 6;

            addChild(list);
            addChild(footer);
        }

        public function addButton(button: Button): void {
            this.footer.addChild(button);
        }

        public function addControl(label: String, control: FeathersControl): void {
            listData.addItem({
                label: label,
                accessory: control
            });
        }
    }
}

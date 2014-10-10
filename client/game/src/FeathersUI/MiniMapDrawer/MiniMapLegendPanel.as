package src.FeathersUI.MiniMapDrawer {
    import feathers.controls.Button;
    import feathers.controls.LayoutGroup;
    import feathers.controls.ToggleButton;
    import feathers.layout.VerticalLayout;

    import starling.display.Image;

    public class MiniMapLegendPanel extends LayoutGroup {

        public function MiniMapLegendPanel() {
            var layout: VerticalLayout = new VerticalLayout();
            layout.horizontalAlign = VerticalLayout.HORIZONTAL_ALIGN_JUSTIFY;

            this.layout = layout;
        }

        public function clear(): void {
            removeChildren();
        }

        public function addHeaderButton(button: Button): void {
            button.nameList.add(Button.ALTERNATE_NAME_FORWARD_BUTTON);
            addChildAt(button, 0);
        }

        public function addFilterButton(button: ToggleButton, text:String, icon: Image) : void {
            button.defaultIcon = icon;
            button.nameList.add(Button.ALTERNATE_NAME_QUIET_BUTTON);
            button.horizontalAlign = Button.HORIZONTAL_ALIGN_LEFT;
            button.defaultLabelProperties.wordWrap = true;

            if (text != null) {
                button.label = text;
            }

            addChild(button);
        }
    }
}

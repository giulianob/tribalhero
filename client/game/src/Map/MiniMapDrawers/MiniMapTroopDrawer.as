package src.Map.MiniMapDrawers {
    import flash.display.DisplayObject;
    import flash.geom.ColorTransform;

    import org.aswing.JToggleButton;

    import src.Constants;
    import src.Map.MiniMap.MiniMapLegendPanel;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Map.MiniMap.MinimapDotIcon;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Factories.SpriteFactory;

    public class MiniMapTroopDrawer implements IMiniMapObjectDrawer {
        private var friendToggleButton: JToggleButton = new JToggleButton();
        private var foeToggleButton: JToggleButton = new JToggleButton();

        public function applyObject(obj: MiniMapRegionObject): void {
            var icon: MinimapDotIcon;

            var friendly: Boolean = Constants.session.tribe.isInTribe(obj.extraProps.tribeId);
            if (friendly && !friendToggleButton.isSelected()) {
                icon = new MinimapDotIcon(false, 0xFFFF00);
            } else if (!friendly && !foeToggleButton.isSelected()) {
                icon = new MinimapDotIcon(false, 0x990000);
            }

            obj.setIcon(icon);
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var icon: DisplayObject = SpriteFactory.getFlashSprite("MINIMAP_SMALL_CIRCLE_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, 255, 255, 0);
            legend.addToggleButton(friendToggleButton, "Friendly Troop", icon);

            icon = SpriteFactory.getFlashSprite("MINIMAP_SMALL_CIRCLE_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, 153, 0, 0);
            legend.addToggleButton(foeToggleButton, "Enemy Troop", icon);
        }

        public function addOnChangeListener(callback: Function): void {
            friendToggleButton.addActionListener(callback);
            foeToggleButton.addActionListener(callback);
        }
    }
}

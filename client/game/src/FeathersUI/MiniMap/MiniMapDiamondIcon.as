package src.FeathersUI.MiniMap {
    import src.Objects.Factories.SpriteFactory;

    import starling.display.Image;
    import starling.display.Sprite;

    public class MiniMapDiamondIcon extends Sprite {
        private var _dot: Image;

        public function MiniMapDiamondIcon(color: uint) {
            _dot = SpriteFactory.getStarlingImage("DOT_SPRITE");
            _dot.color = color;

            addChild(_dot);
        }

        public function get dot(): Image {
            return _dot;
        }
    }
}

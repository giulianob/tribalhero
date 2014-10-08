package src.FeathersUI.MiniMap {
    import feathers.controls.Label;

    import src.Objects.Factories.SpriteFactory;

    import starling.display.*;

    public class MinimapDotIcon extends Sprite {
        private var _dot: Image;

        public function MinimapDotIcon(large: Boolean, color: uint, level: int = 0) {
            if (large) {
                _dot = SpriteFactory.getStarlingImage("MINIMAP_LARGE_CIRCLE_SPRITE");
            }
            else {
                _dot = SpriteFactory.getStarlingImage("MINIMAP_SMALL_CIRCLE_SPRITE");
            }

            _dot.color = color;
            addChild(_dot);

            if (level > 0) {
                var levelLabel: Label = new Label();
                levelLabel.text = level.toString();
                levelLabel.x = _dot.width - 3;
                levelLabel.y = _dot.height - 3;
                levelLabel.isEnabled = false;

                addChild(levelLabel);
            }
        }

        public function get dot(): Image {
            return _dot;
        }
    }
}

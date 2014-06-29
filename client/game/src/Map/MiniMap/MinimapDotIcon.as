package src.Map.MiniMap {
    import feathers.controls.Label;

    import starling.display.*;

    public class MinimapDotIcon extends Sprite {
        public function MinimapDotIcon(radius: int, color: uint, level: int = 0) {
            var g: Graphics = new Graphics(this);
            g.beginFill(color, 0.5);
            g.drawCircle(0, 0, radius);
            g.endFill();

            if (level > 0) {
                var levelLabel: Label = new Label();
                levelLabel.text = level.toString();
                levelLabel.x = radius*2 - 3;
                levelLabel.y = radius*2 - 3;
            }
        }
    }
}

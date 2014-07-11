package src.Map.MiniMap {
    import feathers.controls.Label;

    import src.Objects.Factories.SpriteFactory;
    import src.UI.LookAndFeel.TribalHeroUITheme;

    import starling.display.*;

    public class MinimapDotIcon extends Sprite {
        public function MinimapDotIcon(large: Boolean, color: uint, level: int = 0) {

            var dot: Image;
            if (large) {
                dot = SpriteFactory.getStarlingImage("MINIMAP_LARGE_CIRCLE_SPRITE");
            }
            else {
                dot = SpriteFactory.getStarlingImage("MINIMAP_SMALL_CIRCLE_SPRITE");
            }

            dot.color = color;
            addChild(dot);

            if (level > 0) {
                var levelLabel: Label = new Label();
                levelLabel.text = level.toString();
                levelLabel.x = dot.width - 3;
                levelLabel.y = dot.height - 3;
                levelLabel.nameList.add(TribalHeroUITheme.LABEL_STYLE_MUTED);

                addChild(levelLabel);
            }
        }
    }
}

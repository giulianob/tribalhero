package src.UI.Components {
    import feathers.controls.Label;

    import src.Objects.Factories.SpriteFactory;
    import src.UI.LookAndFeel.TribalHeroUITheme;

    import starling.display.*;

    public class CountBubble extends Sprite {
        private var _count: int;
        private var lblCount: Label;
        private var bubbleWidth: Number;
        private var bubbleHeight: Number;

        public function CountBubble(count: int) {
            super();

            touchable = false;

            var bg: Image = SpriteFactory.getStarlingImage("COUNT_BUBBLE");
            bubbleWidth = bg.width;
            bubbleHeight = bg.height;
            addChild(bg);

            this.lblCount = new Label();
            addChild(lblCount);

            this.count = count;
        }

        public function set count(count: int): void {
            _count = count;

            visible = count > 1;

            lblCount.nameList.add(TribalHeroUITheme.LABEL_STYLE_TOOLTIP);
            lblCount.nameList.add(TribalHeroUITheme.LABEL_STYLE_HEADING);
            lblCount.text = count > 9 ? "!" : count.toString();
            lblCount.validate();

            lblCount.x = (bubbleWidth - lblCount.width)/2;
            lblCount.y = (bubbleHeight - lblCount.height)/2;
        }
    }
}

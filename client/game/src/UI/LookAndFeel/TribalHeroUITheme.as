package src.UI.LookAndFeel {
    import feathers.controls.Label;
    import feathers.themes.AeonDesktopTheme;

    import flash.text.TextFormat;
    import flash.text.TextFormatAlign;

    public class TribalHeroUITheme extends AeonDesktopTheme {

        protected static const FONT_NAME:String = "Arial";

        public static const LABEL_STYLE_TOOLTIP:String = "label-style-tooltip";
        public static const LABEL_STYLE_HEADING:String = "label-style-heading";
        public static const LABEL_STYLE_MUTED:String = "label-style-muted";

        override protected function initializeRoot(): void {
        }

        override protected function initialize():void
        {
            super.initialize();

            this.setInitializerForClass(Label, function(label: Label):void {
                var tf: TextFormat = createTextFormat(this.defaultTextFormat, label.textRendererProperties.textFormat);
                tf.color = 0xFFFFFF;

                label.textRendererProperties.textFormat = label.textRendererProperties.disabledTextFormat = tf;
            }, LABEL_STYLE_TOOLTIP);

            this.setInitializerForClass(Label, function(label: Label):void {
                var tf: TextFormat = createTextFormat(this.defaultTextFormat, label.textRendererProperties.textFormat);
                tf.color = 0x777777;

                label.textRendererProperties.textFormat = label.textRendererProperties.disabledTextFormat = tf;
            }, LABEL_STYLE_MUTED);

            this.setInitializerForClass(Label, function(label: Label):void {
                var tf: TextFormat = createTextFormat(this.defaultTextFormat, label.textRendererProperties.textFormat);
                tf.bold = true;

                label.textRendererProperties.textFormat = label.textRendererProperties.disabledTextFormat = tf;
            }, LABEL_STYLE_HEADING);
        }

        override protected function initializeFonts(): void {
            super.initializeFonts();

            this.defaultTextFormat = new TextFormat(FONT_NAME, 11, PRIMARY_TEXT_COLOR, false, false, false, null, null, TextFormatAlign.LEFT, 0, 0, 0, 0);
            this.disabledTextFormat = new TextFormat(FONT_NAME, 11, DISABLED_TEXT_COLOR, false, false, false, null, null, TextFormatAlign.LEFT, 0, 0, 0, 0);
            this.headerTitleTextFormat = new TextFormat(FONT_NAME, 12, PRIMARY_TEXT_COLOR, false, false, false, null, null, TextFormatAlign.LEFT, 0, 0, 0, 0);
            this.headingTextFormat = new TextFormat(FONT_NAME, 14, PRIMARY_TEXT_COLOR, false, false, false, null, null, TextFormatAlign.LEFT, 0, 0, 0, 0);
            this.headingDisabledTextFormat = new TextFormat(FONT_NAME, 14, DISABLED_TEXT_COLOR, false, false, false, null, null, TextFormatAlign.LEFT, 0, 0, 0, 0);
            this.detailTextFormat = new TextFormat(FONT_NAME, 10, PRIMARY_TEXT_COLOR, false, false, false, null, null, TextFormatAlign.LEFT, 0, 0, 0, 0);
            this.detailDisabledTextFormat = new TextFormat(FONT_NAME, 10, DISABLED_TEXT_COLOR, false, false, false, null, null, TextFormatAlign.LEFT, 0, 0, 0, 0);
        }

        private function createTextFormat(base: TextFormat, existingTf: TextFormat): TextFormat
        {
            if (existingTf) {
                return existingTf;
            }

            return new TextFormat(base.font, base.size, base.color, base.bold, base.italic, base.underline, base.url, base.target, base.align, base.leftMargin, base.rightMargin, base.indent, base.leading);
        }
    }
}

package src.FeathersUI.Controls {
    import feathers.controls.LayoutGroup;

    import flash.geom.Rectangle;

    // Always keeps the content clipped to its width/height
    public class ClippedLayoutGroup extends LayoutGroup {
        override protected function draw(): void {
            super.draw();

            this.clipRect = new Rectangle(0, 0, width, height);
        }
    }
}

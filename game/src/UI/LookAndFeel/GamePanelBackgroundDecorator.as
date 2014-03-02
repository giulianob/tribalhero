
package src.UI.LookAndFeel {

    import flash.display.DisplayObject;
    import flash.display.Sprite;

    import org.aswing.*;
    import org.aswing.geom.IntRectangle;
    import org.aswing.graphics.Graphics2D;
    import org.aswing.plaf.ComponentUI;
    import org.aswing.plaf.DefaultsDecoratorBase;
    import org.aswing.plaf.UIResource;

    public class GamePanelBackgroundDecorator extends DefaultsDecoratorBase implements GroundDecorator, UIResource{

		protected var imageContainer:Sprite;
		protected var bg:DisplayObject;
		protected var bgKeyName: String;

		public function GamePanelBackgroundDecorator(bgKeyName: String) {
			this.bgKeyName = bgKeyName;
			imageContainer = AsWingUtils.createSprite(null, "bgContainer");
		}

		private function reloadAssets(ui:ComponentUI):void {
			bg = ui.getInstance(bgKeyName) as DisplayObject;
			imageContainer.addChild(bg);
		}

		public function updateDecorator(c:Component, g:Graphics2D, bounds:IntRectangle):void{
			if(bg == null) {
				reloadAssets(getDefaultsOwner(c));
			}
			bg.width = c.width;
			bg.height = c.height;
		}

		public function getDisplay(c:Component):DisplayObject{
			return imageContainer;
		}

	}
}


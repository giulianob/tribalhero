/*
Copyright aswing.org, see the LICENCE.txt.
*/

package src.UI{

    import flash.display.DisplayObject;
    import flash.display.Sprite;

    import org.aswing.*;
    import org.aswing.geom.IntRectangle;
    import org.aswing.graphics.Graphics2D;
    import org.aswing.plaf.ComponentUI;
    import org.aswing.plaf.DefaultsDecoratorBase;
    import org.aswing.plaf.UIResource;
    import org.aswing.skinbuilder.SkinEmptyBorder;

    /**
	 * @private
	 */
	public class GameJImagePanelBackground extends DefaultsDecoratorBase implements GroundDecorator, UIResource{

		protected var imageBackground: DisplayObject;
		protected var imageContainer:Sprite;
		protected var activeBG:DisplayObject;
		protected var inactiveBG:DisplayObject;

		public function GameJImagePanelBackground(imageBackground: DisplayObject) {
			this.imageBackground = imageBackground;
			imageContainer = AsWingUtils.createSprite(null, "bgContainer");
		}

		private function reloadAssets(ui:ComponentUI):void{
			activeBG = ui.getInstance("Frame.activeBG") as DisplayObject;
			inactiveBG = ui.getInstance("Frame.inactiveBG") as DisplayObject;
			imageContainer.addChild(activeBG);
			imageContainer.addChild(inactiveBG);
			imageContainer.addChild(imageBackground);
			inactiveBG.visible = false;
		}

		public function updateDecorator(c:Component, g:Graphics2D, bounds:IntRectangle):void{
			if(activeBG == null){
				reloadAssets(getDefaultsOwner(c));
			}
			var frame:JFrame = JFrame(c);

			activeBG.visible = frame.getFrameUI().isPaintActivedFrame();
			inactiveBG.visible = !frame.getFrameUI().isPaintActivedFrame();
			//not use bounds, avoid the border
			activeBG.width = inactiveBG.width = c.width;
			activeBG.height = inactiveBG.height = c.height;

			var border: SkinEmptyBorder = UIManager.getDefaults().get("Frame.borderWithoutPaper");
			imageBackground.x = border.getLeft();
			imageBackground.y = border.getTop();
			imageBackground.width = c.width - border.getLeft() - border.getRight();
			imageBackground.height = c.height - border.getTop() - border.getBottom();
		}

		public function getDisplay(c:Component):DisplayObject{
			return imageContainer;
		}

		public static function getFrameWidth() : int {			
			return 20;
		}
		
		public static function getFrameHeight(topAndBottom: Boolean = true) : int {
			var border: SkinEmptyBorder = UIManager.getDefaults().get("Frame.borderWithoutPaper");
			return border.getTop() * (topAndBottom ? 2 : 1);
		}
	}
}


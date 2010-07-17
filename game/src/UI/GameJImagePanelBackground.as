/*
Copyright aswing.org, see the LICENCE.txt.
*/

package src.UI{

	import flash.display.DisplayObject;
	import org.aswing.plaf.DefaultsDecoratorBase;

	import org.aswing.Component;
	import org.aswing.GroundDecorator;
	import org.aswing.geom.IntRectangle;
	import org.aswing.graphics.Graphics2D;
	import org.aswing.plaf.UIResource;

	/**
	 * @private
	 */
	public class GameJImagePanelBackground extends DefaultsDecoratorBase implements GroundDecorator, UIResource{

		protected var imageBackground: DisplayObject;

		public function GameJImagePanelBackground(imageBackground: DisplayObject) {
			this.imageBackground = imageBackground;
		}

		public function getDisplay(c:Component):DisplayObject{
			return imageBackground;
		}

		public function updateDecorator(c:Component, g:Graphics2D, b:IntRectangle):void{
			imageBackground.x = 0;
			imageBackground.y = 0;
			imageBackground.width = c.width;
			imageBackground.height = c.height;
		}

	}
}


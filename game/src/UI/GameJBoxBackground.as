/*
 Copyright aswing.org, see the LICENCE.txt.
*/

package src.UI{

import flash.display.DisplayObject;
import flash.display.Shape;
import flash.display.Sprite;
import flash.filters.BevelFilter;
import flash.filters.BitmapFilterType;
import flash.filters.DropShadowFilter;
import org.aswing.graphics.Pen;
import org.aswing.graphics.SolidBrush;
import org.aswing.UIManager;

import org.aswing.ASColor;
import org.aswing.Component;
import org.aswing.GroundDecorator;
import org.aswing.JFrame;
import org.aswing.StyleResult;
import org.aswing.StyleTune;
import org.aswing.geom.IntRectangle;
import org.aswing.graphics.Graphics2D;
import org.aswing.plaf.FrameUI;
import org.aswing.plaf.UIResource;
import org.aswing.plaf.basic.BasicGraphicsUtils;

/**
 * @private
 */
public class GameJBoxBackground implements GroundDecorator, UIResource{
	
	protected var shape:Sprite;
	
	public function GameJBoxBackground(){
		shape = new Sprite();		
		shape.mouseChildren = false;
		shape.mouseEnabled = false;
	}

	public function getDisplay(c:Component):DisplayObject{
		return shape;
	}
	
	public function updateDecorator(c:Component, g:Graphics2D, b:IntRectangle):void{
		shape.graphics.clear();
		
		if(c.isOpaque()){			
			var g: Graphics2D = new Graphics2D(shape.graphics);
			shape.alpha = 0.8;
			g.fillRoundRect(new SolidBrush(ASColor.BLACK), b.x, b.y, b.width, b.height, 10);			
		}
		shape.visible = c.isOpaque();
	}
	
}
}
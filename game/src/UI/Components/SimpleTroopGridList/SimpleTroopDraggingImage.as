package src.UI.Components.SimpleTroopGridList 
{
	import flash.display.Sprite;
	import org.aswing.ASColor;
	import org.aswing.AssetIcon;
	import org.aswing.graphics.Graphics2D;
	import org.aswing.graphics.Pen;
	import org.aswing.JLabel;
	import org.aswing.dnd.DraggingImage;
	import flash.display.Shape;
	import org.aswing.Component;
	import flash.display.DisplayObject;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.Troop.*;
	
/**
 * ...
 * @author Giuliano
 */
public class SimpleTroopDraggingImage implements DraggingImage
{
		
	private var image:Sprite;
	private var width:int;
	private var height:int;
	private var troopGridCell: SimpleTroopGridCell;
	private var icon: DisplayObject;
	private var redX: Shape;
	
	public function SimpleTroopDraggingImage(dragInitiator:Component){
		width = dragInitiator.width;
		height = dragInitiator.height;
			
		troopGridCell = SimpleTroopGridCell(dragInitiator);

		image = new Sprite();
		
		redX = new Shape();
		var r:Number = Math.min(width, height) - 2;
		var x:Number = 0;
		var y:Number = 0;
		var w:Number = width;
		var h:Number = height;
		var g:Graphics2D = new Graphics2D(redX.graphics);
		g.drawLine(new Pen(ASColor.RED, 2), x+1, y+1, x+1+r, y+1+r);
		g.drawLine(new Pen(ASColor.RED, 2), x+1+r, y+1, x+1, y+1+r);
		g.drawRectangle(new Pen(ASColor.GRAY), x, y, w, h);		
			
		icon = UnitFactory.getSprite(troopGridCell.getCellValue().data.type, troopGridCell.getCellValue().level) as DisplayObject;				
	}
	
	public function getDisplay():DisplayObject
	{
		return image;		
	}
	
	public function switchToRejectImage():void
	{					
		for (var i: int = 0; i < image.numChildren; i++)
		{
			if (image.getChildAt(i) == icon) {
				image.removeChildAt(i);
				break;
			}
		}
			
		image.addChild(redX);
	}
	
	public function switchToAcceptImage():void
	{				
		for (var i: int = 0; i < image.numChildren; i++)
		{
			if (image.getChildAt(i) == redX) {
				image.removeChildAt(i);
				break;
			}
		}
	
		image.addChild(icon);
	}
		
}
	
}
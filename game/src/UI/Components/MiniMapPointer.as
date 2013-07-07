package src.UI.Components
{
	import flash.display.DisplayObject;
	import flash.display.MovieClip;
	import flash.display.Sprite;
	import flash.events.Event;
	import flash.geom.Point;
	import org.aswing.JLabel;
	import src.Constants;
	import src.Map.TileLocator;
	import com.greensock.*;
	import com.greensock.easing.*;
	import src.UI.Tooltips.Tooltip;
	import src.Util.StringHelper;
	import flash.events.MouseEvent;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import org.aswing.SoftBoxLayout;
	import org.aswing.AsWingConstants;
	
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class MiniMapPointer extends Sprite
	{
		private var cityMinimapPoint:Point;
		private var center:Point;
		private var lastWidth:int;
		private var lastHeight:int;
		
		private var pointer:DisplayObject;
		private var pointerName:String;
		
		private var tooltip:Tooltip;
		private var tooltipCityLabel:JLabel;
		private var tooltipDistanceLabel:JLabel;
		
		public function MiniMapPointer(x:int, y:int, name:String)
		{
			cityMinimapPoint = TileLocator.getMiniMapScreenCoord(x, y);
			pointerName = name;
			pointer = new ICON_MINIMAP_ARROW_BLUE();
			addChild(pointer);
			
			tooltip = new Tooltip();
			tooltipCityLabel = new JLabel(name, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(tooltipCityLabel, "header");
			tooltipDistanceLabel = new JLabel("", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(tooltipDistanceLabel, "Tooltip.italicsText");
			tooltip.getUI().setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL));
			tooltip.getUI().appendAll(tooltipCityLabel, tooltipDistanceLabel);
			addEventListener(MouseEvent.MOUSE_MOVE, onRollOver);
			addEventListener(MouseEvent.ROLL_OUT, onRollOut);
		}
		
		public function getPointerName():String
		{
			return pointerName;
		}
		
		public function setIcon(icon:MovieClip):void
		{
			this.removeChild(pointer);
			pointer = icon;
			addChild(pointer);
			if (center != null)
				update(center, lastWidth, lastHeight);
		}
		
		private function onRollOver(e:Event):void
		{
			var distance:int = TileLocator.distance(cityMinimapPoint.x / Constants.miniMapTileW, cityMinimapPoint.y, (center.x + x - lastWidth / 2) / Constants.miniMapTileW, (center.y + y - lastHeight / 2));
			tooltipDistanceLabel.setText(distance + " tiles away");
			tooltip.show(pointer);
		}
		
		private function onRollOut(e:Event = null):void
		{
			tooltip.hide();
		}
		
		public function update(center:Point, mapWidth:int, mapHeight:int):void
		{
			this.center = center;
			this.lastWidth = mapWidth;
			this.lastHeight = mapHeight;
			
			var dx:int = cityMinimapPoint.x - center.x;
			var dy:int = cityMinimapPoint.y - center.y;
			
			if (Math.abs(dx) <= mapWidth / 2 && Math.abs(dy) <= mapHeight / 2)
			{
				pointer.visible = false;
				return;
			}
			
			var angleRadian:Number = Math.atan2(dy, dx);
			var angleDegree:Number = angleRadian * 180 / Math.PI;
			
			var radiusX:Number = mapWidth / 2 - 20;
			var radiusY:Number = mapHeight / 2 - 20;
			
			var yBasedOnXRadius:Number = Math.abs(Math.tan(angleRadian) * radiusX);
			var xBasedOnYRadius:Number = Math.abs(radiusY / Math.tan(angleRadian));
			
			var yBasedOnXRadiusWithinLimit:Boolean = Math.abs(yBasedOnXRadius) < radiusY;
			var xBasedOnYRadiusWithinLimit:Boolean = Math.abs(xBasedOnYRadius) < radiusX;
			
			if (yBasedOnXRadiusWithinLimit && xBasedOnYRadiusWithinLimit)
			{
				pointer.visible = false;
				return;
			}
			else if (yBasedOnXRadiusWithinLimit)
			{
				pointer.visible = true;
				x = (int)(mapWidth / 2 + (dx > 0 ? radiusX : -radiusX - 15));
				y = (int)(mapHeight / 2 + (dy > 0 ? 1 : -1) * yBasedOnXRadius);
			}
			else if (xBasedOnYRadiusWithinLimit)
			{
				pointer.visible = true;
				x = (int)(mapWidth / 2 + (dx > 0 ? 1 : -1) * xBasedOnYRadius);
				y = (int)(mapHeight / 2 + (dy > 0 ? radiusY : -radiusY - 15)); //
			}
			TweenMax.to(pointer, 0, {transformAroundCenter: {shortRotation: {rotation: angleDegree}}});
		}
	}

}
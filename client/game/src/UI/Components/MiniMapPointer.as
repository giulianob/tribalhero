package src.UI.Components
{
    import com.greensock.*;

    import feathers.controls.Label;
    import feathers.controls.LayoutGroup;
    import feathers.layout.VerticalLayout;

    import flash.geom.Point;

    import src.Constants;
    import src.FeathersUI.Controls.ResponsiveTooltip;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.Factories.SpriteFactory;

    import starling.display.*;
    import starling.utils.deg2rad;

    public class MiniMapPointer extends Sprite
	{
		private var cityMinimapPoint:ScreenPosition;
		private var center:Point;
		private var lastWidth:int;
		private var lastHeight:int;
		
		private var pointer:DisplayObject;
		private var pointerName:String;
        private var _cityId: int;
		
		private var tooltip:ResponsiveTooltip;
        private var tooltipHeaderLabel: Label;
        private var tooltipDistanceLabel: Label;
        private var tooltipContainer: LayoutGroup;

		public function MiniMapPointer(x:int, y:int, name:String, cityId: int) {
            _cityId = cityId;
            cityMinimapPoint = TileLocator.getMiniMapScreenCoord(x, y);
            pointerName = name;
            pointer = SpriteFactory.getStarlingImage("ICON_MINIMAP_ARROW_BLUE");
            addChild(pointer);

            tooltip = new ResponsiveTooltip(getTooltipContent, this);
            tooltip.bind();
        }

        private function getTooltipContent(): LayoutGroup {
            if (!tooltipHeaderLabel) {
                tooltipHeaderLabel = new Label();
                tooltipDistanceLabel = new Label();

                tooltipContainer = new LayoutGroup();
                tooltipContainer.addChild(tooltipHeaderLabel);
                tooltipContainer.addChild(tooltipDistanceLabel);

                var tooltipContainerLayout: VerticalLayout = new VerticalLayout();
                tooltipContainerLayout.gap = 0;
                tooltipContainer.layout = tooltipContainerLayout;
            }

            var distance: int = TileLocator.distance(
                            cityMinimapPoint.x / Constants.miniMapTileW, cityMinimapPoint.y, 1,
                            (center.x + x - lastWidth / 2) / Constants.miniMapTileW, (center.y + y - lastHeight / 2), 1);

            tooltipDistanceLabel.text = pointerName;
            tooltipHeaderLabel.text = distance + " tiles away";

            return tooltipContainer;
        }
		
		public function setIcon(icon:DisplayObject):void
		{
			this.removeChild(pointer);
			pointer = icon;
			addChild(pointer);
			if (center != null)
				update(center, lastWidth, lastHeight);
		}

//        private function onTouch(e: TouchEvent): void {
//            if (e.getTouch(this, TouchPhase.HOVER)) {
//                var distance:int = TileLocator.distance(
//                        cityMinimapPoint.x / Constants.miniMapTileW, cityMinimapPoint.y, 1,
//                        (center.x + x - lastWidth / 2) / Constants.miniMapTileW, (center.y + y - lastHeight / 2), 1);
//
//                tooltipDistanceLabel.setText(distance + " tiles away");
//                tooltip.show(pointer);
//            }
//
//            var endedTouch: Touch = e.getTouch(this, TouchPhase.ENDED);
//            if (endedTouch && !this.hitTest(endedTouch.getLocation(this))) {
//                tooltip.hide();
//            }
//        }

		public function update(center:Point, miniMapWidth:int, miniMapHeight:int):void
		{
			this.center = center;
			this.lastWidth = miniMapWidth;
			this.lastHeight = miniMapHeight;
			
			var dx:int = cityMinimapPoint.x - center.x;
			var dy:int = cityMinimapPoint.y - center.y;
			
			if (Math.abs(dx) <= miniMapWidth / 2 && Math.abs(dy) <= miniMapHeight / 2)
			{
				pointer.visible = false;
				return;
			}
			
			var angleRadian:Number = Math.atan2(dy, dx);
			var angleDegree:Number = angleRadian * 180 / Math.PI;
			
			var radiusX:Number = miniMapWidth / 2 - 20;
			var radiusY:Number = miniMapHeight / 2 - 20;
			
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
				x = (int)(miniMapWidth / 2 + (dx > 0 ? radiusX : -radiusX - 15));
				y = (int)(miniMapHeight / 2 + (dy > 0 ? 1 : -1) * yBasedOnXRadius);
			}
			else if (xBasedOnYRadiusWithinLimit)
			{
				pointer.visible = true;
				x = (int)(miniMapWidth / 2 + (dx > 0 ? 1 : -1) * xBasedOnYRadius);
				y = (int)(miniMapHeight / 2 + (dy > 0 ? radiusY : -radiusY - 15)); //
			}
			TweenMax.to(pointer, 0, {transformAroundCenterStarling: {shortRotation: {rotation: deg2rad(angleDegree), useRadians: true}}});
		}

        public function get cityId(): int {
            return _cityId;
        }
    }

}
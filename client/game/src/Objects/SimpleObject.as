
package src.Objects {

    import com.greensock.TweenMax;

    import flash.geom.Point;

    import src.UI.Components.CountBubble;

    import starling.display.*;

    import src.Constants;
    import src.Map.ScreenPosition;

    import starling.filters.BlurFilter;

    public class SimpleObject extends Sprite {
		
		public static const DISPOSED: String = "DISPOSED";
		
		public var disposed: Boolean;
				
		private var onSelect: Function;
		protected var selected: Boolean;						

        private var objectCount: int = 1;

		private var spriteContainer: Sprite;
        protected var spritePosition: Point;

        public var primaryPosition: ScreenPosition = new ScreenPosition();

        public var size: int;

        public var mapPriority: int;

        public var isHighestPriority: Boolean;

        private var _sprite: Image;

		public function SimpleObject(objX: int, objY: int, size: int) {
			super();

            touchable = true;
            mapPriority = Constants.mapObjectPriority.simpleObject;
			spriteContainer = new Sprite();
			addChild(spriteContainer);

            countBubble = new src.UI.Components.CountBubble(objectCount);
            addChild(countBubble);
			
			this.primaryPosition.x = objX;
            this.primaryPosition.y = objY;

            this.x = objX;
            this.y = objY;

            this.size = size;
		}
		
		public function copy(obj: SimpleObject): void {
			for (var i: int = spriteContainer.numChildren - 1; i >= 0; i--) {
				spriteContainer.removeChildAt(i);				
			}
			
			for (i = obj.spriteContainer.numChildren - 1; i >= 0; i--) {
				spriteContainer.addChildAt(obj.spriteContainer.removeChildAt(i), 0);				
			}

            primaryPosition.x = obj.primaryPosition.x;
            primaryPosition.y = obj.primaryPosition.y;
			x = obj.x;
			y = obj.y;
		}

        public function getObjectCount(): int {
            return objectCount;
        }

        private var countBubble: src.UI.Components.CountBubble;

        public function setObjectCount(count: int) : void {
            objectCount = count;

            countBubble.count = count;
            countBubble.x = spritePosition.x - countBubble.width;
            countBubble.y = 20;
        }

		public override function dispose(): void {
            dispatchEventWith(DISPOSED);

            disposeFilter();

            disposed = true;

            super.dispose();
		}
		
		public function fadeIn(startFromCurrentAlpha: Boolean = false):void
		{
            if (!startFromCurrentAlpha) {
                alpha = 0;
            }

            visible = true;
			TweenMax.to(this, 1, {alpha:1});
		}

		public function setOnSelect(callback: Function):void
		{
			onSelect = callback;
		}
		
		public function isSelectable(): Boolean {
			return onSelect != null;
		}

		public function setSelected(bool: Boolean = false):void
		{
            disposeFilter();
			if (bool) {
				filter = BlurFilter.createGlow(0xFFFFFF, 0.9, 5);
			}
			
			selected = bool;
		}

		public function setHighlighted(bool: Boolean = false):void
		{
			if (selected) {
				return;
            }

            disposeFilter();
            if (bool != false) {
                filter = BlurFilter.createGlow(0xFFDD00, 0.9, 5);
            }
		}		
		
		public function distance(x_1: int, y_1: int): int
		{
			var offset: int = 0;

			var objPos: Point = new Point();

			objPos.x = primaryPosition.x;
			objPos.y = primaryPosition.y;

			if (objPos.y % 2 == 1 && y_1 % 2 == 0 && x_1 <= objPos.x) offset = 1;
			if (objPos.y % 2 == 0 && y_1 % 2 == 1 && x_1 >= objPos.x) offset = 1;

			return ((x_1 > objPos.x ? x_1 - objPos.x : objPos.x - x_1) + (y_1 > objPos.y ? y_1 - objPos.y : objPos.y - y_1) / 2 + offset);
		}		
		
		public static function sortOnXandY(a: SimpleObject, b: SimpleObject):Number {
			var aX:Number = a.primaryPosition.x;
			var bX:Number = b.primaryPosition.x;

			var aY:Number = a.primaryPosition.y;
			var bY:Number = b.primaryPosition.y;
			
			if (aX > bX)
				return 1;
			else if (aX < bX)
				return -1;
			else if (aY > bY)
				return 1;
			else if (aY < bY)
				return -1;
			else
				return 0;
		}		
		
		public static function compareXAndY(a: SimpleObject, value: Array):int
		{
			var xDelta: int = a.primaryPosition.x - value[0];
			var yDelta: int = a.primaryPosition.y - value[1];
			
			if (xDelta != 0)
				return xDelta;
				
			if (yDelta != 0)
				return yDelta;
			else
				return 0;
		}

        public function setSprite(sprite: Image, spritePosition: Point): void {
            sprite.x = spritePosition.x;
            sprite.y = spritePosition.y;

            this.spritePosition = spritePosition;

            spriteContainer.removeChildren();
            spriteContainer.addChild(sprite);

            this._sprite = sprite;

            setObjectCount(objectCount);
        }

        override public function get x(): Number {
            return super.x;
        }

        override public function set x(value: Number): void {
            super.x = int(value);
        }

        override public function get y(): Number {
            return super.y;
        }

        override public function set y(value: Number): void {
            super.y = int(value);
        }

        public function dim(): void {
            TweenMax.to(this, 1, { alpha: 0.25 } );
        }

        public function setVisibilityPriority(isHighestPriority: Boolean, objectsInTile: Array): void {
            visible = isHighestPriority;
            this.isHighestPriority = isHighestPriority;
        }

        protected function disposeFilter(): void {
            if (filter != null) {
                filter.dispose();
                filter = null;
            }
        }

        public function get sprite(): Image {
            return _sprite;
        }
    }
	
}

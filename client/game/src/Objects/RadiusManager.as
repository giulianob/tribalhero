package src.Objects 
{
    import starling.events.Event;

    import src.Map.ScreenPosition;
    import src.UI.Components.GroundCircle;

	public class RadiusManager
	{
		private var radiusVisible: Boolean = false;
		private var parentObj: SimpleGameObject;
		private var circle: GroundCircle;
		private var radius: int;
		
		public function RadiusManager(parentObj: SimpleGameObject) 
		{
			this.parentObj = parentObj;
            this.circle = new GroundCircle(radius, new ScreenPosition(), GroundCircle.GREEN, true);
		}
		
		public function showRadius(radius: int):void {
			radiusVisible = true;
			this.radius = radius;			
			moveRadius();
			parentObj.addEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);
		}

		public function hideRadius():void {
			radiusVisible = false;
			moveRadius();
			parentObj.removeEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);
		}
		
		private function onObjectUpdate(e: Event = null) : void 
		{
			moveRadius();
		}

		private function moveRadius():void
		{
			if (radius == 0 || !radiusVisible)
			{
				circle.clear();
				return;
			}

            circle.moveTo(parentObj.primaryPosition);
		}
	}

}
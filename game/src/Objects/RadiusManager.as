package src.Objects 
{
    import flash.events.Event;

    import src.Global;
    import src.UI.Components.GroundCircle;

    /**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class RadiusManager
	{
		private var radiusVisible: Boolean = false;
		private var parentObj: SimpleGameObject;
		private var circle: GroundCircle;
		private var radius: int;
		
		public function RadiusManager(parentObj: SimpleGameObject) 
		{
			this.parentObj = parentObj;
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
				if (circle)				
					Global.map.objContainer.removeObject(circle, ObjectContainer.LOWER);
					
				circle = null;				
				return;
			}

			if (!circle)
			{
				circle = new GroundCircle(radius, true);
				circle.alpha = 0.6;
			}
			else
				Global.map.objContainer.removeObject(circle, ObjectContainer.LOWER);

			circle.objX = parentObj.objX;
			circle.objY = parentObj.objY;

			Global.map.objContainer.addObject(circle, ObjectContainer.LOWER);
		}		
	}

}
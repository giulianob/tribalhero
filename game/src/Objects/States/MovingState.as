package src.Objects.States 
{
	import flash.display.DisplayObject;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class MovingState extends GameObjectState
	{
		public var destX: int;
		public var destY: int;
		
		public function MovingState(x: int, y: int) 
		{
			this.destX = x;
			this.destY = y;
		}
		
		override public function getStateType():int 
		{
			return SimpleGameObject.STATE_MOVING;
		}
		
	}

}
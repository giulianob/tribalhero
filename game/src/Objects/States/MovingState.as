package src.Objects.States 
{
	import flash.display.DisplayObject;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;

	public class MovingState extends GameObjectState
	{	
		public function MovingState() 
		{
		}
		
		override public function getStateType():int
		{
			return SimpleGameObject.STATE_MOVING;
		}
	}
}
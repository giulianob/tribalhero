package src.Objects.States 
{
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
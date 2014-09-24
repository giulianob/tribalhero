package src.Objects.States
{
    import starling.display.*;

    public class GameObjectState
	{

		public function GameObjectState()
		{

		}
		
		public function getStateType() : int {
			return 0;
		}
		
		public function getStateIcon() : DisplayObject {
			return null;
		}		
		
		public function equals(other: GameObjectState): Boolean {
			return false;
		}
	}

}

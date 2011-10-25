package src.Objects.States
{
	import flash.display.DisplayObject;
	import org.aswing.AssetIcon;
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
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
	}

}

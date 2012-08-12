package src.Objects.States 
{
	import flash.display.DisplayObject;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class BattleState extends GameObjectState
	{
		public var battleId: int;
		
		public function BattleState(battleId: int) 
		{
			this.battleId = battleId;
		}
		
		override public function getStateType():int 
		{
			return SimpleGameObject.STATE_BATTLE;
		}
		
		override public function getStateIcon():DisplayObject 
		{
			return new ICON_BATTLE();
		}
		
	}

}
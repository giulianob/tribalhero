package src.Objects.States 
{
    import src.Objects.Factories.SpriteFactory;
    import src.StarlingStage;

    import starling.display.DisplayObject;

    import src.Objects.SimpleGameObject;

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
			return SpriteFactory.getStarlingImage("ICON_BATTLE");
		}
		
		override public function equals(other: GameObjectState): Boolean {
			return other is BattleState && (other as BattleState).battleId === battleId;
		}
	}

}
package src.Objects.Battle {
    import flash.events.Event;

    /**
	* ...
	* @author Default
	*/
	public class BattleRoundEvent extends Event
	{
		
		public var round: int;
		
		public function BattleRoundEvent(type: String, round: int) {
			super(type);
			
			this.round = round;
		}
		
	}
	
}
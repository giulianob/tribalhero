package src.Objects 
{
    import src.Constants;

	public class AggressiveLazyValue extends LazyValue
	{
		
		public function AggressiveLazyValue(value: int, rate: int, upkeep: int, limit: int, lastRealizeTime: int)
		{
			super(value, rate, upkeep, limit, lastRealizeTime);
		}		
		
		protected override function getCalculatedRate(): Number {
            if ((rate - upkeep) == 0) {
                return 0;
            }

			return (3600.0 / (getRate() - getUpkeep())) * Constants.secondsPerUnit;
		}
		
	}

}
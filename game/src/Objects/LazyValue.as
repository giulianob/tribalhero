package src.Objects
{
    import src.Constants;
    import src.Global;

    public class LazyValue
	{
		private var value: int;
		protected var rate: int;
		protected var upkeep: int;
		private var limit: int;
		private var lastRealizeTime: int;

		public function LazyValue(value: int, rate: int, upkeep: int, limit: int, lastRealizeTime: int)
		{
			this.value = value;
			this.limit = limit;
			this.upkeep = upkeep;
			this.rate = rate;
			this.lastRealizeTime = lastRealizeTime;
		}

		public function getLimit(): int
		{
			return limit;
		}

		public function getRate() :int
		{
			return rate;
		}

		public function getUpkeep() : int
		{
			return upkeep;
		}

		public function getRawValue() :int
		{
			return value;
		}

		public function getValue(): int
		{
			var delta: int = 0;
			var calculatedRate: Number = getCalculatedRate();

			if (calculatedRate != 0) {
				var elapsed: int = Global.map.getServerTime() - lastRealizeTime;
				delta = int(elapsed / calculatedRate);
			}

			if (limit > 0 && (value + delta) > limit) {
				return limit;
			}

			return Math.min(99999, Math.max(0, value + delta));
		}

		protected function getCalculatedRate(): Number {
			if ((rate - upkeep) <= 0) {
                return 0;
            }

			return Math.max(0, (3600.0 / (rate - upkeep)) * Constants.secondsPerUnit);
		}

		public function getHourlyRate(): int
		{
			return LazyResources.getHourlyRate(this.getRate());
		}
		
		public function getHourlyUpkeep(): int
		{
			return LazyResources.getHourlyRate(this.getUpkeep());
		}		
	}

}

